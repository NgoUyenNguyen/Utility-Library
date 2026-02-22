using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace NgoUyenNguyen.Grid
{
    internal class GridPathfindingScheduler : MonoBehaviour
    {
        private CellLayout Layout { get; set; }
        private int2 MapSize { get; set; }
        private NativeArray<PathNode> pathNodeMap;
        public NativeList<PathRequest> PathRequests;
        private NativeList<JobHandle> runningJobs;
        private NativeList<(PathRequest, NativeList<PathNode> path, NativeArray<bool> hasPath)> pendingResults;
        public event Action<int2, int2, NeighborFilter, bool, List<int2>> GiveBackResultEvent;
        
        public int RunningPathfindingCount => runningJobs.Length;
        
        
        private void Awake()
        {
            pathNodeMap = new NativeArray<PathNode>(0, Allocator.Persistent);
            PathRequests = new NativeList<PathRequest>(Allocator.Persistent);
            runningJobs = new NativeList<JobHandle>(Allocator.Persistent);
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
            for (var i = runningJobs.Length - 1; i >= 0; i--)
            {
                if (!runningJobs[i].IsCompleted) continue;
                runningJobs[i].Complete();
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
                runningJobs.RemoveAt(i);
                pendingResults.RemoveAt(i);
            }
        }

        public void Grid_OnUpdatePathfindingDataEvent(Cell[] cellMap, Vector2Int mapSize, CellLayout layout)
        {
            Layout = layout;
            MapSize = new int2(mapSize.x, mapSize.y);
            pathNodeMap.Dispose();
            pathNodeMap = new NativeArray<PathNode>(cellMap.Length, Allocator.Persistent);
            for (var i = 0; i < cellMap.Length; i++)
            {
                var pn = new PathNode();
                if (cellMap[i] is null)
                {
                    pn.Walkable = false;
                    continue;
                }

                pn.Walkable = cellMap[i].Walkable;
                var index = new int2(cellMap[i].index.x, cellMap[i].index.y);
                pn.GridIndex = index;
                pathNodeMap[i] = pn;
            }
        }

        private void SchedulePathfindingJobs()
        {
            foreach (var t in PathRequests)
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
                        Request = t,
                        MapSize = MapSize,
                        GridMap = gridMap,
                        Path = path,
                        HasPath = hasPath,
                        OpenSet = openSet,
                        ClosedSet = closedSet,
                    }.Schedule(),
                    CellLayout.Hexagon => new HexagonPathfindingJob
                    {
                        Request = t,
                        MapSize = MapSize,
                        GridMap = gridMap,
                        Path = path,
                        HasPath = hasPath,
                        OpenSet = openSet,
                        ClosedSet = closedSet,
                    }.Schedule(),
                    _ => throw new NotSupportedException($"Layout {Layout} is not supported!")
                };
                runningJobs.Add(jobHandle);
                pendingResults.Add((t, path, hasPath));

                openSet.Dispose(jobHandle);
                closedSet.Dispose(jobHandle);
                gridMap.Dispose(jobHandle);
            }

            PathRequests.Clear();
        }

        private void OnDestroy()
        {
            JobHandle.CompleteAll(runningJobs.AsArray());
            runningJobs.Dispose();
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
}