using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NgoUyenNguyen.Grid
{
    /// <summary>
    /// Represents a grid structure composed of cells, supporting various alignment and spacing options.
    /// </summary>
    /// <remarks>The <see cref="Grid"/> class provides functionality for creating, managing, and
    /// interacting with a grid of cells. Likely a collection, 
    /// <see cref="Grid"/> can access cells through index like a 2D array and iterated by foreach loop</remarks>
    public abstract partial class Grid<T> : BaseGrid, IEnumerable<T> where T : Cell
    {
        /// <summary>
        /// Access <c>Cell</c> in <c>Grid</c> through (<paramref name="x"/>, <paramref name="y"/>) coordinate
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public new T this[int x, int y] => base[x, y] as T;

        /// <summary>
        /// Access <c>Cell</c> in <c>Grid</c> through index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public new T this[Vector2Int index] => base[index] as T;

        /// <summary>
        /// Method to get <c>Cell</c> from <c>LocalSpace</c> position
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <remarks>
        /// If input position is out of bounds, return the nearest <c>Cell</c>
        /// </remarks>
        /// <param name="localPos"></param>
        /// <returns>Nearest <c>Cell</c></returns>
        public new T LocalToCell(Vector3 localPos)
        {
            return base.LocalToCell(localPos) as T;
        }

        /// <summary>
        /// Method to get <c>Cell</c> from <c>WorldSpace</c> position
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <remarks>
        /// If input position is out of bounds, return the nearest <c>Cell</c>
        /// </remarks>
        /// <param name="worldPos"></param>
        /// <returns>Nearest <c>Cell</c></returns>
        public new T WorldToCell(Vector3 worldPos)
        {
            return base.WorldToCell(worldPos) as T;
        }
        
        

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var t in cellMap)
            {
                if (t == null) continue;
                yield return t as T;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
