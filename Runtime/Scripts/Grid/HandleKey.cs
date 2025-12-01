using System;
using UnityEngine;

namespace NgoUyenNguyen.Grid
{
    internal readonly struct HandleKey : IEquatable<HandleKey>
    {
        public readonly Vector2Int FromIndex;
        public readonly Vector2Int ToIndex;
        public readonly NeighborFilter Filter;

        public HandleKey(Vector2Int fromIndex, Vector2Int toIndex, NeighborFilter filter)
        {
            FromIndex = fromIndex;
            ToIndex = toIndex;
            Filter = filter;
        }

        public bool Equals(HandleKey other)
        {
            return FromIndex == other.FromIndex && 
                   ToIndex == other.ToIndex && 
                   Filter == other.Filter;
        }

        public override bool Equals(object obj)
        {
            return obj is HandleKey other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(FromIndex, ToIndex, Filter);
        }
    }
}