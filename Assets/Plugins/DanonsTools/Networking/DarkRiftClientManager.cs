using System;
using System.Net;
using Cysharp.Threading.Tasks;
using DanonsTools.EventLayer;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

namespace DanonsTools.Networking
{
    public sealed class DarkRiftClientManager
    {
        public Action<Exception> ConnectedEvent { get; set; }
        public Action<Exception>  DisconnectedEvent { get; set; }
        public MessageIdRegistry MessageIdRegistry { get; }
        public ClientMessageEventBus MessageEventBus { get; }
        public UnityClient Client { get; }
        public ConnectionState ConnectionState => Client.ConnectionState;
        public bool IsHosting { get; private set; }

        private readonly IGlobalEventService _globalEventService;
        
        public DarkRiftClientManager(in UnityClient unityClient, in IGlobalEventService globalEventService)
        {
            _globalEventService = globalEventService;
            Client = unityClient;
            MessageIdRegistry = new MessageIdRegistry();
            MessageEventBus = new ClientMessageEventBus();

            Client.Disconnected += OnDisconnected;
        }

        public async UniTask<bool> ConnectAsync(string ip, ushort port, bool isLocalServer)
        {
            if (ConnectionState == ConnectionState.Connecting) return false;

            Client.ConnectInBackground(IPAddress.Parse(ip), port, false, OnConnected);

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

        public void SendMessageLocal(in INetworkMessage message)
        {
            if (Client.ConnectionState != ConnectionState.Connected)
                throw new Exception("Cannot send message with no active server.");
            
            if (!MessageIdRegistry.TryGetId(message.GetType(), out var id))
                throw new Exception($"Attempting to send unregistered message type {message.GetType()}");

            using var packagedMessage = Message.Create(id, message);
            
            _globalEventService.EventBus.Invoke(new LocalMessageFromClientReceivedEvent(packagedMessage));
        }

        public void SendMessage(in INetworkMessage message, in SendMode sendMode)
        {
            if (Client.ConnectionState != ConnectionState.Connected)
                throw new Exception("Cannot send message with no active server.");
            
            if (!MessageIdRegistry.TryGetId(message.GetType(), out var id))
                throw new Exception($"Attempting to send unregistered message type {message.GetType()}");
            
            using var packagedMessage = Message.Create(id, message);
            
            if (IsHosting)
                SendMessageLocal(message);
            else
                Client.SendMessage(packagedMessage, sendMode);
        }

        public void OnLocalMessageFromServerReceived(IEvent @event)
        {
            var eventData = (LocalMessageFromServerReceivedEvent)@event;

            HandleMessage(eventData.Message);
        }
        
        private void OnConnected(Exception exception)
        {
            if (exception != null)
            {
                ConnectedEvent?.Invoke(exception);
                return;
            }

            Client.MessageReceived += OnMessageReceived;
        }

        private void OnDisconnected(object sender, DisconnectedEventArgs disconnectedEventArgs)
        {
            DisconnectedEvent?.Invoke(null);
            
            Client.MessageReceived -= OnMessageReceived;
        }
        
        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using var packedMessage = e.GetMessage();

            HandleMessage(packedMessage);
        }

        private void HandleMessage(in Message packedMessage)
        {
            if (!MessageIdRegistry.TryGetType(packedMessage.Tag, out var type))
                throw new Exception($"Receiving unregistered message type {type}");
            
            MessageEventBus.Invoke(packedMessage, type);
        }
    }
}