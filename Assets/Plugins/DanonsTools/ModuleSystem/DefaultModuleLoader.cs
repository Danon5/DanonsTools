using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace DanonsTools.ModuleSystem
{
    public sealed class DefaultModuleLoader : IModuleLoader
    {
        private readonly Dictionary<Type, IModule> _loadedModules = new Dictionary<Type, IModule>();

        public async UniTask<T> LoadModuleAsync<T>() where T : IModule, new()
        {
            var type = typeof(T);

            if (_loadedModules.ContainsKey(type))
                throw new Exception($"Cannot load already loaded module {type.Name}.");
            
            var module = new T();

            await module.LoadAsync();
            
            _loadedModules.Add(type, module);

            return module;
        }

        public async UniTask UnloadModuleAsync<T>() where T : IModule, new()
        {
            var type = typeof(T);

            if (!_loadedModules.ContainsKey(type))
                throw new Exception($"Cannot unload already unloaded module {type.Name}.");

            var module = _loadedModules[type];

            await module.UnloadAsync();

            _loadedModules.Remove(type);
        }

        public bool TryGetModule<T>(out T module) where T : IModule, new()
        {
            var type = typeof(T);
            
            if (_loadedModules.ContainsKey(type))
            {
                module = (T)_loadedModules[type];
                return true;
            }
            
            module = default;
            return false;
        }
    }
}