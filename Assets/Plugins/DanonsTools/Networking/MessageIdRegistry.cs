using System;
using System.Collections.Generic;
using System.Linq;

namespace DanonsTools.Networking
{
    public sealed class MessageIdRegistry
    {
        private readonly Dictionary<Type, ushort> _ids = new();
        private readonly Dictionary<ushort, Type> _types = new();

        private ushort _nextId;

        public void RegisterMessage(in Type type)
        {
            if (_ids.ContainsKey(type))
                throw new Exception("Attempting to register the same message multiple times.");
            var id = _nextId++;
            _ids.Add(type, id);
            _types.Add(id, type);
        }

        public void RegisterMessage<T>() where T : INetworkMessage
        {
            RegisterMessage(typeof(T));
        }
        
        public bool TryGetId(in Type type, out ushort id)
        {
            return _ids.TryGetValue(type, out id);
        }

        public bool TryGetId<T>(out ushort id)
        {
            return TryGetId(typeof(T), out id);
        }

        public bool TryGetType(in ushort id, out Type type)
        {
            return _types.TryGetValue(id, out type);
        }

        public bool ContainsType(in Type type)
        {
            return _ids.ContainsKey(type);
        }

        public void RegisterAllMessagesViaReflection(string assemblyName)
        {
            var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var sharedAssembly = domainAssemblies.SingleOrDefault(a => a.GetName().Name == assemblyName);
            var messageInterfaceType = typeof(INetworkMessage);

            if (sharedAssembly == null) return;
            
            var messageTypes = sharedAssembly.GetTypes().Where(t => t.IsValueType && messageInterfaceType.IsAssignableFrom(t));
                
            foreach (var messageType in messageTypes)
                RegisterMessage(messageType);
        }
    }
}