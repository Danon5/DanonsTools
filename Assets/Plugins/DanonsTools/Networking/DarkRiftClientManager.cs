using System;
using System.Net;
using Cysharp.Threading.Tasks;
using DanonsTools.EventLayer;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using MessagePack;
using MessagePack.Resolvers;
using Random = UnityEngine.Random;

namespace DanonsTools.Networking
{
    public sealed class DarkRiftClientManager
    {
        public Action<Exception> ConnectedEvent { get; set; }
        public Action<Exception>  DisconnectedEvent { get; set; }
        public Action<int> ProfiledOutgoingMessageEvent { get; set; }
        public Action<int> ProfiledIncomingMessageEvent { get; set; }
        public MessageIdRegistry MessageIdRegistry { get; }
        public ClientMessageEventBus MessageEventBus { get; }
        public UnityClient Client { get; }
        public ConnectionState ConnectionState => Client.ConnectionState;
        public bool IsHosting { get; private set; }
        public bool NetworkProfilingEnabled { get; set; }
        public bool LatencyEmulationEnabled { get; set; }
        public bool PacketLossEmulationEnabled { get; set; }

        private readonly EventBus _globalEventBus;
        private readonly MessagePackSerializerOptions _options;

        private int _latencyMs;
        private float _packetLossChance;
        
        public DarkRiftClientManager(in UnityClient unityClient, in EventBus globalEventBus)
        {
            _globalEventBus = globalEventBus;
            Client = unityClient;
            MessageIdRegistry = new MessageIdRegistry();
            MessageEventBus = new ClientMessageEventBus();

            Client.Disconnected += OnDisconnected;
            
            var resolver = CompositeResolver.Create(
                MessagePack.Unity.Extension.UnityBlitResolver.Instance,
                MessagePack.Unity.UnityResolver.Instance,
                StaticCompositeResolver.Instance,
                StandardResolver.Instance);

            _options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
        }

        public async UniTask<bool> ConnectAsync(string ip, ushort port, bool isLocalServer)
        {
            if (ConnectionState == ConnectionState.Connecting) return false;

            Client.ConnectInBackground(IPAddress.Parse(ip), port, true, OnConnected);

            await UniTask.WaitUntil(() => ConnectionState != ConnectionState.Connecting);
            
            var success = Client.ConnectionState == ConnectionState.Connected;

            if (success)
            {
                ConnectedEvent?.Invoke(null);
                IsHosting = isLocalServer;
            }

            return success;
        }

        public bool TryCancelConnection()
        {
            switch (ConnectionState == ConnectionState.Connecting)
            {
                case true:
                    Client.Disconnect();
                    return true;
                default:
                    return false;
            }
        }

        public void Disconnect()
        {
            if (Client.ConnectionState is ConnectionState.Disconnected or ConnectionState.Disconnecting)
            {
                DisconnectedEvent?.Invoke(new Exception("Cannot disconnect with no active server."));
                return;
            }
            
            Client.Disconnect();
            IsHosting = false;
        }

        public void SendMessage<T>(in T message, in SendMode sendMode) where T : INetworkMessage
        {
            if (PacketLossEmulationEnabled && sendMode == SendMode.Unreliable && Random.Range(0f, 1f) <= _packetLossChance)
                return;

            if (LatencyEmulationEnabled && _latencyMs > 0)
            {
                SendMessageWithLatency(message, sendMode).Forget();
                return;
            }
            
            if (Client.ConnectionState != ConnectionState.Connected)
                throw new Exception("Cannot send message with no active server.");

            if (!MessageIdRegistry.TryGetId(message.GetType(), out var id))
                throw new Exception($"Attempting to send unregistered message type {message.GetType()}");

            using var writer = DarkRiftWriter.Create();

            var serializedMessage = MessagePackSerializer.Serialize(message.GetType(), message, _options);
            writer.Write(serializedMessage.Length);
            writer.WriteRaw(serializedMessage, 0, serializedMessage.Length);

            using var packedMessage = Message.Create(id, writer);

            if (IsHosting)
                _globalEventBus.Invoke(new LocalMessageFromClientReceivedEvent
                {
                    UnpackedMessage = message
                });
            else
                Client.SendMessage(packedMessage, sendMode);
            
            if (NetworkProfilingEnabled)
                ProfiledOutgoingMessageEvent?.Invoke(packedMessage.DataLength);
        }

