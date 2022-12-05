using System;

namespace DanonsTools.ECSLayer
{
    /// <summary>
    /// Acts as a container of a set of components. Can be filtered by queries to get entities that have specified components.
    /// </summary>
    public readonly struct Entity : IEquatable<Entity>
    {
        internal Entity(int index, int version)
        {
            this.index = index;
            this.version = version;
        }

        /// <summary>
        /// the world that the entity belongs to
        /// </summary>
        public World World
        {
            get
            {
                ref var info = ref Entities.all[index];
                return info.version == version ? info.worldInfo.world : default;
            }
        }

        /// <summary>
        /// the archetype that the entity belongs to
        /// </summary>
        public Archetype Archetype
        {
            get
            {
                ref var info = ref Entities.all[index];
                return info.version == version ? info.archInfo.archetype : default;
            }
        }

        /// <summary>
        /// the combination of the index and version act as a unique identifier for the entity
        /// </summary>
        public readonly int index;

        /// <summary>
        /// the combination of the index and version act as a unique identifier for the entity
        /// </summary>
        public readonly int version;

        /// <summary>
        /// returns entity's string value if set
        /// </summary>
        public override string ToString()
        {
            TryGet<string>(out var name);
            if (string.IsNullOrEmpty(name))
                name = IsValid() ? "Entity" : "~Entity";
            return $"{name} {index}.{version}";
        }

        /// <summary>
        /// returns true if the the entity is not destroyed or null
        /// </summary>
        public bool IsValid() => Entities.all[index].version == version;

        /// <summary>
        /// returns true if the entity has the component
        /// </summary>
        public bool Has<TComponent>()
        {
            ref var info = ref Entities.all[index];
            return info.version == version && info.archInfo.Has(TypeID<TComponent>.Value);
        }

        /// <summary>
        /// returns true if the entity has the component
        /// </summary>
        public bool Has(Type type)
        {
            ref var info = ref Entities.all[index];
            return info.version == version && info.archInfo.Has(TypeID.Get(type));
        }

        /// <summary>
        /// [structural]
        /// removes the component from the entity.
        /// if component was removed will trigger the corresponding onremove component event
        /// </summary>
        public Entity Remove<TComponent>()
        {
            Entities.all[index].worldInfo?.StructureEvents.Remove<TComponent>(this);
            return this;
        }

        /// <summary>
        /// [structural]
        /// removes the component from the entity.
        /// if component was removed will trigger the corresponding onremove component event
        /// </summary>
        public Entity Remove(Type type)
        {
            ref var info = ref Entities.all[index];
            info.worldInfo?.GetData(TypeID.Get(type)).Remove(this, info.worldInfo.StructureEvents);
            return this;
        }

        /// <summary>
        /// [structural]
        /// sets the entity's component to value. 
        /// If the entity does not have the component, will move the entity to an archetype that does.
        /// will trigger the onset component event if component was set
        /// </summary>
        public Entity Set<TComponent>(in TComponent component)
        {
            Entities.all[index].worldInfo?.StructureEvents.Set(this, component);
            return this;
        }

        /// <summary>
        /// [structural]
        /// sets the entity's component to value. 
        /// If the entity does not have the component, will move the entity to an archetype that does.
        /// will trigger the onset component event if component was set
        /// will trigger an exception if component_of_type is not of type
        /// </summary>
        public Entity Set(Type type, object componentOfType)
        {
            ref var info = ref Entities.all[index];
            info.worldInfo?.GetData(TypeID.Get(type)).Set(this, componentOfType, info.worldInfo.StructureEvents);
            return this;
        }

        /// <summary>
        /// returns true if the entity has component, outputs the component
        /// </summary>
        public bool TryGet<TComponent>(out TComponent component)
        {
            ref var info = ref Entities.all[index];
            if (info.version == version)
            {
                if (info.archInfo.TryGetArray<TComponent>(out var buffer))
                {
                    component = buffer[info.archIndex];
                    return true;
                }
            }

            component = default;
            return false;
        }

        /// <summary>
        /// returns true if the entity has component, outputs the component
        /// </summary>
        public bool TryGet(Type type, out object component)
        {
            ref var info = ref Entities.all[index];
            if (info.version == version)
            {
                if (info.archInfo.TryGetCompBuffer(TypeID.Get(type), out var buffer))
                {
                    component = buffer.array[info.archIndex];
                    return true;
                }
            }

            component = default;
            return false;
        }

        /// <summary>
        /// gets the reference to the component on the entity.
        /// throws an exception if the entity is invalid or does not have the component
        /// </summary>
        public ref TComponent Get<TComponent>()
        {
            ref var entityInfo = ref Entities.all[index];
            if (entityInfo.version == version)
            {
                if (entityInfo.archInfo.TryGetArray<TComponent>(out var buffer))
                    return ref buffer[entityInfo.archIndex];
                throw new Exception($"{this} does not contain {typeof(TComponent).Name}");
            }

            throw new Exception($"{this} is not a valid entity, cannot get {typeof(TComponent).Name}");
        }

        /// <summary>
        /// [structural]
        /// transfers the entity to the target world
        /// </summary>
        public void Transfer(World targetWorld)
        {
            Entities.all[index].worldInfo?.StructureEvents.Transfer(this, targetWorld);
        }

        /// <summary>
        /// [structural]
        /// destroys the entity
        /// </summary>
        public void Destroy()
        {
            Entities.all[index].worldInfo?.StructureEvents.Destroy(this);
        }

        bool IEquatable<Entity>.Equals(Entity other) => index == other.index && version == other.version;

        public override bool Equals(object obj) => obj is Entity e && e == this;

        public static bool operator ==(Entity a, Entity b) => a.index == b.index && a.version == b.version;

        public static bool operator !=(Entity a, Entity b) => !(a == b);

        public override int GetHashCode() => index;

        public static implicit operator bool(Entity entity) => entity.IsValid();

        /// <summary>
        /// returns a copy of all the entity's components
        /// </summary>
        public object[] GetAllComponents()
        {
            ref var info = ref Entities.all[index];
            return info.version == version ? info.archInfo.GetAllComponents(info.archIndex) : Array.Empty<object>();
        }

        /// <summary>
        /// returns a copy of all the types of components attached to the entity
        /// </summary>
        public Type[] GetAllComponentTypes()
        {
            ref var info = ref Entities.all[index];
            return info.version == version ? info.archInfo.GetComponentTypes() : Type.EmptyTypes;
        }

        /// <summary>
        /// returns how many components are attached to the entity
        /// </summary>
        public int GetComponentCount()
        {
            ref var info = ref Entities.all[index];
            return info.version == version ? info.archInfo.componentCount : 0;
        }
    }

    public partial class Extensions
    {
        /// <summary>
        /// Sets the value to entity only if entity is valid and has component.
        /// Does not invoke set callback event
        /// </summary>
        public static void SetDirectNoInvoke(this Entity entity, Type type, in object value)
        {
            var entityInfo = Entities.all[entity.index];
            if (entityInfo.version == entity.version)
            {
                if (entityInfo.archInfo.TryGetCompBuffer(TypeID.Get(type), out var buffer))
                    buffer.array[entityInfo.archIndex] = value;
            }
        }
    }
}