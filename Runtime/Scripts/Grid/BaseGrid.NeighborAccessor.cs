using System.Collections.Generic;
using UnityEngine;
using ZLinq;

namespace NgoUyenNguyen.Grid
{
    public partial class BaseGrid
    {
        /// <summary>
        /// Method to get cell from <paramref name="cell"/> and <paramref name="indexDelta"/>
        /// </summary>
        /// <remarks>
        /// For example, (1,0) will return the right neighbor<br/>
        /// and (2,0) will return the right second neighbor.
        /// </remarks>
        /// <param name="cell">Origin cell</param>
        /// <param name="indexDelta">index delta from origin cell to neighbor.
        /// </param>
        /// <returns>Neighbor cell from <paramref name="cell"/> plus <paramref name="indexDelta"/></returns>
        public T GetNeighbor<T>(T cell, Vector2Int indexDelta) where T : Cell
        {
            if (cell == null || !CellMap.AsValueEnumerable().Contains(cell)) return null;

            var neighborIndex = cell.index + indexDelta;
            if (_layout == CellLayout.Hexagon) neighborIndex = AxialToIndex(neighborIndex);
            if (neighborIndex.x < 0 ||
                neighborIndex.x >= Size.x ||
                neighborIndex.y < 0 ||
                neighborIndex.y >= Size.y)
            {
                return null;
            }

            return _cellMap[neighborIndex.x + neighborIndex.y * Size.x] as T;
        }


        /// <summary>
        /// Method to get neighbors surrounding <paramref name="cell"/>
        /// </summary>
        /// <param name="cell">Origin <c>Cell</c></param>
        /// <param name="filter">
        /// Filter mode:
        /// <list type="bullet">
        /// <item><see cref="NeighborFilter.None"/> = All neighbors</item>
        /// <item><see cref="NeighborFilter.DiagonalOnly"/> = Only choose <c>Diagonal</c> neighbors</item>
        /// <item><see cref="NeighborFilter.OrthogonalOnly"/> = Only choose <c>Orthogonal</c> neighbors</item>
        /// </list>
        /// </param>
        /// <remarks>
        /// <c>NOTICE: filter only works with Square Grid. Otherwise, it always returns all neighbors</c>
        /// </remarks>
        /// <returns><see cref="HashSet{T}"/> of neighbors</returns>
        public IEnumerable<T> GetNeighbors<T>(T cell, NeighborFilter filter = NeighborFilter.None) where T : Cell
        {
            if (cell == null || !CellMap.AsValueEnumerable().Contains(cell)) return null;

            switch (_layout)
            {
                case CellLayout.Square:
                    return filter switch
                    {
                        NeighborFilter.OrthogonalOnly => Square_GetNeighborsOrthogonal(cell),
                        NeighborFilter.DiagonalOnly => Square_GetNeighborsDiagonal(cell),
                        _ => Square_GetAllNeighbors(cell)
                    };
                case CellLayout.Hexagon:
                    return Hexagon_GetNeighbors(cell);
            }

            return null;
        }

        /// <summary>
        /// Method to get all cells in a ring around one cell
        /// </summary>
        /// <param name="cell">Center <c>Cell</c> of the ring </param>
        /// <param name="radius">Radius of the ring</param>
        /// <returns><see cref="HashSet{T}"/> Ring</returns>
        public IEnumerable<T> GetRing<T>(T cell, int radius) where T : Cell
        {
            if (cell == null || !CellMap.AsValueEnumerable().Contains(cell)) return null;

            return GetRing<T>(cell.index, radius);
        }

        /// <summary>
        /// Method to get all cells in a ring around one index
        /// </summary>
        /// <param name="center">Center index of the ring</param>
        /// <param name="radius">Radius of the ring</param>
        /// <returns><see cref="HashSet{T}"/> Ring</returns>
        public IEnumerable<T> GetRing<T>(Vector2Int center, int radius) where T : Cell
        {
            return _layout switch
            {
                CellLayout.Square => Square_GetRing<T>(center, radius),
                CellLayout.Hexagon => Hexagon_GetRing<T>(center, radius),
                _ => null
            };
        }

