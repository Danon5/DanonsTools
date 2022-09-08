using Cysharp.Threading.Tasks;

namespace DanonsTools.AsyncLoading
{
    public interface ILoadableAsyncEnumerable<T> : ILoadable
    {
        public IUniTaskAsyncEnumerable<T> LoadAsyncEnumerable();
    }
}