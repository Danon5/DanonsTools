using Cysharp.Threading.Tasks;
using DanonsTools.ModuleLayer;

namespace DanonsTools.BootstrapLayer
{
    public interface IBootstrap
    {
        public void InjectDependencies();
        public UniTask Load(IModuleLoader moduleLoader);
    }
}