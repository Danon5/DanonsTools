using System;
using System.Collections;
using System.Collections.Generic;

namespace DanonsTools.ECSLayer
{
    /// <summary>
    /// Operates on all entities that match it's filters
    /// </summary>
    public partial class Query : IEnumerable<Archetype>
    {
        /// <summary>
        /// the world the query operates on
        /// </summary>
        public World World
        {
            get => _world;
            set
            {
                _structureUpdate = -1;
                _world = value;
            }
        }


        private readonly TypeSignature _include = new TypeSignature();
        private readonly TypeSignature _exclude = new TypeSignature();

        private Archetype[] _matchingArchetypes = new Archetype[8];
        private World _world;
        private int _lastLookup, _structureUpdate, _archetypeCount;

        public Query(in World world)
        {
            _world = world;
        }

        public static implicit operator bool(Query query) => query == null ? false : query._world;

        /// <summary>
        /// Returns a copy of all archetypes matching the query
        /// </summary>
        public Archetype[] GetArchetypes()
        {
            if (Update(out var worldInfo))
            {
                var archetypes = new Archetype[_archetypeCount];
                for (var i = 0; i < _archetypeCount; ++i)
                    archetypes[i] = _matchingArchetypes[i];
                return archetypes;
            }

            return Array.Empty<Archetype>();
        }

        /// <summary>
        /// Returns a copy of all entities matching the query
        /// </summary>
        public Entity[] GetEntities()
        {
            if (!Update(out var worldInfo)) return Array.Empty<Entity>();

            var entities = new Entity[EntityCount];
            var count = 0;

            for (var i = 0; i < _archetypeCount; ++i)
            {
                if (!_matchingArchetypes[i].TryGetArchetypeInfo(out var archInfo)) continue;

                for (var e = 0; e < archInfo.entityCount; ++e)
                {
                    entities[count] = archInfo.entities[e];
                    count++;
                }
            }

            return entities;
        }

        /// <summary>
        /// filters entities to those that have component
        /// </summary>
        public Query With<T>()
        {
            _archetypeCount = 0;
            _structureUpdate = -1;
            _include.Add<T>();
            return this;
        }


        /// <summary>
        /// filters entities to those that do not have component
        /// </summary>
        public Query Without<T>()
        {
            _archetypeCount = 0;
            _structureUpdate = -1;
            _exclude.Add<T>();
            return this;
        }

        /// <summary>
        /// filters entities to those that have components
        /// </summary>
        public Query With(params Type[] types)
        {
            _archetypeCount = 0;
            _structureUpdate = -1;
            _include.Add(types);
            return this;
        }

        /// <summary>
        /// filters entities to those that do not have components
        /// </summary>
        public Query Without(params Type[] types)
        {
            _archetypeCount = 0;
            _structureUpdate = -1;
            _exclude.Add(types);
            return this;
        }

        /// <summary>
        /// filters entities to those that have components
        /// </summary>
        public Query With(IEnumerable<Type> types)
        {
            if (types == null) return this;

            foreach (var type in types)
                With(type);
            return this;
        }

        /// <summary>
        /// filters entities to those that do not have components
        /// </summary>
        public Query Without(IEnumerable<Type> types)
        {
            if (types == null) return this;

            foreach (var type in types)
                Without(type);
            return this;
        }

        /// <summary>
        /// clears all previous filters on the query
        /// </summary>
        public Query Clear()
        {
            _include.Clear();
            _exclude.Clear();
            _archetypeCount = 0;
            _structureUpdate = -1;
            return this;
        }

        /// <summary>
        /// iterates and peforms action on all entities that match the query
        /// </summary>
        public void ForEach(in Action<Entity> action)
        {
            if (!Update(out var worldInfo)) return;

            worldInfo.StructureEvents.EnqueueEvents++;
            for (var archetypeIndex = 0; archetypeIndex < _archetypeCount; ++archetypeIndex)
            {
                var archetype = worldInfo.archetypes[_matchingArchetypes[archetypeIndex]].data;
                var count = archetype.entityCount;
                var entities = archetype.entities;
                if (count > 0)
                {
                    for (var e = 0; e < count; ++e)
                        action(entities[e]);
                }
            }

            worldInfo.StructureEvents.EnqueueEvents--;
        }

        /// <summary>
        /// Destroys matching archetypes along with their entities.
        /// Most efficient way to destroy entities.
        /// </summary>
        public void DestroyMatching()
        {
            if (!Update(out var worldInfo)) return;

            foreach (var archetype in GetArchetypes()) // using a copy is safer
                archetype.Destroy();
        }

        // keeps the queried archtypes up to date, return false if the query is not valid
        private bool Update(out WorldInfo worldInfo)
        {
            if (_world.TryGetWorldInfo(out worldInfo))
            {
                if (worldInfo.archetypeStructureUpdateCount != _structureUpdate)
                {
                    _lastLookup = 0;
                    _archetypeCount = 0;
                    _structureUpdate = worldInfo.archetypeStructureUpdateCount;
                }

                for (; _lastLookup < worldInfo.archetypeTerminatingIndex; ++_lastLookup)
                {
                    var arch = worldInfo.archetypes[_lastLookup].data;
                    if (arch == null) continue;
                    if (arch.signature.HasAll(_include) && !arch.signature.HasAny(_exclude))
                    {
                        if (_archetypeCount == _matchingArchetypes.Length)
                            Array.Resize(ref _matchingArchetypes, _archetypeCount * 2);
                        _matchingArchetypes[_archetypeCount] = arch.archetype;
                        ++_archetypeCount;
                    }
                }

                return true;
            }

            _structureUpdate = -1;
            _archetypeCount = 0;
            return false;
        }

        /// <summary>
        /// the total number of entities that currently match the query
        /// </summary>
        /// <value></value>
        public int EntityCount
        {
            get
            {
                var count = 0;

                if (!Update(out var worldInfo)) return count;

                for (var i = 0; i < _archetypeCount; ++i)
                    count += worldInfo.archetypes[_matchingArchetypes[i]].data.entityCount;
                return count;
            }
        }

        public override string ToString()
        {
            return "Query" +
                   (_include.Count > 0 ? $" -> Has {_include.TypesToString()}" : "") +
                   (_exclude.Count > 0 ? $" -> Not {_exclude.TypesToString()}" : "");
        }

        /// <summary>
        /// returns all the types in the queries' has filter
        /// </summary>
        public IReadOnlyList<Type> GetHasFilterTypes() => _include.Types;

        /// <summary>
        /// returns all the types in the queries' not filter
        /// </summary>
        public IReadOnlyList<Type> GetNotFilterTypes() => _exclude.Types;


        IEnumerator<Archetype> IEnumerable<Archetype>.GetEnumerator()
        {
            if (!Update(out var info)) yield break;

            for (var i = 0; i < _archetypeCount; ++i)
            {
                yield return info.archetypes[_matchingArchetypes[i]].data.archetype;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (!Update(out var info)) yield break;

            for (var i = 0; i < _archetypeCount; ++i)
            {
                yield return info.archetypes[_matchingArchetypes[i]].data.archetype;
            }
        }
    }
}