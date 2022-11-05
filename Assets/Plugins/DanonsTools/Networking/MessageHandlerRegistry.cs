using System;
using System.Collections.Generic;

namespace DanonsTools.Networking
{
    public class MessageHandlerRegistry<T> where T : IMessageHandler
    {
        private readonly Dictionary<ushort, T> _handlers = new();

        public void RegisterHandler(in ushort id, in T handler)
        {
            if (_handlers.ContainsKey(id))
                throw new Exception("Attempting to register the same handler multiple times.");
            _handlers.Add(id, handler);
        }

        public void DeregisterHandler(in ushort id)
        {
            if (!_handlers.ContainsKey(id))
                throw new Exception("Attempting to deregister a handler that is not registered.");
            _handlers.Remove(id);
        }
        
        public bool TryGetHandler(ushort id, out T handler)
        {
            return _handlers.TryGetValue(id, out handler);
        }
    }
}