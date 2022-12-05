namespace DanonsTools.ModuleLayer
{
    public sealed class DefaultModuleService : IModuleService
    {
        public IModuleLoader ModuleLoader { get; }

        public DefaultModuleService(in IModuleLoader moduleLoader)
        {
            ModuleLoader = moduleLoader;
        }
    }
}