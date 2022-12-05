using System;
using System.Collections.Generic;
using DanonsTools.BuilderLayer;
using DanonsTools.UtilitiesLayer;
using JetBrains.Annotations;
using UnityEngine;

namespace DanonsTools.ECSLayer.UnityLayer.SystemRunnerLayer
{
    public sealed class EcsSystemRunner : MonoBehaviour, IBuildableObject<EcsSystemRunnerBuilder>, IDisposable
    {
        public bool SimulationDependentOnFrameRate
        {
            get => _simulationDependentOnFrameRate;
            set
            {
                Time.maximumDeltaTime = value ? Time.fixedDeltaTime : .3333333f;
                Time.maximumParticleDeltaTime = value ? Time.fixedDeltaTime : .3333333f;
                _simulationDependentOnFrameRate = value;
            }
        }
        public World World { get; private set; }

        private readonly Dictionary<SystemRunTiming, HashSet<IRunSystem>> _runSystems = new Dictionary<SystemRunTiming, HashSet<IRunSystem>>();
        private readonly HashSet<ISystem> _reactSystems = new HashSet<ISystem>();

        private bool _simulationDependentOnFrameRate;
        private float _physicsDelta;

        [UsedImplicitly]
        private void Start()
        {
            World.SetData(new TimeData
            {
                timeScale = Time.timeScale,
                time = Time.time,
                renderDeltaTime = SimulationDependentOnFrameRate ? Time.fixedDeltaTime : Time.deltaTime,
                fixedDeltaTime = Time.fixedDeltaTime,
                unscaledTime = Time.unscaledTime,
                unscaledRenderDeltaTime = Time.unscaledDeltaTime,
                unscaledFixedDeltaTime = Time.fixedUnscaledDeltaTime,
            });
        }

        [UsedImplicitly]
        private void Update()
        {
            ref var timeData = ref World.GetData<TimeData>();

            UpdateTimeData(ref timeData);

            if (_runSystems.TryGetValue(SystemRunTiming.Update, out var updateSystems))
                foreach (var system in updateSystems) system.Run(World);

            if (SimulationDependentOnFrameRate)
            {
                RunPhysicsSystems();
                return;
            }
            
            _physicsDelta += timeData.renderDeltaTime;

            while (_physicsDelta >= timeData.fixedDeltaTime)
            {
                RunPhysicsSystems();
                
                _physicsDelta -= timeData.fixedDeltaTime;
            }
        }

        [UsedImplicitly]
        private void LateUpdate()
        {
            if (_runSystems.TryGetValue(SystemRunTiming.LateUpdate, out var lateUpdateSystems))
                foreach (var system in lateUpdateSystems) system.Run(World);
        }

        public void Dispose()
        {
            _runSystems.Clear();
            _reactSystems.Clear();
        }

        public EcsSystemRunnerBuilder WithBuilder() => new EcsSystemRunnerBuilder(this);

        public void AddSystem(in IRunSystem system, in SystemRunTiming runTiming)
        {
            if (!_runSystems.ContainsKey(runTiming))
                _runSystems.Add(runTiming, new HashSet<IRunSystem> { system });
            else
                _runSystems[runTiming].Add(system);
        }
        
        public void AddSystem<T1>(IReactOnSetSystem<T1> system)
        {
            World.OnSet((Entity entity, T1 oldComp, ref T1 newComp) => system.OnSet(World, entity, oldComp, newComp));
            _reactSystems.Add(system);
        }
        
        public void AddSystem<T1, T2>(IReactOnSetSystem<T1, T2> system)
        {
            World.OnSet((Entity entity, T1 oldComp, ref T1 newComp) => system.OnSet(World, entity, oldComp, newComp));
            World.OnSet((Entity entity, T2 oldComp, ref T2 newComp) => system.OnSet(World, entity, oldComp, newComp));
            _reactSystems.Add(system);
        }
        
        public void AddSystem<T1, T2, T3>(IReactOnSetSystem<T1, T2, T3> system)
        {
            World.OnSet((Entity entity, T1 oldComp, ref T1 newComp) => system.OnSet(World, entity, oldComp, newComp));
            World.OnSet((Entity entity, T2 oldComp, ref T2 newComp) => system.OnSet(World, entity, oldComp, newComp));
            World.OnSet((Entity entity, T3 oldComp, ref T3 newComp) => system.OnSet(World, entity, oldComp, newComp));
            _reactSystems.Add(system);
        }


