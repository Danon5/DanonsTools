using System;
using System.Collections.Generic;
using DanonsTools.EventLayer;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;

namespace DanonsTools.Networking
{
    public sealed class DarkRiftServerManager
    {
        public Action<Exception> ServerStartedEvent { get; set; }
        public Action<Exception> ServerStoppedEvent { get; set; }
        public DarkRiftServer Server { get; private set; }
        public MessageIdRegistry MessageIdRegistry { get; }
        public ServerMessageEventBus MessageEventBus { get; }
        public bool Active { get; private set; }
        public bool IsLocalServer { get; private set; }

        private readonly XmlUnityServer _unityServer;
        private readonly List<IClient> _clients = new();
        private readonly IGlobalEventService _globalEventService;

        public DarkRiftServerManager(in XmlUnityServer unityServer, in IGlobalEventService globalEventService)
        {
            _unityServer = unityServer;
            _globalEventService = globalEventService;
            MessageIdRegistry = new MessageIdRegistry();
            MessageEventBus = new ServerMessageEventBus();
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

        public void SendMessageLocal(in INetworkMessage message)
        {
            if (!Active)
                throw new Exception("Cannot send message with no active server.");

            if (!MessageIdRegistry.TryGetId(message.GetType(), out var id))
                throw new Exception($"Attempting to send unregistered message type {message.GetType()}");

            using var packedMessage = Message.Create(id, message);
            
            _globalEventService.EventBus.Invoke(new LocalMessageFromServerReceivedEvent(packedMessage));
        }
        
        public void SendMessage(in IClient client, in INetworkMessage message, in SendMode sendMode)
        {
            if (!Active)
                throw new Exception("Cannot send message with no active server.");

            if (!MessageIdRegistry.TryGetId(message.GetType(), out var id))
                throw new Exception($"Attempting to send unregistered message type {message.GetType()}");
            
            using var packagedMessage = Message.Create(id, message);
            
            if (IsLocalServer && ReferenceEquals(client, _clients[0]))
                SendMessageLocal(message);
            else
                client.SendMessage(packagedMessage, sendMode);
        }

        public void SendMessageToAll(in INetworkMessage message, in SendMode sendMode)
        {
            foreach (var client in _clients)
                SendMessage(client, message, sendMode);
        }
        
        public void SendMessageToAllExcept(in IClient exemptClient, in INetworkMessage message, in SendMode sendMode)
        {
            foreach (var client in _clients)
            {
                if (ReferenceEquals(client, exemptClient)) continue;
                SendMessage(client, message, sendMode);
            }
        }

        public void OnLocalMessageFromClientReceived(IEvent @event)
        {
            var eventData = (LocalMessageFromClientReceivedEvent)@event;
            var localClient = _clients[0];
            
            HandleMessage(localClient, eventData.Message);
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

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using var packedMessage = e.GetMessage();
            
            HandleMessage(sender as IClient, packedMessage);
        }

        private void HandleMessage(in IClient sender, in Message packedMessage)
        {
            if (!MessageIdRegistry.TryGetType(packedMessage.Tag, out var type))
                throw new Exception($"Receiving unregistered message type {type}");
            
            MessageEventBus.Invoke(sender, packedMessage, type);
        }
    }
}