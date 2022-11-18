using Svelto.ECS;

namespace DanonsTools.Ecs
{
    public abstract class EcsSystem : IStepEngine<float>, IQueryingEntitiesEngine, IToggleableEngine, IDisposingEngine
    {
        public string name => nameof(EcsSystem);
        public EntitiesDB entitiesDB { get; set; }
        public bool enabled { get; set; }
        public bool isDisposing { get; set; }

        public virtual void Ready() { }
        public virtual void OnEnabled() { }
        public virtual void Step(in float deltaTime) { }
        public virtual void OnDisabled() { }
        public virtual void Dispose() { }
    }
}