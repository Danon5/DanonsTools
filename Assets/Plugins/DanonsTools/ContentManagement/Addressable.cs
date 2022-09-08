using Cysharp.Threading.Tasks;
using DanonsTools.AsyncLoading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DanonsTools.ContentManagement
{
    public sealed class Addressable : ILoadableAsync<Object>
    {
        public string Address { get; set; }
        public Object LoadedObject { get; private set; }

        public async UniTask<Object> LoadAsync()
        {
            return LoadedObject = await Addressables.LoadAssetAsync<Object>(Address).ToUniTask();
        }
    }
}