using System;

namespace NgoUyenNguyen
{
    internal interface IEventBinding<T>
    {
        public Action<T> OnEvent { get; set; }
        public Action OnEventNoArgs { get; set; }
    }

    /// <summary>
    /// Represents a class that manages the binding of event handlers to events of type <typeparamref name="T"/>.
    /// Provides functionality to add, remove, and handle event logic.
    /// </summary>
    /// <typeparam name="T">The type of event that implements the <see cref="IEvent"/> interface.</typeparam>
    public class EventBinding<T> : IEventBinding<T> where T : IEvent
    {
        public Action<T> OnEvent { get; set; } = _ => { };
        
        public Action OnEventNoArgs { get; set; } = () => { };

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBinding{T}"/> class with an event handler that takes event data.
        /// </summary>
        /// <param name="onEvent">The action to be executed when the event occurs, receiving event data of type <typeparamref name="T"/>.</param>
        public EventBinding(Action<T> onEvent) => OnEvent = onEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBinding{T}"/> class with an event handler that takes no arguments.
        /// </summary>
        /// <param name="onEventNoArgs">The action to be executed when the event occurs, receiving no arguments.</param>
        public EventBinding(Action onEventNoArgs) => OnEventNoArgs = onEventNoArgs;
        
        /// <summary>
        /// Adds an event handler that takes event data to the binding.
        /// </summary>
        /// <param name="onEvent">The action to be added to the event handlers, receiving event data of type <typeparamref name="T"/>.</param>
        public void Add(Action<T> onEvent) => OnEvent += onEvent;
        
        /// <summary>
        /// Adds an event handler that takes no arguments to the binding.
        /// </summary>
        /// <param name="onEvent">The action to be added to the event handlers, receiving no arguments.</param>
        public void Add(Action onEvent) => OnEventNoArgs += onEvent;

        /// <summary>
        /// Removes an event handler that takes event data from the binding.
        /// </summary>
        /// <param name="onEvent">The action to be removed from the event handlers, receiving event data of type <typeparamref name="T"/>.</param>
        public void Remove(Action<T> onEvent) => OnEvent -= onEvent;

        /// <summary>
        /// Removes an event handler that takes no arguments from the binding.
        /// </summary>
        /// <param name="onEvent">The action to be removed from the event handlers, receiving no arguments.</param>
        public void Remove(Action onEvent) => OnEventNoArgs -= onEvent;
    }
}