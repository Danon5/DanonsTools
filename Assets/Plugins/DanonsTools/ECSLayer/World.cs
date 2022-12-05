using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DanonsTools.ECSLayer
{
    /// <summary>
    /// manages all entities and archetype information
    /// </summary>
    public readonly partial struct World : IEquatable<World>, IEnumerable<Archetype>
    {
        private World(int index, int version)
        {
            this.index = index;
            this.version = version;
        }

        /// <summary>
        /// the index and version together create a unique identifier for the world
        /// </summary>
        public readonly int index;

        /// <summary>
        /// the index and version together create a unique identifier for the world
        /// </summary>
        public readonly int version;

        /// <summary>
        /// Name of the world
        /// </summary>
        public string Name
        {
            get => this.TryGetWorldInfo(out var info) ? info.name : "~World";
            set
            {
                if (this.TryGetWorldInfo(out var info))
                    info.name = value;
            }
        }

        /// <summary>
        /// Returns a copy of all the archetypes in the current world
        /// </summary>
        public Archetype[] GetArchetypes()
        {
            Archetype[] archetypes;
            if (this.TryGetWorldInfo(out var info))
            {
                archetypes = new Archetype[info.ArchetypeCount];
                var count = 0;
                foreach (var arch in info.archetypes)
                    if (arch.data != null)
                    {
                        archetypes[count] = arch.data.archetype;
                        count++;
                    }
            }
            else
                archetypes = Array.Empty<Archetype>();

            return archetypes;
        }

        /// <summary>
        /// Returns a copy of all the entities in the the current world
        /// </summary>
        public Entity[] GetEntities()
        {
            Entity[] entities;
            if (this.TryGetWorldInfo(out var info))
            {
                entities = new Entity[EntityCount];
                var count = 0;
                foreach (var archetype in info.archetypes)
                {
                    if (archetype.data == null) continue;

                    for (var e = 0; e < archetype.data.entityCount; ++e)
                    {
                        entities[count] = archetype.data.entities[e];
                        count++;
                    }
                }
            }
            else entities = Array.Empty<Entity>();

            return entities;
        }

        /// <summary>
        /// Creates a new world
        /// </summary>
        public static World Create() => Create("World");

        /// <summary>
        /// Gets world with name, else creates and returns a world with name
        /// </summary>
        public static World GetOrCreate(string name)
        {
            return !TryGetWorld(name, out var world) ? Create(name) : world;
        }

        /// <summary>
        /// Tries to get world with name
        /// </summary>
        /// <param name="name">name of the target world</param>
        /// <param name="targetWorld">target world</param>
        /// <returns>returns false if not found</returns>
        public static bool TryGetWorld(string name, out World targetWorld)
        {
            foreach (var current in GetAll())
                if (current.Name == name)
                {
                    targetWorld = current;
                    return true;
                }

            targetWorld = default;
            return false;
        }

        /// <summary>
        /// Creates an new world with Name
        /// </summary>
        public static World Create(string name)
        {
            var index = -1;
            for (var i = 0; i < WorldInfo.All.Length; ++i)
            {
                if (WorldInfo.All[i].data != null) continue;

                index = i;
                break;
            }

            if (index < 0)
            {
                index = WorldInfo.All.Length;
                Array.Resize(ref WorldInfo.All, index + 4);
            }

            ref var worldData = ref WorldInfo.All[index];
            var version = worldData.version;
            worldData.data = new WorldInfo(name, new World(index, version));
            WorldInfo.worldCount++;
            return worldData.data.world;
        }

        /// <summary>
        /// Returns true if the world is not null or destroyed
        /// </summary>
        public bool IsValid() => WorldInfo.All[index].version == version;

        /// <summary>
        /// Destroys the world along with all it's archetypes and entities
        /// </summary>
        public void Destroy()
        {
            if (this.TryGetWorldInfo(out var info))
                info.StructureEvents.DestroyWorld();
        }

        /// <summary>
        /// [structural]
        /// Creates an entity in this world
        /// </summary>
        public Entity CreateEntity()
        {
            if (!this.TryGetWorldInfo(out var info)) return default;
            return info.StructureEvents.CreateEntity(info.GetArchetypeData(info.bufferSignature.Clear()));
        }

        /// <summary>
        /// Returns how many entities are currently in this world
        /// </summary>
        public int EntityCount => this.TryGetWorldInfo(out var info) ? info.entityCount : 0;

        /// <summary>
        /// Creates a query that operates on this world
        /// </summary>
        public Query CreateQuery() => new Query(this);

        /// <summary>
        /// Tries to get the archetype that matches the supplied TypeSignature.
        /// Returns false if the world is destroyed or null
        /// </summary>
        public bool TryGetArchetype(out Archetype archetype, TypeSignature signature)
        {
            if (this.TryGetWorldInfo(out var info))
            {
                archetype = info.GetArchetypeData(signature).archetype;
                return true;
            }

            archetype = default;
            return false;
        }

        /// <summary>
        /// Tries to get an archetype that has the supplied types.
        /// Returns false if the world is destroyed or null
        /// </summary>
        public bool TryGetArchetype(out Archetype archetype, params Type[] types)
            => TryGetArchetype(out archetype, new TypeSignature(types));

        /// <summary>
        /// Tries to get an archetype that has the supplied types.
        /// Returns false if the world is destroyed or null
        /// </summary>
        public bool TryGetArchetype(out Archetype archetype, IEnumerable<Type> types)
            => TryGetArchetype(out archetype, new TypeSignature(types));

        /// <summary>
        /// WorldData is data unique to this world
        /// Set's the world's data to value.
        /// </summary>
        public World SetData<TWorldData>(TWorldData worldData)
        {
            if (!this.TryGetWorldInfo(out var info)) return this;

            var stored = info.GetData<TWorldData>();
            stored.assignedData = true;
            stored.data = worldData;
            return this;
        }

        /// <summary>
        /// WorldData is data unique to this world
        /// Get's a reference to the data which can be assigned.
        /// Throws an exception if the world is destroyed or null
        /// </summary>
        public ref TWorldData GetData<TWorldData>()
        {
            if (!this.TryGetWorldInfo(out var info))
                throw new Exception($"{this} is invalid, cannot get resource {typeof(TWorldData).Name}");

            var stored = info.GetData<TWorldData>();
            stored.assignedData = true;
            return ref info.GetData<TWorldData>().data;
        }

        /// <summary>
        /// Returns a copy of all the world data currently assigned in the world
        /// </summary>
        public object[] GetAllWorldData()
        {
            var all = new List<object>();

            if (!this.TryGetWorldInfo(out var info)) return all.ToArray();

            all.AddRange(from stored in info.worldData where stored != null && stored.assignedData select stored.GetData());
            return all.ToArray();
        }

        /// <summary>
        /// Retuns a copy of all the Types of world data currently assigned in the world
        /// </summary>
        public Type[] GetAllWorldDataTypes()
        {
            var all = new List<Type>();

            if (!this.TryGetWorldInfo(out var info)) return all.ToArray();

            all.AddRange(from stored in info.worldData where stored != null && stored.assignedData select stored.DataType);
            return all.ToArray();
        }

        /// <summary>
        /// Adds a callback to be invoked whenever an entity sets a component of type
        /// </summary>
        /// <param name="callback">callback to invoke</param>
        /// <param name="register">set true to add callback, false to remove the callback</param>
        public World OnSet<TComponent>(SetComponentEvent<TComponent> callback, bool register = true)
        {
            if (!this.TryGetWorldInfo(out var info)) return this;

            var data = info.GetData<TComponent>();
            if (register)
                data.setCallback += callback;
            else data.setCallback -= callback;
            data.hasSetCallback = data.setCallback != null;
            return this;
        }

        /// <summary>
        /// Adds a callback to be invoked whenever an entity sets a component of type
        /// </summary>
        /// <param name="callback">callback to invoke</param>
        /// <param name="register">set true to add callback, false to remove the callback</param>
        public World OnSet<TComponent>(SetComponentEventRefOnly<TComponent> callback, bool register = true)
        {
            if (!this.TryGetWorldInfo(out var info)) return this;

            var data = info.GetData<TComponent>();
            if (register)
            {
                if (data.setRefCallback == null)
                    data.setCallback += data.CallSetRefCallback;
                data.setRefCallback += callback;
            }
            else
            {
                data.setRefCallback -= callback;
                if (data.setRefCallback == null)
                    data.setCallback -= data.CallSetRefCallback;
            }

            data.hasSetCallback = data.setCallback != null;
            return this;
        }

        /// <summary>
        /// Adds a callback to be invoked whenever an entity sets a component of type
        /// </summary>
        /// <param name="callback">callback to invoke</param>
        /// <param name="register">set true to add callback, false to remove the callback</param>
        public World OnSet<TComponent>(SetComponentEventCompOnly<TComponent> callback, bool register = true)
        {
            if (!this.TryGetWorldInfo(out var info)) return this;

            var data = info.GetData<TComponent>();
            if (register)
            {
                if (data.setCompCallback == null)
                    data.setCallback += data.CallSetCompCallback;
                data.setCompCallback += callback;
            }
            else
            {
                data.setCompCallback -= callback;
                if (data.setCompCallback == null)
                    data.setCallback -= data.CallSetCompCallback;
            }

            data.hasSetCallback = data.setCallback != null;
            return this;
        }

        /// <summary>
        /// Adds a callback to be invoked whenever an entity removes a component of type
        /// </summary>
        /// <param name="callback">callback to invoke</param>
        /// <param name="register">set true to add callback, false to remove the callback</param>
        public World OnRemove<TComponent>(RemoveComponentEvent<TComponent> callback, bool register = true)
        {
            if (!this.TryGetWorldInfo(out var worldInfo)) return this;

            var data = worldInfo.GetData<TComponent>();
            if (register)
                data.removeCallback += callback;
            else data.removeCallback -= callback;
            data.hasRemoveCallback = data.removeCallback != null;
            return this;
        }

        /// <summary>
        /// Adds a callback to be invoked whenever an entity removes a component of type
        /// </summary>
        /// <param name="callback">callback to invoke</param>
        /// <param name="register">set true to add callback, false to remove the callback</param>
        public World OnRemove<TComponent>(RemoveComponentEventCompOnly<TComponent> callback, bool register = true)
        {
            if (!this.TryGetWorldInfo(out var worldInfo)) return this;

            var data = worldInfo.GetData<TComponent>();
            if (register)
            {
                if (data.removeCompCallback == null)
                    data.removeCallback += data.CallRemoveCompCallback;
                data.removeCompCallback += callback;
            }
            else
            {
                data.removeCompCallback -= callback;
                if (data.removeCompCallback == null)
                    data.removeCallback -= data.CallRemoveCompCallback;
            }

            data.hasRemoveCallback = data.removeCallback != null;
            return this;
        }

        /// <summary>
        /// [structural]
        /// Resizes all archetype's backing arrays to the minimum power of 2 needed to store their entities
        /// </summary>
        public void ResizeBackingArrays()
        {
            foreach (var archetype in GetArchetypes())
                archetype.ResizeBackingArrays();
        }

        /// <summary>
        /// [structural]
        /// Destroys all archetypes with 0 entities
        /// </summary>
        public void DestroyEmptyArchetypes()
        {
            foreach (var archetype in GetArchetypes())
            {
                if (archetype.EntityCount == 0)
                    archetype.Destroy();
            }
        }

        /// <summary>
        /// When enabled all structural methods will be cached like they are when iterating a query.
        /// They will be applied once you disable caching.
        /// Use to prevent iterator invalidation when manually iterating over entity or component buffers.
        /// </summary>
        public void CacheStructuralEvents(bool enable)
        {
            if (this.TryGetWorldInfo(out var info))
                info.CacheStructuralChanges = enable;
        }

        /// <summary>
        /// Returns true if the world is currently caching structural changes
        /// </summary>
        public bool IsCachingStructuralEvents()
        {
            if (this.TryGetWorldInfo(out var info))
                return info.StructureEvents.EnqueueEvents > 0;
            return false;
        }
        
        public override string ToString()
            => $"{(IsValid() ? "" : "~")}{Name} {index}.{version}";

        bool IEquatable<World>.Equals(World other) => index == other.index && version == other.version;

        public override int GetHashCode() => index;

        public override bool Equals(object obj)
        {
            if (obj is World world)
                return world == this;
            return false;
        }

        public static bool operator ==(World a, World b) => a.index == b.index && a.version == b.version;
        public static bool operator !=(World a, World b) => !(a == b);

        public static implicit operator bool(World world) => WorldInfo.All[world.index].version == world.version;

        /// <summary>
        /// Returns a copy of all active Worlds
        /// </summary>
        public static World[] GetAll() => (from info in WorldInfo.All where info.data != null select info.data.world).ToArray();

        /// <summary>
        /// Tries to get the entity with index.
        /// returns true if entity is valid
        /// </summary>
        public static bool TryGetEntity(int index, out Entity entity)
        {
            if (index <= Entities.last)
            {
                var data = Entities.all[index];
                if (data.archInfo != null)
                {
                    entity = new Entity(index, data.version);
                    return true;
                }
            }

            entity = default;
            return false;
        }

        IEnumerator<Archetype> IEnumerable<Archetype>.GetEnumerator()
        {
            if (!this.TryGetWorldInfo(out var info)) yield break;

            for (var i = 0; i < info.archetypeTerminatingIndex; ++i)
            {
                var archInfo = info.archetypes[i].data;
                if (archInfo != null)
                    yield return archInfo.archetype;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (!this.TryGetWorldInfo(out var info)) yield break;

            for (var i = 0; i < info.archetypeTerminatingIndex; ++i)
            {
                var archInfo = info.archetypes[i].data;
                if (archInfo != null)
                    yield return archInfo.archetype;
            }
        }
    }

    public delegate void SetComponentEvent<T>(Entity entity, T oldComp, ref T newComp);

    public delegate void SetComponentEventRefOnly<T>(Entity entity, ref T newComp);

    public delegate void SetComponentEventCompOnly<T>(ref T newComp);

    public delegate void RemoveComponentEvent<T>(Entity entity, T component);

    public delegate void RemoveComponentEventCompOnly<T>(T component);

    public static partial class Extensions
    {
        public static bool TryGetWorldInfo(this World world, out WorldInfo info)
        {
            var data = WorldInfo.All[world.index];
            if (data.version == world.version)
            {
                info = data.data;
                return true;
            }

            info = default;
            return false;
        }
    }

    public partial class WorldInfo
    {
        static WorldInfo()
        {
            All = new (WorldInfo, int)[4];
            All[0].version = 1; // added a version to the 0th index so that a default world will be invalid
        }

        public static (WorldInfo data, int version)[] All;
        public static int worldCount;

        public string name;
        public World world;

        public WorldInfo(string name, World world)
        {
            this.name = name;
            this.world = world;
            archetypes[0].version++; // this is just to prevent default archetype and default entity from being valid
            StructureEvents = new StructureEventHandler(this);
        }

        private bool _cacheStructuralChanges;

        public bool CacheStructuralChanges
        {
            get => _cacheStructuralChanges;
            set
            {
                StructureEvents.EnqueueEvents += value ? 1 : -1;
                _cacheStructuralChanges = value;
            }
        }

        public int ArchetypeCount => archetypeTerminatingIndex - freeArchetypes.Count;
        public int entityCount;
        public int archetypeTerminatingIndex;
        public int archetypeStructureUpdateCount;

        public readonly Stack<int> freeArchetypes = new Stack<int>();
        public WorldData[] worldData = new WorldData[32];

        public List<int> assignedWorldData = new List<int>();

        public (ArchetypeInfo data, int version)[] archetypes = new (ArchetypeInfo, int)[32];
        public readonly Dictionary<TypeSignature, int> signatureToArchetypeIndex = new Dictionary<TypeSignature, int>();

        public readonly TypeSignature bufferSignature = new TypeSignature();
        // just a scratch signature so that I'm not making new ones all the time

        public ArchetypeInfo GetArchetypeData(TypeSignature signature)
        {
            if (signatureToArchetypeIndex.TryGetValue(signature, out var index)) return archetypes[index].data;

            if (freeArchetypes.Count > 0)
            {
                index = freeArchetypes.Pop();
                archetypeStructureUpdateCount++;
            }
            else
            {
                if (archetypeTerminatingIndex == archetypes.Length)
                    Array.Resize(ref archetypes, archetypeTerminatingIndex * 2);
                index = archetypeTerminatingIndex;
                archetypeTerminatingIndex++;
            }

            var sig = new TypeSignature(signature);
            signatureToArchetypeIndex[sig] = index;
            archetypes[index].data = new ArchetypeInfo(this, sig, index, archetypes[index].version);

            return archetypes[index].data;
        }

        public WorldData<T> GetData<T>()
        {
            var typeID = TypeID<T>.Value;
            if (typeID >= worldData.Length)
            {
                var size = worldData.Length;
                while (size <= typeID)
                    size *= 2;
                Array.Resize(ref worldData, size);
            }

            worldData[typeID] ??= new WorldData<T>();

            return (WorldData<T>)worldData[typeID];
        }

        public WorldData GetData(int typeID)
        {
            if (typeID >= worldData.Length)
            {
                var size = worldData.Length;
                while (size <= typeID)
                    size *= 2;
                Array.Resize(ref worldData, size);
            }

            if (worldData[typeID] != null) return worldData[typeID];

            var type = TypeID.Get(typeID);
            worldData[typeID] = (WorldData)Activator.CreateInstance(typeof(WorldData<>).MakeGenericType(type));

            return worldData[typeID];
        }

        /// <summary>
        /// Handles all structural changes to the ecs world
        /// </summary>
        public StructureEventHandler StructureEvents;
    }

    public struct EntityInfo
    {
        public ArchetypeInfo archInfo;
        public WorldInfo worldInfo;
        public int version;
        public int archIndex;
    }

    public static class Entities
    {
        static Entities()
        {
            all = new EntityInfo[1024];
            Free = new Queue<int>(1024);
            all[0].version++;
        }

        public static readonly Queue<int> Free;

        public static EntityInfo[] all;
        public static int last;
    }

    public abstract class WorldData
    {
        public bool hasRemoveCallback, hasSetCallback, assignedData;
        public abstract void Set(in Entity entity, in StructureEventHandler handler);
        public abstract void Set(in Entity entity, object component, in StructureEventHandler handler);
        public abstract void Remove(in Entity entity, in StructureEventHandler handler);
        public abstract void InvokeRemoveCallbackAll(in Entity[] entities, in object buffer, int count);
        public abstract void InvokeRemoveCallback(in Entity entity, in object component);

        public abstract object GetData();
        public abstract Type DataType { get; }
    }

    public sealed class WorldData<T> : WorldData
    {
        public T data;
        public SetComponentEvent<T> setCallback;
        public SetComponentEventRefOnly<T> setRefCallback;
        public SetComponentEventCompOnly<T> setCompCallback;
        public RemoveComponentEvent<T> removeCallback;
        public RemoveComponentEventCompOnly<T> removeCompCallback;

        public readonly Queue<T> setQueue = new Queue<T>();

        public override void Set(in Entity entity, in StructureEventHandler handler)
        {
            handler.Set(entity, setQueue.Dequeue());
        }

        public override void Set(in Entity entity, object component, in StructureEventHandler handler)
        {
            handler.Set(entity, (T)component);
        }

        public override void Remove(in Entity entity, in StructureEventHandler handler) => handler.Remove<T>(entity);

        public override void InvokeRemoveCallbackAll(in Entity[] entities, in object buffer, int count)
        {
            var array = (T[])buffer;
            for (var i = 0; i < count; ++i)
                removeCallback?.Invoke(entities[i], array[i]);
        }

        public override void InvokeRemoveCallback(in Entity entity, in object comp)
            => removeCallback?.Invoke(entity, (T)comp);


        public void CallSetRefCallback(Entity entity, T oldComp, ref T newComp)
        {
            setRefCallback.Invoke(entity, ref newComp);
        }

        public void CallSetCompCallback(Entity entity, T oldComp, ref T newComp)
        {
            setCompCallback.Invoke(ref newComp);
        }

        public void CallRemoveCompCallback(Entity entity, T component)
            => removeCompCallback.Invoke(component);

        public override object GetData() => data;

        public override Type DataType => typeof(T);
    }

    public struct StructureEventHandler
    {
        public StructureEventHandler(WorldInfo world)
        {
            _cacheEvents = 0;
            _events = new Queue<EventData>();
            _world = world;
        }

        private readonly WorldInfo _world;
        private int _cacheEvents;

        public int EnqueueEvents
        {
            get => _cacheEvents;
            set
            {
                _cacheEvents = value;
                ExecuteEventPlayback();
            }
        }

        private readonly Queue<EventData> _events;

        private struct EventData
        {
            public EventType type;
            public Entity entity;
            public int typeID;
            public Archetype archetype;
            public World world;
        }

        private enum EventType
        {
            CreateEntity,
            DestroyEntity,
            SetComponent,
            RemoveComponent,
            TransferEntity,
            DestroyArchetype,
            DestroyWorld,
            ResizeBackingArrays,
        }

        public void ExecuteEventPlayback()
        {
            while (_cacheEvents == 0 && _events.Count > 0)
            {
                var e = _events.Dequeue();
                switch (e.type)
                {
                    case EventType.CreateEntity:
                    {
                        ref var archData = ref _world.archetypes[e.archetype.index];
                        if (archData.version == e.archetype.version)
                            SetUpEntity(e.entity, _world.archetypes[e.archetype.index].data);
                        else
                        {
                            Entities.all[e.entity.index].worldInfo = default;
                            Entities.Free.Enqueue(e.entity.index);
                        }
                    }
                        break;

                    case EventType.SetComponent:
                        _world.GetData(e.typeID).Set(e.entity, this);
                        break;

                    case EventType.RemoveComponent:
                        _world.GetData(e.typeID).Remove(e.entity, this);
                        break;

                    case EventType.DestroyEntity:
                        Destroy(e.entity);
                        break;

                    case EventType.TransferEntity:
                        Transfer(e.entity, e.world);
                        break;

                    case EventType.DestroyArchetype:
                        DestroyArchetype(e.archetype);
                        break;

                    case EventType.DestroyWorld:
                        DestroyWorld();
                        break;

                    case EventType.ResizeBackingArrays:
                        ResizeBackingArrays(e.archetype);
                        break;
                }
            }
        }

        public Entity CreateEntity(ArchetypeInfo archetypeData)
        {
            var index = 0;
            if (Entities.Free.Count > 0)
                index = Entities.Free.Dequeue();
            else
            {
                index = Entities.last;
                if (index == Entities.all.Length)
                    Array.Resize(ref Entities.all, index * 2);
                Entities.last++;
            }

            var version = Entities.all[index].version;
            var entity = new Entity(index, version);
            Entities.all[index].worldInfo = _world;

            if (_cacheEvents > 0)
            {
                Entities.all[index].version++;
                _events.Enqueue(new EventData { type = EventType.CreateEntity, entity = entity, archetype = archetypeData.archetype });
            }
            else SetUpEntity(entity, archetypeData);

            return entity;
        }
        
        public Entity CreateEntity(ArchetypeInfo archetypeData, int id)
        {
            while (id >= Entities.all.Length)
                Array.Resize(ref Entities.all, id * 2);

            var version = Entities.all[id].version;
            var entity = new Entity(id, version);
            Entities.all[id].worldInfo = _world;

            if (_cacheEvents > 0)
            {
                Entities.all[id].version++;
                _events.Enqueue(new EventData { type = EventType.CreateEntity, entity = entity, archetype = archetypeData.archetype });
            }
            else SetUpEntity(entity, archetypeData);

            return entity;
        }

        private void SetUpEntity(Entity entity, ArchetypeInfo archetypeData)
        {
            ref var entityData = ref Entities.all[entity.index];
            entityData.version = entity.version;
            entityData.archInfo = archetypeData;
            var archIndex = entityData.archIndex = archetypeData.entityCount;
            archetypeData.entityCount++;
            archetypeData.worldInfo.entityCount++;
            archetypeData.EnsureCapacity(archIndex);
            archetypeData.entities[archIndex] = entity;
        }

        public readonly void Set<TComponent>(in Entity entity, in TComponent component)
        {
            var worldData = _world.GetData<TComponent>();
            if (_cacheEvents > 0)
            {
                worldData.setQueue.Enqueue(component);
                _events.Enqueue(new EventData { type = EventType.SetComponent, entity = entity, typeID = TypeID<TComponent>.Value });
                return;
            }

            ref var entityInfo = ref Entities.all[entity.index];

            if (entityInfo.version != entity.version) return;

            if (entityInfo.archInfo.TryGetArray<TComponent>(out var buffer))
            {
                var index = entityInfo.archIndex;
                var old = buffer[index];
                buffer[index] = component;
                worldData.setCallback?.Invoke(entity, old, ref buffer[index]);
            }
            else
            {
                var oldIndex = entityInfo.archIndex;
                var archetype = entityInfo.archInfo;
                var lastIndex = --archetype.entityCount;
                var last = archetype.entities[oldIndex] = archetype.entities[lastIndex];
                Entities.all[last.index].archIndex = oldIndex; // reassign moved entity to to index

                // adding entity to target archetype
                var targetArchetype = entityInfo.archInfo = _world.GetArchetypeData(_world.bufferSignature.Copy(archetype.signature).Add<TComponent>());
                var targetIndex = entityInfo.archIndex = targetArchetype.entityCount;
                targetArchetype.EnsureCapacity(targetIndex);
                targetArchetype.entityCount++;

                // moving components over
                targetArchetype.entities[targetIndex] = entity;
                for (var i = 0; i < archetype.componentCount; ++i)
                    archetype.componentBuffers[i].buffer.Move(oldIndex, lastIndex, targetArchetype, targetIndex);

                // setting the added component and calling the callback event
                if (targetArchetype.TryGetArray<TComponent>(out var targetBuffer))
                {
                    targetBuffer[targetIndex] = component;
                    worldData.setCallback?.Invoke(entity, default, ref targetBuffer[targetIndex]);
                }
                else
                    throw new Exception("Frame Work Bug");
            }
        }

        public void Remove<TComponent>(in Entity entity)
        {
            var typeID = TypeID<TComponent>.Value;
            if (_cacheEvents > 0)
            {
                _events.Enqueue(new EventData { type = EventType.RemoveComponent, entity = entity, typeID = typeID });
            }
            else
            {
                ref var entityInfo = ref Entities.all[entity.index];

                if (entity.version != entityInfo.version) return;

                var oldArch = entityInfo.archInfo;

                if (!oldArch.TryGetArray<TComponent>(out var oldBuffer)) return; // if archetype already has component, just set and fire event

                var oldIndex = entityInfo.archIndex;

                var targetArch = _world.GetArchetypeData(_world.bufferSignature.Copy(oldArch.signature).Remove(typeID));
                var targetIndex = targetArch.entityCount;
                targetArch.entityCount++;
                targetArch.EnsureCapacity(targetIndex);

                oldArch.entityCount--;
                var lastIndex = oldArch.entityCount;
                var last = oldArch.entities[oldIndex] = oldArch.entities[lastIndex];
                Entities.all[last.index].archIndex = oldIndex;

                entityInfo.archIndex = targetIndex;
                entityInfo.archInfo = targetArch;

                targetArch.entities[targetIndex] = entity;
                var removed = oldBuffer[oldIndex];
                for (var i = 0; i < oldArch.componentCount; ++i)
                    oldArch.componentBuffers[i].buffer.Move(oldIndex, lastIndex, targetArch, targetIndex);
                _world.GetData<TComponent>().removeCallback?.Invoke(entity, removed);
            }
        }

        public void Transfer(Entity entity, World targetWorld)
        {
            if (_cacheEvents > 0)
                _events.Enqueue(new EventData { type = EventType.TransferEntity, entity = entity, world = targetWorld });
            else
            {
                ref var entityInfo = ref Entities.all[entity.index];

                if (entityInfo.version != entity.version
                    || entityInfo.archInfo.worldInfo.world == targetWorld
                    || !targetWorld.TryGetWorldInfo(out var targetWorldInfo)) return;

                var targetArch = targetWorldInfo.GetArchetypeData(entityInfo.archInfo.signature);
                var targetIndex = targetArch.entityCount;
                targetArch.EnsureCapacity(targetIndex);
                targetArch.entityCount++;
                targetArch.worldInfo.entityCount++;

                var oldIndex = entityInfo.archIndex;
                var oldArch = entityInfo.archInfo;
                var lastIndex = --oldArch.entityCount;
                --oldArch.worldInfo.entityCount;

                var last = oldArch.entities[oldIndex] = oldArch.entities[lastIndex];
                Entities.all[last.index].archIndex = oldIndex;
                targetArch.entities[targetIndex] = entity;

                for (var i = 0; i < oldArch.componentCount; ++i)
                    oldArch.componentBuffers[i].buffer.Move(oldIndex, lastIndex, targetArch.componentBuffers[i].buffer.array, targetIndex);

                entityInfo.archIndex = targetIndex;
                entityInfo.archInfo = targetArch;
                entityInfo.worldInfo = targetWorldInfo;
            }
        }

        public void Destroy(Entity entity)
        {
            if (_cacheEvents > 0)
                _events.Enqueue(new EventData { type = EventType.DestroyEntity, entity = entity });
            else
            {
                ref var entityInfo = ref Entities.all[entity.index];

                if (entityInfo.version != entity.version) return;

                entityInfo.version++;
                var oldArch = entityInfo.archInfo;
                var oldIndex = entityInfo.archIndex;
                --oldArch.entityCount;
                --_world.entityCount;
                var lastIndex = oldArch.entityCount;
                var last = oldArch.entities[oldIndex] = oldArch.entities[lastIndex]; // swap 
                Entities.all[last.index].archIndex = oldIndex;

                (WorldData callback, object value)[] removed = // this causes allocations
                    new (WorldData, object)[oldArch.componentCount]; // but other means are quite convuluted
                var length = 0;

                for (var i = 0; i < oldArch.componentCount; ++i)
                {
                    var pool = oldArch.componentBuffers[i];
                    var callback = _world.GetData(pool.typeID);
                    if (callback.hasRemoveCallback)
                    {
                        removed[length] = (callback, pool.buffer.array[entityInfo.archIndex]); // this causes boxing :(
                        length++;
                    }

                    pool.buffer.Remove(oldIndex, lastIndex);
                }

                entityInfo.version++;
                entityInfo.archInfo = default;
                entityInfo.worldInfo = default;
                Entities.Free.Enqueue(entity.index);

                for (var i = 0; i < length; ++i)
                    removed[i].callback.InvokeRemoveCallback(entity, removed[i].value);
            }
        }

        public void DestroyArchetype(Archetype archetype)
        {
            if (_cacheEvents > 0)
            {
                _events.Enqueue(new EventData { type = EventType.DestroyArchetype, archetype = archetype });
            }
            else
            {
                if (!archetype.TryGetArchetypeInfo(out var archInfo)) return;

                _world.entityCount -= archInfo.entityCount;
                _world.signatureToArchetypeIndex.Remove(archInfo.signature); // update archetype references
                _world.archetypes[archetype.index].version++;
                _world.archetypes[archetype.index].data = default;
                _world.freeArchetypes.Push(archetype.index);
                _world.archetypeStructureUpdateCount++;

                for (var i = 0; i < archInfo.entityCount; ++i) // remove entities from world
                {
                    var entity = archInfo.entities[i];
                    ref var info = ref Entities.all[entity.index];
                    info.version++;
                    info.archInfo = default;
                    info.worldInfo = default;
                    Entities.Free.Enqueue(entity.index);
                }

                for (var i = 0; i < archInfo.componentCount; ++i) // invoke callbacks
                {
                    var pool = archInfo.componentBuffers[i];
                    var callback = _world.GetData(pool.typeID);
                    if (callback.hasRemoveCallback)
                    {
                        callback.InvokeRemoveCallbackAll(archInfo.entities, pool.buffer.array, archInfo.entityCount);
                    }
                }
            }
        }

        public void DestroyWorld()
        {
            if (_cacheEvents > 0)
                _events.Enqueue(new EventData { type = EventType.DestroyWorld });
            else
            {
                ref var worldInfo = ref WorldInfo.All[_world.world.index];

                if (worldInfo.version != _world.world.version) return; // still needs to be checked incase multiple destorys are queued

                worldInfo.version++;
                var data = worldInfo.data;
                worldInfo.data = default;

                foreach (var archetype in data.archetypes) // delete all entities first
                {
                    var archInfo = archetype.data;
                    if (archInfo == null) continue;

                    for (var i = 0; i < archInfo.entityCount; ++i)
                    {
                        var index = archInfo.entities[i].index;
                        ref var info = ref Entities.all[index];
                        info.version++;
                        info.archInfo = default;
                        info.worldInfo = default;
                        Entities.Free.Enqueue(index);
                    }
                }

                foreach (var archetype in data.archetypes) // then do all their callbacks
                {
                    var archInfo = archetype.data;
                    if (archInfo == null) continue;
                    for (var i = 0; i < archInfo.componentCount; ++i)
                    {
                        var pool = archInfo.componentBuffers[i];
                        var worldData = data.GetData(pool.typeID);
                        if (worldData.hasRemoveCallback)
                        {
                            worldData.InvokeRemoveCallbackAll(archInfo.entities, pool.buffer.array, archInfo.entityCount);
                        }
                    }
                }
            }
        }

        public void ResizeBackingArrays(Archetype archetype)
        {
            if (_cacheEvents > 0)
                _events.Enqueue(new EventData { type = EventType.ResizeBackingArrays, archetype = archetype });
            else if (archetype.TryGetArchetypeInfo(out var info))
                info.ResizeBackingArrays();
        }
    }
}