using System;
using System.Collections.Generic;

namespace DanonsTools.RequestLayer
{
    public sealed class RequestBus
    {
        private readonly Dictionary<Type, IRequestHandler> _bus = new();

        public void RegisterRequestHandler<T>(in IRequestHandler requestHandler) where T : IRequest
        {
            var type = typeof(T);

            if (_bus.ContainsKey(type))
                throw new Exception("Cannot register multiple request handlers for the same request.");

            _bus.Add(type, requestHandler);
        }

        public void DeregisterRequestHandler<T>() where T : IRequest
        {
            var type = typeof(T);

            if (!_bus.ContainsKey(type))
                throw new Exception("Cannot deregister a request handler that is not registered.");
            
            _bus.Remove(type);
        }

        public IRequestResult ProcessRequest<T>(T request) where T : IRequest
        {
            var type = typeof(T);
            
            if (!_bus.ContainsKey(type))
                throw new Exception("Cannot process a request that is not registered.");

            return _bus[type].Handle(request);
        }
    }
}