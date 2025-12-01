using System.Collections.Generic;

namespace NgoUyenNguyen.Grid
{
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
}