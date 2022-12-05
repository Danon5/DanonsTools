using System;

namespace DanonsTools.ECSLayer.UnityLayer.EventLayer.Physics.EventComponents
{
    [Serializable]
    public struct TriggerEnterEventComponent : IEcsEventComponent
    {
        public Entity entity;
        public Entity otherEntity;
    }
}