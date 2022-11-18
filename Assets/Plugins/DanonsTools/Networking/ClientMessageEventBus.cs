using System;
using System.Collections.Generic;
using System.Linq;

namespace DanonsTools.Networking
{
    public sealed class ClientMessageEventBus
    {
        private readonly Dictionary<Type, Action<INetworkMessage>> _bus = new();
        
        public void RegisterEvent(in Type type)
        {
            if (_bus.ContainsKey(type))
                throw new Exception($"Cannot register {type} multiple times.");
            
            _bus.Add(type, default);
        }
        
        public void RegisterEvent<T>() where T : INetworkMessage
        {
            RegisterEvent(typeof(T));
        }

        public void Subscribe<T>(in Action<INetworkMessage> subscriber) where T : INetworkMessage
        {
            var type = typeof(T);

            if (!_bus.ContainsKey(type))
                throw new Exception($"Cannot subscribe to {type} as it is not registered.");
            
            _bus[type] += subscriber;
        }

        public void Unsubscribe<T>(in Action<INetworkMessage> subscriber) where T : INetworkMessage
        {
            var type = typeof(T);

            if (!_bus.ContainsKey(type))
                throw new Exception($"Cannot unsubscribe from {type} as it is not registered.");

            _bus[type] -= subscriber;
        }

        public void Invoke(in INetworkMessage unpackedMessage)
        {
            var messageType = unpackedMessage.GetType();
            
            if (!_bus.ContainsKey(messageType))
                throw new Exception($"Cannot invoke {messageType} as it is not registered.");

            _bus[messageType]?.Invoke(unpackedMessage);
        }

        public bool HasEventRegistered(in Type type)
        {
            return _bus.ContainsKey(type);
        }

        public bool HasEventRegistered<T>() where T : INetworkMessage
        {
            return HasEventRegistered(typeof(T));
        }
        
        public void RegisterAllMessagesViaReflection(string assemblyName)
        {
            var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var sharedAssembly = domainAssemblies.SingleOrDefault(a => a.GetName().Name == assemblyName);
            var messageInterfaceType = typeof(INetworkMessage);

            if (sharedAssembly == null) return;
            
            var messageTypes = sharedAssembly.GetTypes().Where(t => t.IsValueType && messageInterfaceType.IsAssignableFrom(t));
                
            foreach (var messageType in messageTypes)
                RegisterEvent(messageType);
        }
    }
}