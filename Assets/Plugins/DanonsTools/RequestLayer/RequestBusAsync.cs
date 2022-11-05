using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace DanonsTools.RequestLayer
{
    public sealed class RequestBusAsync
    {
        private readonly Dictionary<Type, IAsyncRequestHandler> _bus = new();

        public void RegisterRequestHandler<T>(in IAsyncRequestHandler requestHandler) where T : IAsyncRequest
        {
            var type = typeof(T);

            if (_bus.ContainsKey(type))
                throw new Exception("Cannot register multiple request handlers for the same request.");

            _bus.Add(type, requestHandler);
        }

        public void DeregisterRequestHandler<T>() where T : IAsyncRequest
        {
            var type = typeof(T);

            if (!_bus.ContainsKey(type))
                throw new Exception("Cannot deregister a request handler that is not registered.");
            
            _bus.Remove(type);
        }

        public async UniTask<IRequestResult> ProcessRequestAsync<T>(T request) where T : IAsyncRequest
        {
            var type = typeof(T);
            
            if (!_bus.ContainsKey(type))
                throw new Exception("Cannot process a request that is not registered.");
            
            return await _bus[type].Handle(request);
        }
    }
}