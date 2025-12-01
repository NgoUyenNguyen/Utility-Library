using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace NgoUyenNguyen.Grid
{
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
            var startIndex = AxialToIndex(Request.From);
            var startNode = GridMap[startIndex.x + startIndex.y * MapSize.x];
            var goalIndex = AxialToIndex(Request.To);
            var goalNode = GridMap[goalIndex.x + goalIndex.y * MapSize.x];
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

                        var neighborIndex = AxialToIndex(currentNode.GridIndex + new int2(x, y));
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
            var axialA = a.GridIndex;
            var axialB = b.GridIndex;
            return (math.abs(axialA.x - axialB.x) + 
                    math.abs(axialA.x + axialA.y - axialB.x - axialB.y) +  
                    math.abs(axialA.y - axialB.y)) / 2;
        }

        private void RetracePath(PathNode startNode, PathNode goalNode)
        {
            var currentNode = goalNode;
            while (!currentNode.Equals(startNode))
            {
                Path.Add(currentNode);
                var parentIndex = AxialToIndex(currentNode.ParentIndex);
                currentNode = GridMap[parentIndex.x + parentIndex.y * MapSize.x];
            }

            Path.Add(startNode);
            Path.Reverse();
        }

        private int2 AxialToIndex(int2 axial)
        {
            var parity = axial.y & 1;
            var col = axial.x + (axial.y - parity) / 2;
            return new int2(col, axial.y);
        }
    }
}