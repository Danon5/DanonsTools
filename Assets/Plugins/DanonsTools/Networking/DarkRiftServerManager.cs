using System;
using System.Collections.Generic;
using DanonsTools.EventLayer;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using MessagePack;
using MessagePack.Resolvers;

namespace DanonsTools.Networking
{
    public sealed class DarkRiftServerManager
    {
        public Action<Exception> ServerStartedEvent { get; set; }
        public Action<Exception> ServerStoppedEvent { get; set; }
        public Action<int> ProfiledOutgoingMessageEvent { get; set; }
        public Action<int> ProfiledIncomingMessageEvent { get; set; }
        public DarkRiftServer Server { get; private set; }
        public MessageIdRegistry MessageIdRegistry { get; }
        public ServerMessageEventBus MessageEventBus { get; }
        public bool Active { get; private set; }
        public bool IsLocalServer { get; private set; }
        public bool NetworkProfilingEnabled { get; set; }

        private readonly XmlUnityServer _unityServer;
        private readonly List<IClient> _clients = new();
        private readonly EventBus _globalEventBus;
        private readonly MessagePackSerializerOptions _options;

        public DarkRiftServerManager(in XmlUnityServer unityServer, in EventBus globalEventBus)
        {
            _unityServer = unityServer;
            _globalEventBus = globalEventBus;
            MessageIdRegistry = new MessageIdRegistry();
            MessageEventBus = new ServerMessageEventBus();
            
            var resolver = CompositeResolver.Create(
                MessagePack.Unity.Extension.UnityBlitResolver.Instance,
                MessagePack.Unity.UnityResolver.Instance,
                StaticCompositeResolver.Instance,
                StandardResolver.Instance);

            _options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
        }
        
        public void Start(in bool isLocalServer)
        {
            if (Active) return;

            IsLocalServer = isLocalServer;
            
            _unityServer.Create();
            Server = _unityServer.Server;
            Active = true;

            Server.ClientManager.ClientConnected += OnClientConnected;
            Server.ClientManager.ClientDisconnected += OnClientDisconnected;
            
            ServerStartedEvent?.Invoke(null);
        }

        public void Stop()
        {
            if (!Active) return;

            _unityServer.Close();
            Server = null;
            Active = false;
            
            ServerStoppedEvent?.Invoke(null);
        }

        public void SendMessage<T>(in IClient client, in T message, in SendMode sendMode) where T : INetworkMessage
        {
            if (!Active)
                throw new Exception("Cannot send message with no active server.");

            if (!MessageIdRegistry.TryGetId(message.GetType(), out var id))
                throw new Exception($"Attempting to send unregistered message type {message.GetType()}");

            using var writer = DarkRiftWriter.Create();
            
            var serializedMessage = MessagePackSerializer.Serialize(message.GetType(), message, _options);
            writer.Write(serializedMessage.Length);
            writer.WriteRaw(serializedMessage, 0, serializedMessage.Length);

            using var packedMessage = Message.Create(id, writer);

            if (IsLocalServer && ReferenceEquals(client, _clients[0]))
                _globalEventBus.Invoke(new LocalMessageFromServerReceivedEvent
                {
                    UnpackedMessage = message
                });
            else
                client.SendMessage(packedMessage, sendMode);
            
            if (NetworkProfilingEnabled)
                ProfiledOutgoingMessageEvent?.Invoke(packedMessage.DataLength);
        }

        public void SendMessageToAll<T>(in T message, in SendMode sendMode) where T : INetworkMessage
        {
            foreach (var client in _clients)
                SendMessage(client, message, sendMode);
        }
        
        public void SendMessageToAllExcept<T>(in IClient exemptClient, in T message, in SendMode sendMode) where T : INetworkMessage
        {
            foreach (var client in _clients)
            {
                if (ReferenceEquals(client, exemptClient)) continue;
                SendMessage(client, message, sendMode);
            }
        }

        private void OnClientConnected(object sender, ClientConnectedEventArgs clientConnectedEventArgs)
        {
            _clients.Add(clientConnectedEventArgs.Client);
            clientConnectedEventArgs.Client.MessageReceived += OnMessageReceived;
        }

        private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs clientDisconnectedEventArgs)
        {
            _clients.Remove(clientDisconnectedEventArgs.Client);
            clientDisconnectedEventArgs.Client.MessageReceived -= OnMessageReceived;
        }

        public void OnLocalMessageFromClientReceived(IEvent @event)
        {
            if (_clients.Count == 0)
                return;
            
            var eventData = (LocalMessageFromClientReceivedEvent)@event;
            var localClient = _clients[0];

            if (NetworkProfilingEnabled)
            {
                using var writer = DarkRiftWriter.Create();
            
                var serializedMessage = MessagePackSerializer.Serialize(eventData.UnpackedMessage.GetType(), eventData.UnpackedMessage, _options);
                writer.Write(serializedMessage.Length);
                writer.WriteRaw(serializedMessage, 0, serializedMessage.Length);

                using var packedMessage = Message.Create(0, writer);
                
                ProfiledIncomingMessageEvent?.Invoke(packedMessage.DataLength);
            }

            HandleUnpackedMessage(localClient, eventData.UnpackedMessage);
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using var packedMessage = e.GetMessage();
            
            if (NetworkProfilingEnabled)
                ProfiledIncomingMessageEvent?.Invoke(packedMessage.DataLength);
            
            if (!MessageIdRegistry.TryGetType(packedMessage.Tag, out var type))
                throw new Exception($"Receiving unregistered message type {type}");
            
            using var reader = packedMessage.GetReader();

            var unpackedMessage = (INetworkMessage)MessagePackSerializer.Deserialize(type, reader.ReadBytes(), _options);
            
            HandleUnpackedMessage(sender as IClient, unpackedMessage);
        }

        private void HandleUnpackedMessage(in IClient sender, in INetworkMessage unpackedMessage)
        {
            MessageEventBus.Invoke(sender, unpackedMessage);
        }
    }
}