        public void AddSystem<T1>(IReactOnRemoveSystem<T1> system)
        {
            World.OnRemove((Entity entity, T1 oldComp) => system.OnRemoved(World, entity, oldComp));
            _reactSystems.Add(system);
        }
        
        public void AddSystem<T1, T2>(IReactOnRemove<T1, T2> system)
        {
            World.OnRemove((Entity entity, T1 oldComp) => system.OnRemoved(World, entity, oldComp));
            World.OnRemove((Entity entity, T2 oldComp) => system.OnRemoved(World, entity, oldComp));
            _reactSystems.Add(system);
        }
        
        public void AddSystem<T1, T2, T3>(IReactOnRemove<T1, T2, T3> system)
        {
            World.OnRemove((Entity entity, T1 oldComp) => system.OnRemoved(World, entity, oldComp));
            World.OnRemove((Entity entity, T2 oldComp) => system.OnRemoved(World, entity, oldComp));
            World.OnRemove((Entity entity, T3 oldComp) => system.OnRemoved(World, entity, oldComp));
            _reactSystems.Add(system);
        }

        public void AddSystem<T1>(IReactOnSetSystem<T1> system, in SystemRunTiming runTiming)
        {
            AddSystem(system);
            AddSystem(system as IRunSystem, runTiming);
        }
        
        public void AddSystem<T1, T2>(IReactOnSetSystem<T1, T2> system, in SystemRunTiming runTiming)
        {
            AddSystem(system);
            AddSystem(system as IRunSystem, runTiming);
        }
        
        public void AddSystem<T1, T2, T3>(IReactOnSetSystem<T1, T2, T3> system, in SystemRunTiming runTiming)
        {
            AddSystem(system);
            AddSystem(system as IRunSystem, runTiming);
        }

        
        public void AddSystem<T1>(IReactOnRemoveSystem<T1> system, in SystemRunTiming runTiming)
        {
            AddSystem(system);
            AddSystem(system as IRunSystem, runTiming);
        }
        
        public void AddSystem<T1, T2>(IReactOnRemove<T1, T2> system, in SystemRunTiming runTiming)
        {
            AddSystem(system);
            AddSystem(system as IRunSystem, runTiming);
        }
        
        public void AddSystem<T1, T2, T3>(IReactOnRemove<T1, T2, T3> system, in SystemRunTiming runTiming)
        {
            AddSystem(system);
            AddSystem(system as IRunSystem, runTiming);
        }

        public static EcsSystemRunner Create(in World world)
        {
            var runner = new GameObject("SystemRunner").AddComponent<EcsSystemRunner>();
            runner.World = world;
            return runner;
        }

        public static EcsSystemRunner Create(in World world, in string scene)
        {
            var runner = SceneUtilities.CreateGameObjectInScene(scene, "SystemRunner").AddComponent<EcsSystemRunner>();
            runner.World = world;
            return runner;
        }

        private void RunPhysicsSystems()
        {
            if (_runSystems.TryGetValue(SystemRunTiming.PrePhysics, out var prePhysicsSystems))
                foreach (var system in prePhysicsSystems) system.Run(World);
                
            if (_runSystems.TryGetValue(SystemRunTiming.Physics, out var physicsSystems))
                foreach (var system in physicsSystems) system.Run(World);
                
            if (_runSystems.TryGetValue(SystemRunTiming.PostPhysics, out var postPhysicsSystems))
                foreach (var system in postPhysicsSystems) system.Run(World);
        }

        private void UpdateTimeData(ref TimeData timeData)
        {
            Time.timeScale = timeData.timeScale;
            timeData.time = Time.time;
            timeData.renderDeltaTime = SimulationDependentOnFrameRate ? Time.fixedDeltaTime : Time.deltaTime;
            Time.fixedDeltaTime = timeData.fixedDeltaTime;
            timeData.unscaledTime = Time.unscaledTime;
            timeData.unscaledRenderDeltaTime = Time.unscaledDeltaTime;
            timeData.unscaledFixedDeltaTime = Time.fixedUnscaledDeltaTime;
        }
    }
}