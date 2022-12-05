using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DanonsTools.ECSLayer
{
    /// <summary>
    /// stores component data of entities that matches the archetype's type signature
    /// </summary>
    public readonly struct Archetype : IEquatable<Archetype>, IEnumerable<Entity> 
    {
        internal Archetype(World world, int index, int version)
        {
            this.world = world; this.index = index; this.version = version;
        }

        /// <summary>
        /// returns a copy of archetype's type signature
        /// </summary>
        public TypeSignature GetTypeSignature() => this.TryGetArchetypeInfo(out var archetypeInfo) ? 
            new TypeSignature(archetypeInfo.signature) : new TypeSignature();

        /// <summary>
        /// returns a copy of component types in this archetype
        /// </summary>
        public Type[] GetTypes() => this.TryGetArchetypeInfo(out var archetypeInfo) ? archetypeInfo.GetComponentTypes() : Type.EmptyTypes;
        
        /// <summary>
        /// the world this archetype belongs to
        /// </summary>
        public readonly World world;

        /// <summary>
        /// the index and version create a unique identifier for the archetype
        /// </summary>
        public readonly int index;

        /// <summary>
        /// the index and version create a unique identifier for the archetype
        /// </summary>
        public readonly int version;

        /// <summary>
        /// [structural]
        /// creates an entity that matches this archetype
        /// </summary>
        public Entity CreateEntity()
        {
            return this.TryGetArchetypeInfo(out var worldInfo, out var archetypeInfo) ? 
                worldInfo.StructureEvents.CreateEntity(archetypeInfo) : default;
        }
        
        /// <summary>
        /// [structural]
        /// creates an entity that matches this archetype
        /// </summary>
        public Entity CreateEntity(int id)
        {
            return this.TryGetArchetypeInfo(out var worldInfo, out var archetypeInfo) ? 
                worldInfo.StructureEvents.CreateEntity(archetypeInfo, id) : default;
        }

        /// <summary>
        /// returns a copy of all the entities stored in the archetype
        /// </summary>
        public Entity[] GetEntities()
        {
            var entities = new Entity[EntityCount];
            
            if (!this.TryGetArchetypeInfo(out var archetypeInfo)) return entities;
            
            for (var i = 0; i < archetypeInfo.entityCount; ++i)
                entities[i] = archetypeInfo.entities[i];
            return entities;
        }

        /// <summary>
        /// returns the total amount of entities stored in the archetype
        /// </summary>
        public int EntityCount => this.TryGetArchetypeInfo(out var archetypeInfo) ? archetypeInfo.entityCount : 0;

        /// <summary>
        /// returns false if the archetype is invalid or destroyed.
        /// outputs the raw entity storage buffer.
        /// should be treated as readonly as changing values will break the ecs.
        /// only entities up to archetype's EntityCount are valid, DO NOT use the length of the array
        /// </summary>
        public bool TryGetEntityBuffer(out Entity[] entityBuffer)
        {
            if (this.TryGetArchetypeInfo(out var data))
            {
                entityBuffer = data.entities;
                return true;
            }
            entityBuffer = default;
            return false;
        }

        /// <summary>
        /// returns false if the archetype is invalid or does not store the component buffer
        /// outputs the raw component storage buffer.
        /// only components up to archetype's EntityCount are valid
        /// entities in the entity buffer that share the same index as the component in the component buffer own that component
        /// </summary>
        public bool TryGetComponentBuffer<TComponent>(out TComponent[] compBuffer)
        {
            if (this.TryGetArchetypeInfo(out var data))
                return data.TryGetArray(out compBuffer);
            compBuffer = default;
            return false;
        }

        /// <summary>
        /// [structural]
        /// destroys the archetype along with all the entities within it
        /// </summary>
        public void Destroy()
        {
            if (world.IsValid())
                WorldInfo.All[world.index].data.StructureEvents.DestroyArchetype(this);
        }

        /// <summary>
        /// [structural]
        /// resizes the archetype's backing arrays to the minimum number of 2 needed to store the entities
        /// </summary>
        public void ResizeBackingArrays()
        {
            if (world.IsValid())
                WorldInfo.All[world.index].data.StructureEvents.ResizeBackingArrays(this);
        }

        bool IEquatable<Archetype>.Equals(Archetype other)
            => world == other.world && index == other.index && version == other.version;
 
        /// <summary>
        /// returns true if the archetype is not null or destroyed
        /// </summary>
        public bool IsValid()
            => world.TryGetWorldInfo(out var info) && info.archetypes[index].version == version;

        public static implicit operator bool(Archetype archetype) => archetype.IsValid();

        public override bool Equals(object obj) => obj is Archetype a && a == this;

        public static implicit operator int(Archetype a) => a.index;

        public static bool operator ==(Archetype a, Archetype b) => a.world == b.world && a.index == b.index && a.version == b.version;

        public static bool operator !=(Archetype a, Archetype b) => !(a == b);

        public override int GetHashCode() => index;

        public override string ToString() => $"{(IsValid() ? "" : "~")}Arch [{GetTypeString()}]";

        private string GetTypeString()
        {
            var val = "";
            
            if (!this.TryGetArchetypeInfo(out var archetypeInfo)) return val;
            
            for(var i = 0; i < archetypeInfo.componentCount; ++ i)
                val += $" {TypeID.Get(archetypeInfo.componentBuffers[i].typeID).Name}";
            
            return val;
        }

        IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator()
        {
            if (!this.TryGetArchetypeInfo(out var info)) yield break;
            
            for(var i = 0; i < info.entityCount; ++ i)
                yield return info.entities[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (!this.TryGetArchetypeInfo(out var info)) yield break;
            
            for(var i = 0; i < info.entityCount; ++ i)
                yield return info.entities[i];
        }
    }

    public static partial class Extensions
    {
        public static bool TryGetArchetypeInfo(this Archetype archetype, out WorldInfo worldInfo, out ArchetypeInfo archInfo)
        {
            if (archetype.world.TryGetWorldInfo(out worldInfo))
            {
                var arch = worldInfo.archetypes[archetype.index];
                if (arch.version == archetype.version)
                {
                    archInfo = arch.data;
                    return true;
                }
            }

            archInfo = default;
            worldInfo = default;
            return false;
        }

        public static bool TryGetArchetypeInfo(this Archetype archetype, out ArchetypeInfo archInfo)
        {
            if (archetype.world.TryGetWorldInfo(out var worldInfo))
            {
                var arch = worldInfo.archetypes[archetype.index];
                if (arch.version == archetype.version)
                {
                    archInfo = arch.data;
                    return true;
                }
            }

            archInfo = default;
            return false;
        }
    }

    public class ArchetypeInfo
    {
        public ArchetypeInfo(WorldInfo world, TypeSignature signature, int archIndex, int archVersion)
        {
            worldInfo = world;
            this.signature = signature;
            archetype = new Archetype(world.world, archIndex, archVersion);

            componentBuffers = new CompBufferData[signature.Count == 0 ? 1 : signature.Count];
            componentCount = signature.Count;

            for (var i = 0; i < componentBuffers.Length; ++i)
                componentBuffers[i].next = -1;

            // add components into empty bucket, skip if bucket is occupied
            for (var i = 0; i < componentCount; ++i)
            {
                var type = signature.Types[i];
                var typeID = TypeID.Get(type);
                var index = typeID % componentBuffers.Length;
                ref var bufferData = ref componentBuffers[index];

                if (bufferData.typeID != 0) continue;

                bufferData.typeID = typeID;
                bufferData.buffer = CreatePool(type);
            }

            // add skipped components into buckets not filled in first pass
            // hopefully this minimizes lookup time
            for (var i = 0; i < componentCount; ++i)
            {
                var type = signature.Types[i];
                var typeID = TypeID.Get(type);
                if (ContainsType(typeID)) continue;
                var index = GetEmptyIndex(typeID % componentBuffers.Length);
                ref var bufferData = ref componentBuffers[index];

                bufferData.typeID = typeID;
                bufferData.buffer = CreatePool(type);
            }

            bool ContainsType(int typeID) => componentBuffers.Any(val => val.typeID == typeID);

            // if current index is filled, will return an empty index with a way to get to that index from the provided one
            int GetEmptyIndex(int currentIndex)
            {
                if (componentBuffers[currentIndex].typeID == 0)
                    return currentIndex;

                while (componentBuffers[currentIndex].next >= 0)
                {
                    currentIndex = componentBuffers[currentIndex].next;
                }

                for (var i = 0; i < componentCount; ++i)
                    if (componentBuffers[i].typeID == 0)
                    {
                        componentBuffers[currentIndex].next = i;
                        return i;
                    }

                throw new Exception("FRAMEWORK BUG: not enough components in archetype");
            }

            CompBuffer CreatePool(Type type)
                => Activator.CreateInstance(typeof(CompBuffer<>).MakeGenericType(type)) as CompBuffer;
        }


        public int entityCount;
        public Entity[] entities = new Entity[8];
        public readonly WorldInfo worldInfo;
        public readonly TypeSignature signature;
        public readonly Archetype archetype;
        public readonly int componentCount;
        public readonly CompBufferData[] componentBuffers;

        public struct CompBufferData
        {
            public int next;
            public int typeID;
            public CompBuffer buffer;
        }

        /// <summary>
        /// resizes all backing arrays to minimum power of 2
        /// </summary>
        public void ResizeBackingArrays()
        {
            var size = 8;
            while (size <= entityCount)
                size *= 2;
            Array.Resize(ref entities, size);
            for (var i = 0; i < componentCount; ++i)
                componentBuffers[i].buffer.Resize(size);
        }

        public void EnsureCapacity(int capacity)
        {
            if (capacity >= entities.Length)
            {
                var size = entities.Length;
                while (capacity >= size)
                    size *= 2;
                Array.Resize(ref entities, size);
                for (var i = 0; i < componentCount; ++i)
                    componentBuffers[i].buffer.Resize(size);
            }
        }

        public bool Has(int typeID)
        {
            var data = componentBuffers[typeID % componentBuffers.Length];
            if (data.typeID == typeID)
                return true;

            while (data.next >= 0)
            {
                data = componentBuffers[data.next];
                if (data.typeID == typeID)
                    return true;
            }

            return false;
        }

        public bool TryGetArray<TComponent>(out TComponent[] components)
        {
            var typeID = TypeID<TComponent>.Value;
            var data = componentBuffers[typeID % componentBuffers.Length];
            if (data.typeID == typeID)
            {
                components = (TComponent[])data.buffer.array;
                return true;
            }

            while (data.next >= 0)
            {
                data = componentBuffers[data.next];
                if (data.typeID == typeID)
                {
                    components = (TComponent[])data.buffer.array;
                    return true;
                }
            }

            components = default;
            return false;
        }

        public bool TryGetCompBuffer(int typeID, out CompBuffer buffer)
        {
            var data = componentBuffers[typeID % componentBuffers.Length];
            if (data.typeID == typeID)
            {
                buffer = data.buffer;
                return true;
            }

            while (data.next >= 0)
            {
                data = componentBuffers[data.next];
                if (data.typeID == typeID)
                {
                    buffer = data.buffer;
                    return true;
                }
            }

            buffer = default;
            return false;
        }

        public object[] GetAllComponents(int entityArchIndex)
        {
            var components = new object[componentCount];

            for (var i = 0; i < componentCount; ++i)
                components[i] = componentBuffers[i].buffer.array[entityArchIndex];
            return components;
        }

        public Type[] GetComponentTypes()
        {
            var components = new Type[componentCount];
            for (var i = 0; i < componentCount; ++i)
                components[i] = TypeID.Get(componentBuffers[i].typeID);
            return components;
        }

        public abstract class CompBuffer //handles component data
        {
            public IList array;
            public abstract void Resize(int capacity);

            /// <summary>
            /// returns removed component
            /// </summary>
            public abstract object Remove(int entityArchIndex, int last);

            public abstract void Move(int entityArchIndex, int lastEntityIndex, ArchetypeInfo targetArchetype, int targetIndex);
            public abstract void Move(int entityArchIndex, int lastEntityIndex, object buffer, int targetIndex);
        }

        public sealed class CompBuffer<TComponent> : CompBuffer
        {
            public CompBuffer()
            {
                array = components;
            }

            public TComponent[] components = new TComponent[8];

            public override void Resize(int capacity)
            {
                Array.Resize(ref components, capacity);
                array = components;
            }

            public override object Remove(int entityArchIndex, int last)
            {
                var comp = components[entityArchIndex];
                components[entityArchIndex] = components[last];
                components[last] = default;
                return comp;
            }

            public override void Move(int entityArchIndex, int lastEntityIndex, ArchetypeInfo targetArchetype, int targetIndex)
            {
                if (targetArchetype.TryGetArray<TComponent>(out var targetArray))
                {
                    targetArray[targetIndex] = components[entityArchIndex];
                }

                components[entityArchIndex] = components[lastEntityIndex];
                components[lastEntityIndex] = default;
            }

            public override void Move(int entityArchIndex, int lastEntityIndex, object buffer, int targetIndex)
            {
                ((TComponent[])buffer)[targetIndex] = components[entityArchIndex];
                components[entityArchIndex] = components[lastEntityIndex];
                components[lastEntityIndex] = default;
            }
        }
    }
}