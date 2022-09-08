using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace DanonsTools.ContentManagement
{
    public sealed class DefaultContentLoader : IContentLoader
    {
        public int LoadIndex { get; private set; }
        public int LoadLength => _loadPhases.Count;
        public IEnumerable<ContentLoadPhase> LoadQueue => _loadPhases;

        private readonly Queue<ContentLoadPhase> _loadPhases = new Queue<ContentLoadPhase>();
        private readonly Dictionary<string, Object> _cachedContent = new Dictionary<string, Object>();

        public void EnqueueLoadable(in ContentLoadPhase loadable)
        {
            _loadPhases.Enqueue(loadable);
        }

        public void EnqueueLoadables(in IEnumerable<ContentLoadPhase> loadables)
        {
            foreach (var loadable in loadables)
                _loadPhases.Enqueue(loadable);
        }

        public async UniTask ProcessQueue()
        {
            LoadIndex = 0;
            foreach (var loadPhase in _loadPhases)
            {
                await foreach (var addressable in loadPhase.ProcessQueueEnumerable())
                {
                    await addressable.LoadAsync();
                    _cachedContent.TryAdd(addressable.Address, addressable.LoadedObject);
                }
                LoadIndex++;
            }
        }

        public IUniTaskAsyncEnumerable<ContentLoadPhase> ProcessQueueEnumerable()
        {
            return UniTaskAsyncEnumerable.Create<ContentLoadPhase>(async (writer, token) =>
            {
                LoadIndex = 0;
                foreach (var loadPhase in _loadPhases)
                {
                    await foreach (var addressable in loadPhase.ProcessQueueEnumerable())
                    {
                        await writer.YieldAsync(loadPhase);
                        await addressable.LoadAsync();
                        _cachedContent.TryAdd(addressable.Address, addressable.LoadedObject);
                    }
                    LoadIndex++;
                }
            });
        }

        public async UniTask<T> LoadContent<T>(string address) where T : Object
        {
            var content = _cachedContent.ContainsKey(address) ?
                _cachedContent[address] : 
                await Addressables.LoadAssetAsync<T>(address).ToUniTask();

            _cachedContent.TryAdd(address, content);
            
            return content as T;
        }

        public T GetCachedContent<T>(in string address) where T : Object
        {
            if (!_cachedContent.ContainsKey(address))
                throw new Exception($"Content cache has no content with address: {address}");
            return _cachedContent[address] as T;
        }
    }
}