using Cysharp.Threading.Tasks;

namespace DanonsTools.ModuleLayer
{
    public interface IModuleLoader
    {
        public UniTask<T> LoadModuleAsync<T>(T module) where T : IModule;
        public UniTask UnloadModuleAsync<T>() where T : IModule;
        public bool ModuleExists<T>();
        public bool TryGetModule<T>(out T module) where T : IModule;
        public bool ModuleIsLoading<T>() where T : IModule;
        public bool ModuleIsLoaded<T>() where T : IModule;
    }
}