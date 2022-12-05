using Cysharp.Threading.Tasks;
using DanonsTools.ModuleLayer;
using UnityEngine;

namespace DanonsTools.BootstrapLayer
{
    public abstract class ScriptableBootstrap : ScriptableObject, IBootstrap
    {
        public abstract void InjectDependencies();
        public abstract UniTask Load(IModuleLoader moduleLoader);
    }
}