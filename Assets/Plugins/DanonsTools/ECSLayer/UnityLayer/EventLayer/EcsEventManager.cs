using System;
using System.Collections.Generic;

namespace DanonsTools.ECSLayer.UnityLayer.EventLayer
{
    public sealed class EcsEventManager
    {
        public World World { get; private set; }
        
        private readonly List<Type> _eventTypes = new List<Type>();

        public EcsEventManager(in World world)
        {
            World = world;
        }
        
        public void CreateEvent<T>(in T eventComponent) where T : IEcsEventComponent
        {
            var type = typeof(T);
            
            if (!_eventTypes.Contains(type))
                _eventTypes.Add(type);

            World.CreateEntity().Set(eventComponent);
        }

        public void ClearEvents()
        {
            World.CacheStructuralEvents(true);
            
            foreach (var type in _eventTypes)
            {
                var query = World.CreateQuery().With(type);
                query.ForEach(entity => entity.Destroy());
            }
            
            World.CacheStructuralEvents(false);
        }
    }
}