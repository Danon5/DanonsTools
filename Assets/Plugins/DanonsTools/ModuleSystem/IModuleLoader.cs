using Cysharp.Threading.Tasks;

namespace DanonsTools.ModuleSystem
{
    public interface IModuleLoader
    {
        public UniTask LoadModuleAsync<T>() where T : IModule, new();
        public UniTask UnloadModuleAsync<T>() where T : IModule, new();
        public bool TryGetModule<T>(out T module) where T : IModule, new();
    }
}