using System;

namespace NgoUyenNguyen
{
    internal interface IEventBinding<T>
    {
        public int ExecuteOrder { get; set; }
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
        /// <summary>
        /// Specifies the order in which the event handler should be executed within the event's execution sequence.
        /// The value of this property determines the priority of the event execution, with smaller values being executed earlier.
        /// </summary>
        public int ExecuteOrder { get; set; }

        public Action<T> OnEvent { get; set; } = _ => { };
        public Action OnEventNoArgs { get; set; } = () => { };

        /// <summary>
        /// Represents a class that manages the binding of event handlers to events of type <typeparamref name="T"/>.
        /// Provides functionality to add, remove, and handle event logic.
        /// </summary>
        /// <typeparam name="T">The type of event that implements the <see cref="IEvent"/> interface.</typeparam>
        public EventBinding(Action<T> onEvent, int executeOrder = 0)
        {
            ExecuteOrder = executeOrder;
            OnEvent = onEvent;
        }

        /// <summary>
        /// Represents a class that manages the binding of event handlers to events of type <typeparamref name="T"/>.
        /// Provides functionality to add, remove, and handle event logic.
        /// </summary>
        /// <typeparam name="T">The type of event that implements the <see cref="IEvent"/> interface.</typeparam>
        public EventBinding(Action onEventNoArgs, int executeOrder = 0)
        {
            ExecuteOrder = executeOrder;
            OnEventNoArgs = onEventNoArgs;
        }
        
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