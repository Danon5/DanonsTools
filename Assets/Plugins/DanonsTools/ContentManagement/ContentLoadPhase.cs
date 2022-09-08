using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DanonsTools.AsyncLoading;

namespace DanonsTools.ContentManagement
{
    public sealed class ContentLoadPhase : ILoaderAsync<Addressable>, ILoadable
    {
        public int LoadIndex { get; private set; }
        public int LoadLength => _addressableQueue.Count;
        public IEnumerable<Addressable> LoadQueue => _addressableQueue;
        public string Name { get; set; }
        public Addressable CurrentlyLoadingAddressable { get; set; }

        private readonly Queue<Addressable> _addressableQueue = new Queue<Addressable>();

        public ContentLoadPhase(in string name, in IEnumerable<Addressable> addressables)
        {
            Name = name;
            EnqueueLoadables(addressables);
        }
        
        public ContentLoadPhase(in string name, in Type contentAddressContainer)
        {
            Name = name;
            EnqueueLoadables(ContentLoadUtils.GetAssetAddressesInType(contentAddressContainer).ToAddressables());
        }

        public void EnqueueLoadable(in Addressable loadable)
        {
            _addressableQueue.Enqueue(loadable);
        }

        public void EnqueueLoadables(in IEnumerable<Addressable> loadables)
        {
            foreach (var loadable in loadables)
                _addressableQueue.Enqueue(loadable);
        }

        public async UniTask ProcessQueue()
        {
            LoadIndex = 0;
            foreach (var addressable in _addressableQueue)
            {
                CurrentlyLoadingAddressable = addressable;
                await addressable.LoadAsync();
                LoadIndex++;
            }
        }

        public IUniTaskAsyncEnumerable<Addressable> ProcessQueueEnumerable()
        {
            return UniTaskAsyncEnumerable.Create<Addressable>(async (writer, token) =>
            {
                LoadIndex = 0;
                foreach (var addressable in _addressableQueue)
                {
                    CurrentlyLoadingAddressable = addressable;
                    await addressable.LoadAsync();
                    await writer.YieldAsync(addressable);
                    LoadIndex++;
                }
            });
        }
    }
}