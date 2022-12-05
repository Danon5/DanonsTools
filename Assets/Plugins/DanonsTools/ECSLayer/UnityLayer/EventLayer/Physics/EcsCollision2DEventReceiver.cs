using DanonsTools.ECSLayer.UnityLayer.ActorLayer;
using DanonsTools.ECSLayer.UnityLayer.EventLayer.Physics.EventComponents;
using JetBrains.Annotations;
using UnityEngine;

namespace DanonsTools.ECSLayer.UnityLayer.EventLayer.Physics
{
    public sealed class EcsCollision2DEventReceiver : MonoBehaviour
    {
        public World World { get; private set; }
        public Entity Entity { get; private set; }

        public void Initialize(in World world, in Entity entity)
        {
            World = world;
            Entity = entity;
        }
        
        [UsedImplicitly]
        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.TryGetComponent<Actor>(out var actor))
                World.GetData<EcsEventManager>().CreateEvent(new CollisionEnterEventComponent
                {
                    entity = Entity, 
                    otherEntity = actor.Entity
                });
        }

        [UsedImplicitly]
        private void OnCollisionStay2D(Collision2D col)
        {
            if (col.gameObject.TryGetComponent<Actor>(out var actor))
                World.GetData<EcsEventManager>().CreateEvent(new CollisionStayEventComponent
                {
                    entity = Entity, 
                    otherEntity = actor.Entity
                });
        }

        [UsedImplicitly]
        private void OnCollisionExit2D(Collision2D col)
        {
            if (col.gameObject.TryGetComponent<Actor>(out var actor))
                World.GetData<EcsEventManager>().CreateEvent(new CollisionExitEventComponent
                {
                    entity = Entity, 
                    otherEntity = actor.Entity
                });
        }
    }
}