using UnityEngine;

namespace DanonsTools.ECSLayer.UnityLayer.EventLayer.Animation
{
    public abstract class BaseEcsAnimationEventReceiver : MonoBehaviour
    {
        public World World { get; private set; }
        public Entity Entity { get; private set; }

        public void Initialize(in World world, in Entity entity)
        {
            World = world;
            Entity = entity;
        }
    }
}