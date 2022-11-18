using System;
using DanonsTools.ServiceLayer;
using Svelto.DataStructures;
using Svelto.ECS;

namespace DanonsTools.Ecs
{
    public sealed class EcsWorld : IDisposable
    {
        public IEntityFactory EntityFactory => _compositionRoot.EntityFactory;
        
        private readonly EcsCompositionRoot _compositionRoot;
        
        public EcsWorld(ref Action<float> submitEntitiesAction)
        {
            if (ServiceLocator.Retrieve<ICompositionDisposerService>() == null)
                ServiceLocator.Bind<ICompositionDisposerService, DefaultCompositionDisposerService>(new DefaultCompositionDisposerService());

            _compositionRoot = new EcsCompositionRoot();
            _compositionRoot.OnContextCreated(this);
            _compositionRoot.OnContextInitialized(this);
            
            submitEntitiesAction += _ => _compositionRoot.SubmissionScheduler.SubmitEntities();
        }

        public void AddSystemGroup(in EcsSystem[] systems, ref Action<float> tickAction)
        {
            var stepEngines = new FasterList<IStepEngine<float>>();

            foreach (var system in systems)
                stepEngines.Add(system);
            
            var engineGroup = new StepEngineGroup(stepEngines, ref tickAction);
            
            _compositionRoot.EnginesRoot.AddEngine(engineGroup);
            
            foreach (var system in systems)
                _compositionRoot.EnginesRoot.AddEngine(system);
        }

        public void Dispose()
        {
            _compositionRoot.OnContextDestroyed(true);
            _compositionRoot?.Dispose();
        }
    }
}