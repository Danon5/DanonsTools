using System;

namespace DanonsTools.ECSLayer.UnityLayer.EventLayer.Physics.EventComponents
{
    [Serializable]
    public struct TriggerStayEventComponent : IEcsEventComponent
    {
        public Entity entity;
        public Entity otherEntity;
    }
}