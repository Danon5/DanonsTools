using DanonsTools.ECSLayer.UnityLayer.EventLayer;
using DanonsTools.ECSLayer.UnityLayer.EventLayer.Animation;
using DanonsTools.ECSLayer.UnityLayer.EventLayer.Physics;
using UnityEngine;

namespace DanonsTools.ECSLayer.UnityLayer.ActorLayer
{
    public sealed class Actor : MonoBehaviour, IActor
    {
        public Entity Entity { get; private set; }

        [SerializeField] private Actor[] _subActors;
        [SerializeField] private BaseEcsAnimationEventReceiver[] _animationEventReceivers;
        [SerializeField] private EcsCollision2DEventReceiver[] _collision2DEventReceivers;

        public void Clone(in World world, in Entity entity)
        {
            Entity = entity;

            foreach (var actor in GetComponents<IActor>())
            {
                if (ReferenceEquals(actor, this))
                    continue;
                actor.Clone(world, entity);
            }

            foreach (var actor in _subActors)
                actor.Clone(world, world.CreateEntity());

            foreach (var animationEventReceiver in _animationEventReceivers)
                animationEventReceiver.Initialize(world, entity);

            foreach (var collision2DEventReceiver in _collision2DEventReceivers)
                collision2DEventReceiver.Initialize(world, entity);
        }
    }
}