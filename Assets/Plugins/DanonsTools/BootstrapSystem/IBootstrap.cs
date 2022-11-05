using Cysharp.Threading.Tasks;
using DanonsTools.ModuleSystem;

namespace DanonsTools.BootstrapSystem
{
    public interface IBootstrap
    {
        public void InjectDependencies();
        public UniTask Load(IModuleLoader moduleLoader);
    }
}