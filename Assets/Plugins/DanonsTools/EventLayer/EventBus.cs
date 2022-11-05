using System;
using System.Collections.Generic;

namespace DanonsTools.EventLayer
{
    public sealed class EventBus
    {
        private readonly Dictionary<Type, Action<IEvent>> _bus = new();

        public void RegisterEvent<T>() where T : IEvent
        {
            var type = typeof(T);

            if (_bus.ContainsKey(type)) return;

            _bus.Add(typeof(T), default);
        }

        public void Subscribe<T>(in Action<IEvent> subscriber) where T : IEvent
        {
            var type = typeof(T);

            if (!_bus.ContainsKey(type))
                throw new Exception($"Cannot subscribe to {type} as it is not registered.");
            
            _bus[type] += subscriber;
        }

        public void Unsubscribe<T>(in Action<IEvent> subscriber) where T : IEvent
        {
            var type = typeof(T);

            if (!_bus.ContainsKey(type))
                throw new Exception($"Cannot unsubscribe from {type} as it is not registered.");

            _bus[type] -= subscriber;
        }

        public void Invoke<T>() where T : IEvent
        {
            var type = typeof(T);

            if (!_bus.ContainsKey(type))
                throw new Exception($"Cannot invoke {type} as it is not registered.");
            
            _bus[type]?.Invoke(default);
        }
        
        public void Invoke<T>(in T eventData) where T : IEvent
        {
            var type = typeof(T);

            if (!_bus.ContainsKey(type))
                throw new Exception($"Cannot invoke {type} as it is not registered.");
            
            _bus[type]?.Invoke(eventData);
        }

        public bool HasEventRegistered(in Type type)
        {
            return _bus.ContainsKey(type);
        }

        public bool HasEventRegistered<T>() where T : IEvent
        {
            return HasEventRegistered(typeof(T));
        }
    }
}