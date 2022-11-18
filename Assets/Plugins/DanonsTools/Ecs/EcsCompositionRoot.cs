using System;
using DanonsTools.ServiceLayer;
using Svelto.Context;
using Svelto.ECS;
using Svelto.ECS.Schedulers;

namespace DanonsTools.Ecs
{
    public sealed class EcsCompositionRoot : ICompositionRoot, IDisposable
    {
        public IEntityFactory EntityFactory { get; private set; }
        public SimpleEntitiesSubmissionScheduler SubmissionScheduler { get; private set; }
        public EnginesRoot EnginesRoot { get; private set; }

        public void OnContextCreated<T>(T contextHolder)
        {
            SubmissionScheduler = new SimpleEntitiesSubmissionScheduler();
            EnginesRoot = new EnginesRoot(SubmissionScheduler);
            EntityFactory = EnginesRoot.GenerateEntityFactory();
            
            ServiceLocator.Retrieve<ICompositionDisposerService>().AddPlayModeExitedListener(Dispose);
        }

        public void OnContextInitialized<T>(T contextHolder)
        {
        }

        public void OnContextDestroyed(bool hasBeenInitialised)
        {
            Dispose();
        }

        public void Dispose()
        {
            try
            {
                ServiceLocator.Retrieve<ICompositionDisposerService>().RemovePlayModeExitedListener(Dispose);
                EnginesRoot?.Dispose();
            }
            catch
            {
                // ignored
            }
        }
    }
}