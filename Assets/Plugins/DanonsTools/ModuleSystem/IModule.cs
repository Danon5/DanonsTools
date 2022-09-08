using Cysharp.Threading.Tasks;

namespace DanonsTools.ModuleSystem
{
    public interface IModule
    {
        public UniTask LoadAsync();
        public UniTask UnloadAsync();
        public UniTask SetModuleActiveAsync();
    }
}