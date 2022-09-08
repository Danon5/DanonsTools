using Cysharp.Threading.Tasks;

namespace DanonsTools.AsyncLoading
{
    public interface ILoadableAsync<T> : ILoadable
    {
        public UniTask<T> LoadAsync();
    }
}