        #region Square

        private IEnumerable<T> Square_GetRing<T>(Vector2Int center, int radius) where T : Cell
        {
            var ring = new HashSet<T>();
            for (var x = -radius; x <= radius; x++)
            {
                for (var y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius) continue;

                    var neighborIndex = center + new Vector2Int(x, y);
                    var neighbor = default(T);
                    if (neighborIndex.x >= 0 &&
                        neighborIndex.x < _size.x &&
                        neighborIndex.y >= 0 &&
                        neighborIndex.y < _size.y)
                    {
                        neighbor = _cellMap[neighborIndex.x + neighborIndex.y * _size.x] as T;
                    }

                    if (neighbor != null)
                    {
                        ring.Add(neighbor);
                    }
                }
            }

            return ring;
        }

        private IEnumerable<T> Square_GetAllNeighbors<T>(T cell) where T : Cell
        {
            HashSet<T> neighbors = new();

            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue; // Skip cell itself

                    Cell neighbor = GetNeighbor(cell, new Vector2Int(x, y));

                    if (neighbor != null) // Ignore null neighbors
                    {
                        neighbors.Add((T)neighbor);
                    }
                }
            }

            return neighbors;
        }

        private IEnumerable<T> Square_GetNeighborsDiagonal<T>(T cell) where T : Cell
        {
            HashSet<T> neighbors = new();

            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    if ((x == 0 && y == 0) || (x != 0 && y == 0) || (x == 0 && y != 0))
                        continue; // Skip cell itself and inline neighbors

                    Cell neighbor = GetNeighbor(cell, new Vector2Int(x, y));

                    if (neighbor != null) // Ignore null neighbors
                    {
                        neighbors.Add((T)neighbor);
                    }
                }
            }

            return neighbors;
        }

        private IEnumerable<T> Square_GetNeighborsOrthogonal<T>(T cell) where T : Cell
        {
            HashSet<T> neighbors = new();

            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    if ((x == 0 && y == 0) || (x != 0 && y != 0)) continue; // Skip cell itself and diagonal neighbors

                    Cell neighbor = GetNeighbor(cell, new Vector2Int(x, y));

                    if (neighbor != null) // Ignore null neighbors
                    {
                        neighbors.Add((T)neighbor);
                    }
                }
            }

            return neighbors;
        }

        #endregion

        #region Hexagon

        public HashSet<T> Hexagon_GetNeighbors<T>(T cell) where T : Cell
        {
            HashSet<T> neighbors = new();
            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    if (x == y) continue;
                    Cell neighbor = GetNeighbor(cell, new Vector2Int(x, y));

                    if (neighbor != null) // Ignore null neighbors
                    {
                        neighbors.Add((T)neighbor);
                    }
                }
            }

            return neighbors;
        }

        private HashSet<T> Hexagon_GetRing<T>(Vector2Int center, int radius) where T : Cell
        {
            var ring = new HashSet<T>();
            for (var x = -radius; x <= radius; x++)
            {
                for (var y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x + y) > radius ||
                        (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius && Mathf.Abs(x + y) != radius))
                        continue;
                    var neighborIndex = AxialToIndex(center + new Vector2Int(x, y));
                    var neighbor = default(T);
                    if (neighborIndex.x >= 0 &&
                        neighborIndex.x < _size.x &&
                        neighborIndex.y >= 0 &&
                        neighborIndex.y < _size.y)
                    {
                        neighbor = _cellMap[neighborIndex.x + neighborIndex.y * _size.x] as T;
                    }

                    if (neighbor != null)
                    {
                        ring.Add(neighbor);
                    }
                }
            }

            return ring;
        }

        #endregion
    }
}