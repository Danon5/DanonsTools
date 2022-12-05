using DanonsTools.ECSLayer.UnityLayer;
using UnityEngine;

namespace DanonsTools.ECSLayer
{
    public static class EcsUtilities
    {
        public static void DestroyWithSceneObject(in this Entity entity)
        {
            if (entity.Has<SceneObjectComponent>())
                Object.Destroy(entity.Get<SceneObjectComponent>().transform.gameObject);
            entity.Destroy();
        }
    }
}