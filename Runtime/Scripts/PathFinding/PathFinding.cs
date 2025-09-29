using System;
using System.Collections.Generic;

namespace NgoUyenNguyen.PathFinding
{
    public class PathFinding
    {
        private readonly Predicate<IMonoPath> isGoal;
        private readonly Func<IMonoPath, IEnumerable<IMonoPath>> getNext;
        private readonly Func<IMonoPath, int> getDistanceToGoal;

        /// <summary>
        /// Initializes a new instance of the PathFinding class with the specified path-finding parameters.
        /// This constructor sets up the core functionality needed for pathfinding operations.
        /// </summary>
        /// <param name="isGoal">A predicate that determines if a given node is the goal node. Returns true if the node is the goal, false otherwise.</param>
        /// <param name="getNext">A function that returns all possible next nodes from a given node. Used to generate the neighboring nodes during path traversal.</param>
        /// <param name="getDistanceToGoal">A function that calculates and returns the estimated distance (heuristic) from a given node to the goal node.</param>
        public PathFinding(
            Predicate<IMonoPath> isGoal, 
            Func<IMonoPath, IEnumerable<IMonoPath>> getNext, 
            Func<IMonoPath, int> getDistanceToGoal)
        {
            this.isGoal = isGoal;
            this.getNext = getNext;
            this.getDistanceToGoal = getDistanceToGoal;
        }

        public List<IMonoPath> FindPath(IMonoPath start, IMonoPath goal)
        {
            throw new System.NotImplementedException();
        }
    }
}
