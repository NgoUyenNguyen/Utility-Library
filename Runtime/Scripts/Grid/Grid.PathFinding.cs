using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace NgoUyenNguyen.Grid
{
    public partial class Grid<T> where T : Cell
    {
        private GridPathfindingScheduler gps;
        private readonly Dictionary<HandleKey, PathHandle<T>> activeRequests = new();
        public event Action<Cell[], Vector2Int, CellLayout> UpdatePathfindingDataEvent;

        private GridPathfindingScheduler GPS
        {
            get
            {
                if (gps != null) return gps;
                gps = gameObject.AddComponent<GridPathfindingScheduler>();
                gps.GiveBackResultEvent += PathfindingManager_OnGiveBackResultEvent;
                UpdatePathfindingDataEvent += gps.Grid_OnUpdatePathfindingDataEvent;
                UpdatePathFindingData();
                return gps;
            }
        }
        public int RunningPathfindingCount => GPS.RunningPathfindingCount;
        
        private void PathfindingManager_OnGiveBackResultEvent(int2 from, int2 to, NeighborFilter filter, bool hasPath, List<int2> result)
        {
            var key = new HandleKey(new Vector2Int(from.x, from.y), new Vector2Int(to.x, to.y), filter);
            var path = new List<T>();
            foreach (var index in result)
            {
                path.Add(this[index.x, index.y]);
            }
            
            activeRequests[key].Complete(new Result<T>(hasPath, path));
            activeRequests.Remove(key);
        }

        /// <summary>
        /// Requests a pathfinding operation between two specified cells.
        /// </summary>
        /// <param name="from">The starting cell for the path request.</param>
        /// <param name="to">The destination cell for the path request.</param>
        /// <param name="filter">Optional filter that determines the type of neighbor connections to consider (e.g., OrthogonalOnly or DiagonalOnly).</param>
        /// <returns>A <see cref="PathHandle{T}"/> object representing the requested path operation.</returns>
        public PathHandle<T> RequestPath(T from, T to, NeighborFilter filter = NeighborFilter.None)
        {
            if (!cellMap.Contains(from) || !cellMap.Contains(to))
            {
                throw new ArgumentException($"{nameof(from)} and {nameof(to)} must be in the grid!");
            }

            var fromIndex = from.index;
            var toIndex = to.index;
            
            var key = new HandleKey(fromIndex, toIndex, filter);
            if (activeRequests.TryGetValue(key, out var existing))
            {
                return existing;
            }
            
            AddPathRequest(fromIndex, toIndex, filter);

            var handle = new PathHandle<T>();
            activeRequests.Add(key ,handle);
            return handle;
        }

        private void AddPathRequest(Vector2Int fromIndex, Vector2Int toIndex, NeighborFilter filter)
        {
            GPS.PathRequests.Add(new PathRequest
            {
                From = new int2(fromIndex.x, fromIndex.y),
                To = new int2(toIndex.x, toIndex.y),
                Filter = filter,
            });
        }

        /// <summary>
        /// Updates the internal pathfinding data for the grid.
        /// </summary>
        public void UpdatePathFindingData()
        {
            UpdatePathfindingDataEvent?.Invoke(cellMap, size, layout);
        }
    }
}
