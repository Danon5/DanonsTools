using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DanonsTools.ECSLayer
{
    /// <summary>
    /// Aggregates types into a unique signature
    /// </summary>
    public sealed class TypeSignature : IEquatable<TypeSignature>, IReadOnlyList<Type>
    {
        private int[] _typeIds;

        private TypeSignature(int capacity = 4)
        {
            _typeIds = new int[capacity];
        }

        /// <summary>
        /// number of types that make up the signature
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Creates a new type signature using the supplied types
        /// </summary>
        public TypeSignature(IEnumerable<Type> types)
        {
            _typeIds = new int[4];
            foreach (var type in types)
                Add(type);
        }

        /// <summary>
        /// Creates a new type signature that matches the supplied type signature
        /// </summary>
        /// <param name="signature"></param>
        public TypeSignature(TypeSignature signature)
        {
            Count = signature.Count;
            _typeIds = new int[Count + 1];

            for (var i = 0; i < Count; ++i)
            {
                _typeIds[i] = signature._typeIds[i];
            }
        }

        /// <summary>
        /// Creates a new type signature with the supplied types
        /// </summary>
        /// <param name="types"></param>
        public TypeSignature(params Type[] types)
        {
            _typeIds = new int[types.Length];
            foreach (var type in types)
                Add(type);
        }

        /// <summary>
        /// Create a new type signature with the same signature as the supplied archetype
        /// </summary>
        /// <param name="archetype"></param>
        public TypeSignature(Archetype archetype)
        {
            if (archetype.IsValid())
            {
                var signature = archetype.GetTypeSignature();
                _typeIds = new int[signature.Count + 1];
                Copy(signature);
            }
            else _typeIds = new int[2];
        }

        /// <summary>
        /// Clears signature to be an empty type
        /// </summary>
        public TypeSignature Clear()
        {
            Count = 0;
            return this;
        }

        public TypeSignature Add(params Type[] types)
        {
            foreach (var type in types)
                if (type != null)
                    Add(TypeID.Get(type));
            return this;
        }

        internal TypeSignature Add(int typeID)
        {
            for (var i = 0; i < Count; ++i)
            {
                if (_typeIds[i] == typeID) // if same exit
                    return this;

                if (typeID > _typeIds[i])  // since the hash is generated from this, ordering is important
                    (_typeIds[i], typeID) = (typeID, _typeIds[i]);
            }

            if (Count++ == _typeIds.Length)
                Array.Resize(ref _typeIds, Count + 4);

            _typeIds[Count - 1] = typeID;
            return this;
        }

        public TypeSignature Remove(params Type[] types)
        {
            foreach (var type in types)
                if (type != null)
                    Remove(TypeID.Get(type));
            return this;
        }

        internal TypeSignature Remove(int typeID)
        {
            var swap = _typeIds[0] == typeID;
            for (var i = 1; i < Count; ++i)
            {
                if (swap)
                    _typeIds[i - 1] = _typeIds[i];
                else
                    swap = _typeIds[i] == typeID;
            }
            if (swap)
                Count--;
            return this;
        }

        /// <summary>
        /// Makes this signature an exact copy of other signature
        /// </summary>
        public TypeSignature Copy(TypeSignature signature)
        {
            if (_typeIds.Length < signature.Count)
                Array.Resize(ref _typeIds, signature.Count + 1);
            for (var i = 0; i < signature.Count; ++i)
                _typeIds[i] = signature._typeIds[i];
            Count = signature.Count;
            return this;
        }

        /// <summary>
        /// Makes this type signature the same as the archetype's type signature
        /// </summary>
        public TypeSignature Copy(Archetype archetype)
            => Copy(archetype.GetTypeSignature());

        /// <summary>
        /// Adds type to the signature
        /// </summary>
        public TypeSignature Add(Type type)
            => Add(TypeID.Get(type));

        /// <summary>
        /// Adds type to the signature
        /// </summary>
        public TypeSignature Add<T>()
            => Add(TypeID<T>.Value);

        /// <summary>
        /// Removes type from signature
        /// </summary>
        public TypeSignature Remove(Type type)
            => Remove(TypeID.Get(type));

        /// <summary>
        /// Removes type from signature
        /// </summary>
        public TypeSignature Remove<T>()
            => Remove(TypeID<T>.Value);

        /// <summary>
        /// Returns true if signature has type
        /// </summary>
        public bool Has<T>() => Has(TypeID<T>.Value);

        /// <summary>
        /// Returns true if signature has type
        /// </summary>
        public bool Has(Type type) => Has(TypeID.Get(type));

        internal bool Has(int typeid)
        {
            for (var i = 0; i < Count; ++i)
                if (_typeIds[i] == typeid)
                    return true;
            return false;
        }

        /// <summary>
        /// Returns true if signatures have any types in common
        /// </summary>
        public bool HasAny(TypeSignature other)
        {
            for (var a = 0; a < Count; ++a)
            {
                for (var b = 0; b < other.Count; ++b)
                {
                    if (_typeIds[a] == other._typeIds[b])
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if this signature has all types contained in the other signature
        /// </summary>
        /// <returns></returns>
        public bool HasAll(TypeSignature other)
        {
            for (var a = 0; a < other.Count; ++a)
            {
                for (var b = 0; b < Count; ++b)
                {
                    if (other._typeIds[a] == _typeIds[b])
                        goto next;
                }
                return false;
                next:
                ;
            }
            return true;
        }

#pragma warning disable
        public override int GetHashCode()
        {
            const int prime = 53;
            var power = 1;
            var hashVal = 0;

            unchecked
            {
                for (var i = 0; i < Count; ++i)
                {
                    power *= prime;
                    hashVal = (hashVal + _typeIds[i] * power);
                }
            }
            return hashVal;
        }

        public bool Equals(TypeSignature other)
        {
            if (Count != other.Count)
                return false;
            for (var i = 0; i < Count; ++i)
            {
                if (_typeIds[i] != other._typeIds[i])
                    return false;
            }
            return true;
        }

        public override bool Equals(object obj)
            => obj is TypeSignature sig && sig.Equals(this);

        public override string ToString()
        {
            var sig = "Type Signature [";
            for (var i = 0; i < Count; ++i)
            {
                var type = TypeID.Get(_typeIds[i]);
                sig += $" {type.Name}";
            }
            sig += "]";

            return sig;
        }

        public string TypesToString()
        {
            var sig = "[";
            for (var i = 0; i < Count; ++i)
            {
                var type = TypeID.Get(_typeIds[i]);
                sig += $" {type.Name}";
            }
            sig += "]";

            return sig;
        }

        /// <summary>
        /// All types that currently make up this signature
        /// </summary>
        public IReadOnlyList<Type> Types => this;
        Type IReadOnlyList<Type>.this[int index] => TypeID.Get(_typeIds[index]);

        IEnumerator<Type> IEnumerable<Type>.GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
                yield return TypeID.Get(_typeIds[i]);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
                yield return TypeID.Get(_typeIds[i]);
        }

        int IReadOnlyCollection<Type>.Count => Count;
    }

    /// <summary>
    /// assigns ids to types
    /// </summary>
    public static class TypeID<T>
    {
        public static readonly int Value = TypeID.Get(typeof(T));
    }

    /// <summary>
    /// assigns ids to types
    /// </summary>
    public static class TypeID    // class to map types to ids
    {
        private static readonly Dictionary<Type, int> newIDs = new Dictionary<Type, int>();
        private static Type[] idToType = new Type[64];
        public static Type Get(int typeID) => idToType[typeID];

        public static int Get(Type type)
        {
            if (newIDs.TryGetValue(type, out var id)) return id;
            
            newIDs[type] = id = newIDs.Count + 1;
            if (id == idToType.Length)
                Array.Resize(ref idToType, idToType.Length * 2);
            idToType[id] = type;
            return id;
        }

        public static List<Type> GetAssignedTypes() => newIDs.Select(type => type.Key).ToList();
    }
}