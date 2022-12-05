using JetBrains.Annotations;

namespace DanonsTools.ECSLayer.UnityLayer.EventLayer.Physics.EventComponents
{
    [UsedImplicitly]
    public struct CollisionExitEventComponent : IEcsEventComponent
    {
        public Entity entity;
        public Entity otherEntity;
    }
}