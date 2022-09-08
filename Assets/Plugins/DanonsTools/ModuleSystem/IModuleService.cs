using DanonsTools.ServiceLayer;

namespace DanonsTools.ModuleSystem
{
    public interface IModuleService
    {
        public IModuleLoader ModuleLoader { get; }
    }
}