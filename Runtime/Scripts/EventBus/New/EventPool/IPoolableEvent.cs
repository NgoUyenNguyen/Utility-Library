namespace NgoUyenNguyen
{
    /// <summary>
    /// Defines a contract for events that can be pooled and reused within an object pool structure.
    /// </summary>
    public interface IPoolableEvent : IReusable
    {
        /// <summary>
        /// Invoked when an instance of the event is retrieved from the pool.
        /// This method allows initialization or setup of the event's state upon acquisition.
        /// </summary>
        void OnGet()
        {
        }

        /// <summary>
        /// Invoked when an instance of the event is returned to the pool.
        /// This method allows cleanup or resetting of the event's state to prepare it for reuse.
        /// </summary>
        void OnRelease() => Reset();

        /// <summary>
        /// Invoked when an instance of the event is being permanently destroyed and removed from the pool.
        /// This method allows for cleanup of any resources or custom logic for disposal of the event's state.
        /// </summary>
        void OnDestroy()
        {
        }
    }
}