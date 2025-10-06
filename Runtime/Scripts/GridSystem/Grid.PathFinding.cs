using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace NgoUyenNguyen.GridSystem
{
    /// <summary>
    /// Represents a handle for a pathfinding operation within a grid system.
    /// </summary>
    /// <typeparam name="T">The type of cells used in the grid, where cells must inherit from <see cref="Cell"/>.</typeparam>
    public class PathHandle<T> where T : Cell
    {
        /// <summary>
        /// Indicates whether the pathfinding operation associated with this handle has been completed.
        /// When true, the operation has finished processing, and results are available.
        /// </summary>
        public bool IsComplete { get; private set;}

        /// <summary>
        /// Provides the result of a pathfinding operation, containing details about the path's existence
        /// and the list of cells that form the computed path, if any.
        /// </summary>
        public Result<T> Result { get; private set; }

        /// <summary>
        /// An event triggered upon the completion of the pathfinding operation associated with the current handle.
        /// Subscribing to this event allows retrieval of the resulting pathfinding data when the operation is finalized.
        /// </summary>
        public event Action<Result<T>> OnComplete;
        internal void Complete(Result<T> result)
        {
            Result = result;
            IsComplete = true;
            OnComplete?.Invoke(result);
        }
    }

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

    public struct Result<T> where T : Cell
    {
        /// <summary>
        /// Indicates whether a valid path exists between the specified start and end points
        /// in the pathfinding result.
        /// </summary>
        public readonly bool HasPath;

        /// <summary>
        /// Represents the calculated sequence of cells forming a path within the grid system.
        /// This variable contains the ordered set of grid cells, starting from the origin cell and ending at the destination cell,
        /// which constitutes the shortest or most efficient route as determined by the pathfinding algorithm.
        /// </summary>
        public readonly List<T> Path;

        public Result(bool hasPath, List<T> path)
        {
            HasPath = hasPath;
            Path = path;
        }
    }
    
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
