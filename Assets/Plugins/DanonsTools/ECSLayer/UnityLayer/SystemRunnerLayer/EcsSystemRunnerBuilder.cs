using DanonsTools.BuilderLayer;

namespace DanonsTools.ECSLayer.UnityLayer.SystemRunnerLayer
{
    public readonly struct EcsSystemRunnerBuilder : IObjectBuilder<EcsSystemRunner>
    {
        internal readonly EcsSystemRunner systemRunner;
        
        public EcsSystemRunnerBuilder(in EcsSystemRunner systemRunner)
        {
            this.systemRunner = systemRunner;
        }
        
        public EcsSystemRunnerBuilder WithSimulationDependentOnFrameRate(in bool value)
        {
            systemRunner.SimulationDependentOnFrameRate = value;
            return this;
        }

        public EcsSystemRunnerBuilder WithSystem(in IRunSystem system, in SystemRunTiming runTiming)
        {
            systemRunner.AddSystem(system, runTiming);
            return this;
        }

        public EcsSystemRunnerBuilder WithReactSystem<T1>(in IReactOnSetSystem<T1> system)
        {
            systemRunner.AddSystem(system);
            return this;
        }
        
        public EcsSystemRunnerBuilder WithReactSystem<T1, T2>(in IReactOnSetSystem<T1, T2> system)
        {
            systemRunner.AddSystem(system);
            return this;
        }
        
        public EcsSystemRunnerBuilder WithReactSystem<T1, T2, T3>(in IReactOnSetSystem<T1, T2, T3> system)
        {
            systemRunner.AddSystem(system);
            return this;
        }

        public EcsSystemRunnerBuilder WithReactSystem<T1>(in IReactOnRemoveSystem<T1> system)
        {
            systemRunner.AddSystem(system);
            return this;
        }
        
        public EcsSystemRunnerBuilder WithReactSystem<T1, T2>(in IReactOnRemove<T1, T2> system)
        {
            systemRunner.AddSystem(system);
            return this;
        }
        
        public EcsSystemRunnerBuilder WithReactSystem<T1, T2, T3>(in IReactOnRemove<T1, T2, T3> system)
        {
            systemRunner.AddSystem(system);
            return this;
        }
        
        public EcsSystemRunnerBuilder WithReactSystem<T1>(in IReactOnSetSystem<T1> system, in SystemRunTiming runTiming)
        {
            systemRunner.AddSystem(system, runTiming);
            return this;
        }
        
        public EcsSystemRunnerBuilder WithReactSystem<T1, T2>(in IReactOnSetSystem<T1, T2> system, in SystemRunTiming runTiming)
        {
            systemRunner.AddSystem(system, runTiming);
            return this;
        }
        
        public EcsSystemRunnerBuilder WithReactSystem<T1, T2, T3>(in IReactOnSetSystem<T1, T2, T3> system, in SystemRunTiming runTiming)
        {
            systemRunner.AddSystem(system, runTiming);
            return this;
        }

        public EcsSystemRunnerBuilder WithReactSystem<T1>(in IReactOnRemoveSystem<T1> system, in SystemRunTiming runTiming)
        {
            systemRunner.AddSystem(system, runTiming);
            return this;
        }
        
        public EcsSystemRunnerBuilder WithReactSystem<T1, T2>(in IReactOnRemove<T1, T2> system, in SystemRunTiming runTiming)
        {
            systemRunner.AddSystem(system, runTiming);
            return this;
        }
        
        public EcsSystemRunnerBuilder WithReactSystem<T1, T2, T3>(in IReactOnRemove<T1, T2, T3> system, in SystemRunTiming runTiming)
        {
            systemRunner.AddSystem(system, runTiming);
            return this;
        }

        public EcsSystemRunner Build()
        {
            return systemRunner;
        }
    }
}