using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace NgoUyenNguyen.GridSystem
{
    internal struct PathNode : INativeHeapItem<PathNode>
    {
        public int HeapIndex { get; set; }
        public int2 GridIndex;
        public int2 ParentIndex;
        public bool Walkable;
        public int G;
        public int H;
        private int F => G + H;


        public int CompareTo(PathNode other)
        {
            int cmp = F.CompareTo(other.F);
            if (cmp == 0) cmp = H.CompareTo(other.H);
            return cmp;
        }

        public bool Equals(PathNode other)
        {
            return GridIndex.Equals(other.GridIndex);
        }

        public override int GetHashCode()
        {
            return GridIndex.GetHashCode();
        }
    }

    internal struct PathRequest
    {
        public int2 From;
        public int2 To;
        public NeighborFilter Filter;
    }

    internal class GridPathfindingScheduler : MonoBehaviour
    {
        private CellLayout Layout { get; set; }
        private int2 MapSize { get; set; }
        private NativeArray<PathNode> pathNodeMap;
        public NativeList<PathRequest> PathRequests;
        private NativeList<JobHandle> pendingJobs;
        private NativeList<(PathRequest, NativeList<PathNode> path, NativeArray<bool> hasPath)> pendingResults;
        public event Action<int2, int2, NeighborFilter, bool, List<int2>> GiveBackResultEvent;

        
        private void Awake()
        {
            pathNodeMap = new NativeArray<PathNode>(0, Allocator.Persistent);
            PathRequests = new NativeList<PathRequest>(Allocator.Persistent);
            pendingJobs = new NativeList<JobHandle>(Allocator.Persistent);
            pendingResults =
                new NativeList<(PathRequest, NativeList<PathNode> path, NativeArray<bool> hasPath)>(
                    Allocator.Persistent);
        }

        private void LateUpdate()
        {
            ReturnFinishedJobs();
            SchedulePathfindingJobs();
        }

        private void ReturnFinishedJobs()
        {
            for (int i = pendingJobs.Length - 1; i >= 0; i--)
            {
                if (pendingJobs[i].IsCompleted)
                {
                    pendingJobs[i].Complete();
                    var (request, path, hasPath) = pendingResults[i];
                    var fromIndex = request.From;
                    var toIndex = request.To;
                    var filter = request.Filter;
                    var hasPathResult = hasPath[0];
                    var result = new List<int2>();
                    foreach (var node in path)
                    {
                        result.Add(node.GridIndex);
                    }

                    GiveBackResultEvent?.Invoke(fromIndex, toIndex, filter, hasPathResult, result);

                    path.Dispose();
                    hasPath.Dispose();
                    pendingJobs.RemoveAt(i);
                    pendingResults.RemoveAt(i);
                }
            }
        }

        public void Grid_OnUpdatePathfindingDataEvent(Cell[] cellMap, Vector2Int mapSize, CellLayout layout)
        {
            JobHandle.CompleteAll(pendingJobs.AsArray());
            Layout = layout;
            MapSize = new int2(mapSize.x, mapSize.y);
            pathNodeMap.Dispose();
            pathNodeMap = new NativeArray<PathNode>(cellMap.Length, Allocator.Persistent);
            for (int i = 0; i < cellMap.Length; i++)
            {
                var pn = new PathNode();
                if (cellMap[i] is null)
                {
                    pn.Walkable = false;
                    continue;
                }

                pn.Walkable = cellMap[i].Walkable;
                int2 index = new int2(cellMap[i].index.x, cellMap[i].index.y);
                pn.GridIndex = index;
                pathNodeMap[i] = pn;
            }
        }

        private void SchedulePathfindingJobs()
        {
            for (int i = 0; i < PathRequests.Length; i++)
            {
                var openSet = new NativeHeap<PathNode>(Allocator.TempJob);
                var closedSet = new NativeHashSet<PathNode>(pathNodeMap.Length / 2, Allocator.TempJob);
                var path = new NativeList<PathNode>(Allocator.Persistent);
                var hasPath = new NativeArray<bool>(1, Allocator.Persistent);
                var gridMap = new NativeArray<PathNode>(pathNodeMap, Allocator.TempJob);
                
                var jobHandle = Layout switch
                {
                    CellLayout.Square => new SquarePathfindingJob
                    {
                        Request = PathRequests[i],
                        MapSize = MapSize,
                        GridMap = gridMap,
                        Path = path,
                        HasPath = hasPath,
                        OpenSet = openSet,
                        ClosedSet = closedSet,
                    }.Schedule(),
                    CellLayout.Hexagon => new HexagonPathfindingJob
                    {
                        Request = PathRequests[i],
                        MapSize = MapSize,
                        GridMap = gridMap,
                        Path = path,
                        HasPath = hasPath,
                        OpenSet = openSet,
                        ClosedSet = closedSet,
                    }.Schedule(),
                    _ => throw new NotImplementedException()
                };
                pendingJobs.Add(jobHandle);
                pendingResults.Add((PathRequests[i], path, hasPath));

                openSet.Dispose(jobHandle);
                closedSet.Dispose(jobHandle);
                gridMap.Dispose(jobHandle);
            }

            PathRequests.Clear();
        }

        private void OnDestroy()
        {
            JobHandle.CompleteAll(pendingJobs.AsArray());
            pendingJobs.Dispose();
            foreach (var (_, path, hasPath) in pendingResults)
            {
                path.Dispose();
                hasPath.Dispose();
            }

            pendingResults.Dispose();
            pathNodeMap.Dispose();
            PathRequests.Dispose();
        }
    }

    [BurstCompile]
    internal struct SquarePathfindingJob : IJob
    {
        [ReadOnly] public PathRequest Request;
        [ReadOnly] public int2 MapSize;
        [WriteOnly] public NativeArray<bool> HasPath;
        public NativeList<PathNode> Path;
        public NativeArray<PathNode> GridMap;
        public NativeHeap<PathNode> OpenSet;
        public NativeHashSet<PathNode> ClosedSet;


        public void Execute()
        {
            var startNode = GridMap[Request.From.x + Request.From.y * MapSize.x];
            var goalNode = GridMap[Request.To.x + Request.To.y * MapSize.x];
            if (!startNode.Walkable || !goalNode.Walkable)
            {
                HasPath[0] = false;
                return;
            }

            OpenSet.Push(startNode);
            while (OpenSet.Count > 0)
            {
                var currentNode = OpenSet.Pop();
                ClosedSet.Add(currentNode);

                // If reach the goal return the path
                if (currentNode.Equals(goalNode))
                {
                    RetracePath(startNode, currentNode);
                    HasPath[0] = true;
                    return;
                }

                // Evaluate each neighbor of the current node
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        // Skip current node itself
                        if (x == 0 && y == 0) continue;
                        // Filter
                        if (Request.Filter == NeighborFilter.DiagonalOnly &&
                            ((x != 0 && y == 0) || (x == 0 && y != 0))) continue;
                        if (Request.Filter == NeighborFilter.OrthogonalOnly &&
                            x != 0 && y != 0) continue;

                        var neighborIndex = currentNode.GridIndex + new int2(x, y);
                        if (neighborIndex.x < 0 ||
                            neighborIndex.x >= MapSize.x ||
                            neighborIndex.y < 0 ||
                            neighborIndex.y >= MapSize.y)
                            continue;
                        var neighborNode = GridMap[neighborIndex.x + neighborIndex.y * MapSize.x];

                        // Skip unwalkable neighbor and neighbor already in the closed set
                        if (!neighborNode.Walkable || ClosedSet.Contains(neighborNode)) continue;

                        // Calculate the cost to reach the neighbor node
                        var newNeighborCost = currentNode.G + GetDistance(currentNode, neighborNode, Request.Filter);
                        if (newNeighborCost < neighborNode.G || !OpenSet.Contains(neighborNode))
                        {
                            neighborNode.G = newNeighborCost;
                            neighborNode.H = GetDistance(neighborNode, goalNode, Request.Filter);
                            neighborNode.ParentIndex = currentNode.GridIndex;
                            GridMap[neighborIndex.x + neighborIndex.y * MapSize.x] = neighborNode;

                            if (!OpenSet.Contains(neighborNode))
                                OpenSet.Push(neighborNode);
                        }
                    }
                }
            }

            HasPath[0] = false;
        }

        private int GetDistance(PathNode a, PathNode b, NeighborFilter filter)
        {
            if (filter == NeighborFilter.None)
            {
                int distX = math.abs(a.GridIndex.x - b.GridIndex.x);
                int distY = math.abs(a.GridIndex.y - b.GridIndex.y);

                int min = math.min(distX, distY);
                int max = math.max(distX, distY);
                return 14 * min + 10 * (max - min);
            }

            return 1;
        }

        private void RetracePath(PathNode startNode, PathNode goalNode)
        {
            var currentNode = goalNode;
            while (!currentNode.Equals(startNode))
            {
                Path.Add(currentNode);
                currentNode = GridMap[currentNode.ParentIndex.x + currentNode.ParentIndex.y * MapSize.x];
            }

            Path.Add(startNode);
            Path.Reverse();
        }
    }

    [BurstCompile]
    internal struct HexagonPathfindingJob : IJob
    {
        [ReadOnly] public PathRequest Request;
        [ReadOnly] public int2 MapSize;
        [WriteOnly] public NativeArray<bool> HasPath;
        public NativeList<PathNode> Path;
        public NativeArray<PathNode> GridMap;
        public NativeHeap<PathNode> OpenSet;
        public NativeHashSet<PathNode> ClosedSet;


        public void Execute()
        {
            var startNode = GridMap[Request.From.x + Request.From.y * MapSize.x];
            var goalNode = GridMap[Request.To.x + Request.To.y * MapSize.x];
            if (!startNode.Walkable || !goalNode.Walkable)
            {
                HasPath[0] = false;
                return;
            }

            OpenSet.Push(startNode);
            while (OpenSet.Count > 0)
            {
                var currentNode = OpenSet.Pop();
                ClosedSet.Add(currentNode);

                // If reach the goal, return the path
                if (currentNode.Equals(goalNode))
                {
                    RetracePath(startNode, currentNode);
                    HasPath[0] = true;
                    return;
                }

                // Evaluate each neighbor of the current node
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        // Skip current node itself
                        if (x == y) continue;

                        var neighborIndex = currentNode.GridIndex + new int2(x, y);
                        if (neighborIndex.x < 0 ||
                            neighborIndex.x >= MapSize.x ||
                            neighborIndex.y < 0 ||
                            neighborIndex.y >= MapSize.y)
                            continue;
                        var neighborNode = GridMap[neighborIndex.x + neighborIndex.y * MapSize.x];

                        // Skip unwalkable neighbor and neighbor already in the closed set
                        if (!neighborNode.Walkable || ClosedSet.Contains(neighborNode)) continue;

                        // Calculate the cost to reach the neighbor node
                        var newNeighborCost = currentNode.G + GetDistance(currentNode, neighborNode);
                        if (newNeighborCost < neighborNode.G || !OpenSet.Contains(neighborNode))
                        {
                            neighborNode.G = newNeighborCost;
                            neighborNode.H = GetDistance(neighborNode, goalNode);
                            neighborNode.ParentIndex = currentNode.GridIndex;
                            GridMap[neighborIndex.x + neighborIndex.y * MapSize.x] = neighborNode;

                            if (!OpenSet.Contains(neighborNode))
                                OpenSet.Push(neighborNode);
                        }
                    }
                }
            }

            HasPath[0] = false;
        }

        private int GetDistance(PathNode a, PathNode b)
        {
            return 1;
        }

        private void RetracePath(PathNode startNode, PathNode goalNode)
        {
            var currentNode = goalNode;
            while (!currentNode.Equals(startNode))
            {
                Path.Add(currentNode);
                currentNode = GridMap[currentNode.ParentIndex.x + currentNode.ParentIndex.y * MapSize.x];
            }

            Path.Add(startNode);
            Path.Reverse();
        }
    }
}