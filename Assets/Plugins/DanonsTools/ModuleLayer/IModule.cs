using Cysharp.Threading.Tasks;

namespace DanonsTools.ModuleLayer
{
    public interface IModule
    {
        public UniTask LoadAsync();
        public UniTask UnloadAsync();
    }
}