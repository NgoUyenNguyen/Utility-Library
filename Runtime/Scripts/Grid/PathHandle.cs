using System;

namespace NgoUyenNguyen.Grid
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
}