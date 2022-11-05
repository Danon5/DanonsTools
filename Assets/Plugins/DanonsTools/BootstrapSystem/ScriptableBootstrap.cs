using Cysharp.Threading.Tasks;
using DanonsTools.ModuleSystem;
using UnityEngine;

namespace DanonsTools.BootstrapSystem
{
    public abstract class ScriptableBootstrap : ScriptableObject, IBootstrap
    {
        public abstract void InjectDependencies();
        public abstract UniTask Load(IModuleLoader moduleLoader);
    }
}