using System;
using DanonsTools.UtilitiesLayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DanonsTools.ECSLayer.UnityLayer.ActorLayer
{
    public static class ActorUtilities
    {
        public static Entity BuildEntityFromActor(in World world, in Actor actor)
        {
            actor.Clone(world, world.CreateEntity());
            return actor.Entity;
        }
        
        public static Entity BuildEntityFromActorGameObject(in World world, in GameObject actorObject)
        {
            if (!actorObject.TryGetComponent(out Actor actor))
                throw new Exception("Cannot build entity from GameObject that has no Actor component.");
            return BuildEntityFromActor(world, actor);
        }

        public static Entity BuildEntityFromActorPrefab(in World world, in GameObject actorPrefab)
        {
            return BuildEntityFromActorGameObject(world, Object.Instantiate(actorPrefab));
        }

        public static Entity BuildEntityFromActorPrefab(in World world, in GameObject actorPrefab, in string scene)
        {
            return BuildEntityFromActorGameObject(world, SceneUtilities.InstantiateIntoScene(actorPrefab, scene));
        }

        public static void BuildEntitiesFromAllActorsInScene(in World world, in string scene)
        {
            var rootObjectsInScene = SceneManager.GetSceneByName(scene).GetRootGameObjects();

            foreach (var rootObject in rootObjectsInScene)
            {
                if (rootObject.TryGetComponent<Actor>(out var actor))
                    BuildEntityFromActor(world, actor);
            }
        }
    }
}