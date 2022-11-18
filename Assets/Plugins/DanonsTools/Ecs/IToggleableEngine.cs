using Svelto.ECS;

namespace DanonsTools.Ecs
{
    public interface IToggleableEngine : IEngine
    {
        public bool enabled { get; set; }
        
        public void OnEnabled();
        public void OnDisabled();
    }
}