        public void ConfigureLatencyEmulation(in int latencyMs)
        {
            _latencyMs = latencyMs;
        }

        public void ConfigurePacketLossEmulation(in float packetLossChance)
        {
            _packetLossChance = packetLossChance;
        }

        private async UniTaskVoid SendMessageWithLatency<T>(T message, SendMode sendMode) where T : INetworkMessage
        {
            await UniTask.Delay(_latencyMs);
            
            if (Client.ConnectionState != ConnectionState.Connected)
                throw new Exception("Cannot send message with no active server.");

            if (!MessageIdRegistry.TryGetId(message.GetType(), out var id))
                throw new Exception($"Attempting to send unregistered message type {message.GetType()}");

            using var writer = DarkRiftWriter.Create();

            var serializedMessage = MessagePackSerializer.Serialize(message, _options);
            writer.Write(serializedMessage.Length);
            writer.WriteRaw(serializedMessage, 0, serializedMessage.Length);

            using var packedMessage = Message.Create(id, writer);

            if (IsHosting)
                _globalEventBus.Invoke(new LocalMessageFromClientReceivedEvent
                {
                    UnpackedMessage = message
                });
            else
                Client.SendMessage(packedMessage, sendMode);

            if (NetworkProfilingEnabled)
                ProfiledOutgoingMessageEvent?.Invoke(packedMessage.DataLength);
        }

        private void OnConnected(Exception exception)
        {
            if (exception != null)
            {
                ConnectedEvent?.Invoke(exception);
                return;
            }

            Client.MessageReceived += OnPackedMessageReceived;
        }

        private void OnDisconnected(object sender, DisconnectedEventArgs disconnectedEventArgs)
        {
            DisconnectedEvent?.Invoke(null);
            
            Client.MessageReceived -= OnPackedMessageReceived;
        }

        public void OnLocalPackedMessageReceived(IEvent @event)
        {
            var eventData = (LocalMessageFromServerReceivedEvent)@event;
            var unpackedMessage = eventData.UnpackedMessage;

            if (NetworkProfilingEnabled)
            {
                using var writer = DarkRiftWriter.Create();
            
                var serializedMessage = MessagePackSerializer.Serialize(unpackedMessage.GetType(), unpackedMessage, _options);
                writer.Write(serializedMessage.Length);
                writer.WriteRaw(serializedMessage, 0, serializedMessage.Length);

                using var packedMessage = Message.Create(0, writer);
                
                ProfiledIncomingMessageEvent?.Invoke(packedMessage.DataLength);
            }

            if (LatencyEmulationEnabled && _latencyMs > 0)
            {
                HandleUnpackedMessageWithDelay(unpackedMessage).Forget();
                return;
            }
            
            HandleUnpackedMessage(unpackedMessage);
        }

        private void OnPackedMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using var packedMessage = e.GetMessage();
            
            if (PacketLossEmulationEnabled && e.SendMode == SendMode.Unreliable && Random.Range(0f, 1f) <= _packetLossChance)
                return;

            if (NetworkProfilingEnabled)
                ProfiledIncomingMessageEvent?.Invoke(packedMessage.DataLength);

            if (!MessageIdRegistry.TryGetType(packedMessage.Tag, out var type))
                throw new Exception($"Receiving unregistered message type {type}");
            
            var messageData = (INetworkMessage)MessagePackSerializer.Deserialize(type, packedMessage.GetReader().ReadBytes(), _options);

            if (LatencyEmulationEnabled && _latencyMs > 0)
            {
                HandleUnpackedMessageWithDelay(messageData).Forget();
                return;
            }
            
            HandleUnpackedMessage(messageData);
        }

        private void HandleUnpackedMessage(in INetworkMessage unpackedMessage)
        {
            MessageEventBus.Invoke(unpackedMessage);
        }

        private async UniTaskVoid HandleUnpackedMessageWithDelay(INetworkMessage unpackedMessage)
        {
            await UniTask.Delay(_latencyMs);

            MessageEventBus.Invoke(unpackedMessage);
        }
    }
}