using UnityEngine.Pool;

namespace NgoUyenNguyen
{
    /// <summary>
    /// Provides a pool for reusing instances of events that implement the <see cref="IPoolableEvent"/> interface.
    /// </summary>
    /// <typeparam name="TPooledEvent">
    /// The type of event instances managed by the pool.
    /// It must be a class that implements <see cref="IPoolableEvent"/> and provides a parameterless constructor.
    /// </typeparam>
    public static class EventPool<TPooledEvent> where TPooledEvent : class, IPoolableEvent, new()
    {
        private static readonly ObjectPool<TPooledEvent> Pool = new(
            createFunc: () => new TPooledEvent(),
            actionOnGet: e => e.OnGet(),
            actionOnRelease: e => e.Reset(),
            actionOnDestroy: e => e.OnDestroy(),
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 100
            );

        /// <summary>
        /// Retrieves an instance of the pooled event of type <typeparamref name="TPooledEvent"/> from the pool.
        /// The retrieved instance is initialized using the provided action defined in the pool configuration.
        /// </summary>
        /// <returns>
        /// An instance of <typeparamref name="TPooledEvent"/> obtained from the pool.
        /// </returns>
        public static TPooledEvent Get() => Pool.Get();

        /// <summary>
        /// Releases the specified pooled event instance back to the pool.
        /// The event instance will be reset and made available for reuse.
        /// </summary>
        /// <param name="event">
        /// The instance of <typeparamref name="TPooledEvent"/> to be released back to the pool.
        /// </param>
        public static void Release(TPooledEvent @event) => Pool.Release(@event);

        /// <summary>
        /// Clears all instances currently stored in the pool managed by the EventPool.
        /// This action removes all available objects, effectively resetting the pool's state.
        /// </summary>
        public static void Clear() => Pool.Clear();
    }
}
