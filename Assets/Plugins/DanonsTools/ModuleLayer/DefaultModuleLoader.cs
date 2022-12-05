using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace DanonsTools.ModuleLayer
{
    public sealed class DefaultModuleLoader : IModuleLoader
    {
        private readonly Dictionary<Type, IModule> _modules = new();
        private readonly List<Type> _loadingModules = new();
        private readonly List<Type> _unloadingModules = new();

        public async UniTask<T> LoadModuleAsync<T>(T module) where T : IModule
        {
            var type = typeof(T);
            
            if (_modules.ContainsKey(type))
                throw new Exception($"Cannot load already loaded module {type.Name}.");

            _modules.Add(type, module);
            
            _loadingModules.Add(type);
            
            await module.LoadAsync();

            _loadingModules.Remove(type);

            return module;
        }

        public async UniTask UnloadModuleAsync<T>() where T : IModule
        {
            var type = typeof(T);

            if (!_modules.ContainsKey(type))
                throw new Exception($"Cannot unload already unloaded module {type.Name}.");

            var module = _modules[type];
            
            _modules.Remove(type);

            _unloadingModules.Add(type);
            
            await module.UnloadAsync();

            _unloadingModules.Remove(type);
        }

        public bool TryGetModule<T>(out T module) where T : IModule
        {
            var type = typeof(T);
            
            if (_modules.ContainsKey(type))
            {
                module = (T)_modules[type];
                return true;
            }
            
            module = default;
            return false;
        }

        public bool ModuleExists<T>()
        {
            return _modules.ContainsKey(typeof(T));
        }

        public bool ModuleIsLoading<T>() where T : IModule
        {
            return _loadingModules.Contains(typeof(T));
        }

        public bool ModuleIsLoaded<T>() where T : IModule
        {
            var type = typeof(T);
            return _modules.ContainsKey(type) && !_loadingModules.Contains(type);
        }
    }
}