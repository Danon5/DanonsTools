using System;
using DanonsTools.ModuleLayer;

namespace DanonsTools.ServiceLayer
{
    public static class ServiceUtilities
    {
        public static T GetModule<T>() where T : IModule
        {
            if (ServiceLocator.Retrieve<IModuleService>().ModuleLoader.TryGetModule<T>(out var module))
                return module;
            throw new Exception($"Module {module.GetType()} not loaded.");
        }
    }
}