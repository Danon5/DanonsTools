using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DanonsTools.ECSLayer.UnityLayer.ActorLayer;
using UnityEngine;

namespace DanonsTools.ECSLayer.UnityLayer.Physics2DLayer
{
    public static class EcsPhysics2D
    {
        #region Raycast

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Raycast(in Vector2 origin, in Vector2 direction, 
            out RaycastHit2D hit, out Entity entity)
        {
            hit = Physics2D.Raycast(origin, direction);
            TryGetEntityFromHit(hit, out entity);
            return hit.collider != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Raycast(in Vector2 origin, in Vector2 direction, in float distance, 
            out RaycastHit2D hit, out Entity entity)
        {
            hit = Physics2D.Raycast(origin, direction, distance);
            TryGetEntityFromHit(hit, out entity);
            return hit.collider != null;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Raycast(in Vector2 origin, in Vector2 direction, in float distance, in int layerMask,
            out RaycastHit2D hit, out Entity entity)
        {
            hit = Physics2D.Raycast(origin, direction, distance, layerMask);
            TryGetEntityFromHit(hit, out entity);
            return hit.collider != null;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Raycast(in Vector2 origin, in Vector2 direction, in ContactFilter2D contactFilter, 
            in List<RaycastHit2D> hits, in List<Entity> entities, float distance)
        {
            var count = Physics2D.Raycast(origin, direction, contactFilter, hits, distance);

            for (var i = 0; i < count; i++)
            {
                TryGetEntityFromHit(hits[i], out var entity);
                
                if (i >= hits.Count)
                    entities.Add(entity);
                else
                    entities[i] = entity;
            }

            return count;
        }

        #endregion

        #region RaycastNonAlloc

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RaycastNonAlloc(in Vector2 origin, in Vector2 direction, in RaycastHit2D[] hits, 
            in Entity[] entities, in float distance, in int layerMask)
        {
            var count = Physics2D.RaycastNonAlloc(origin, direction, hits, distance, layerMask);

            for (var i = 0; i < count; i++)
            {
                TryGetEntityFromHit(hits[i], out var entity);
                entities[i] = entity;
            }

            return count;
        }

        #endregion

        #region CircleCast

        public static bool CircleCast(in Vector2 origin, in float radius, in Vector2 direction, in float distance,
            out RaycastHit2D hit, out Entity entity)
        {
            hit = Physics2D.CircleCast(origin, radius, direction, distance);
            TryGetEntityFromHit(hit, out entity);
            return hit.collider != null;
        }

        #endregion

        #region CircleCastNonAlloc

        public static int CircleCastNonAlloc(in Vector2 origin, in float radius, in Vector2 direction, 
            in RaycastHit2D[] hits, in Entity[] entities, in float distance)
        {
            var count = Physics2D.CircleCastNonAlloc(origin, radius, direction, hits, distance);

            for (var i = 0; i < count; i++)
            {
                TryGetEntityFromHit(hits[i], out var entity);
                entities[i] = entity;
            }

            return count;
        }
        
        public static int CircleCastNonAlloc(in Vector2 origin, in float radius, in Vector2 direction, 
            in RaycastHit2D[] hits, in Entity[] entities, in float distance, in int layerMask)
        {
            var count = Physics2D.CircleCastNonAlloc(origin, radius, direction, hits, distance, layerMask);

            for (var i = 0; i < count; i++)
            {
                TryGetEntityFromHit(hits[i], out var entity);
                entities[i] = entity;
            }

            return count;
        }

        #endregion
        
        #region OverlapCircle

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool OverlapCircle(in Vector2 point, in float radius)
        {
            return Physics2D.OverlapCircle(point, radius);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool OverlapCircle(in Vector2 point, in float radius, in int layerMask)
        {
            return Physics2D.OverlapCircle(point, radius, layerMask);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int OverlapCircle(in Vector2 point, in float radius, in ContactFilter2D contactFilter, 
            in Collider2D[] colliders, in Entity[] entities)
        {
            var count = Physics2D.OverlapCircle(point, radius, contactFilter, colliders);

            for (var i = 0; i < count; i++)
            {
                TryGetEntityFromCollider(colliders[i], out var entity);
                entities[i] = entity;
            }

            return count;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int OverlapCircle(in Vector2 point, in float radius, in ContactFilter2D contactFilter, 
            in List<Collider2D> colliders, in List<Entity> entities)
        {
            var count = Physics2D.OverlapCircle(point, radius, contactFilter, colliders);

            for (var i = 0; i < count; i++)
            {
                TryGetEntityFromCollider(colliders[i], out var entity);
                
                if (i >= colliders.Count)
                    entities.Add(entity);
                else
                    entities[i] = entity;
            }

            return count;
        }

        #endregion

        #region OverlapCircleNonAlloc

        public static int OverlapCircleNonAlloc(in Vector2 point, in float radius,
            in Collider2D[] colliders, in Entity[] entities)
        {
            var count = Physics2D.OverlapCircleNonAlloc(point, radius, colliders);
            
            for (var i = 0; i < count; i++)
            {
                TryGetEntityFromCollider(colliders[i], out var entity);
                entities[i] = entity;
            }

            return count;
        }
        
        public static int OverlapCircleNonAlloc(in Vector2 point, in float radius,
            in Collider2D[] colliders, in Entity[] entities, in int layerMask)
        {
            var count = Physics2D.OverlapCircleNonAlloc(point, radius, colliders, layerMask);
            
            for (var i = 0; i < count; i++)
            {
                TryGetEntityFromCollider(colliders[i], out var entity);
                entities[i] = entity;
            }

            return count;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryGetEntityFromHit(in RaycastHit2D hit, out Entity entity)
        {
            return TryGetEntityFromCollider(hit.collider, out entity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryGetEntityFromCollider(in Collider2D collider, out Entity entity)
        {
            if (collider != null && collider.TryGetComponent<Actor>(out var actor))
            {
                entity = actor.Entity;
                return true;
            }
            entity = default;
            return false;
        }
    }
}