using Cysharp.Threading.Tasks;

namespace DanonsTools.ModuleSystem
{
    public sealed class NetworkModule : IModule
    {
        public UniTask LoadAsync()
        {
            return default;
        }

        public UniTask UnloadAsync()
        {
            return default;
        }

        public UniTask SetModuleActiveAsync()
        {
            return default;
        }
    }
}