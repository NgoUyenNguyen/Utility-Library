using System;
using System.Collections.Generic;
using System.Linq;

namespace NgoUyenNguyen
{
    public partial class EventBus
    {
        private readonly Dictionary<Type, SortedDictionary<int, HashSet<Delegate>>> listeners = new();

        /// <summary>
        /// Subscribes a callback to a specific event type with an optional execution order.
        /// </summary>
        /// <typeparam name="T">The type of the event to subscribe to.</typeparam>
        /// <param name="callback">The callback to invoke when the event is published.</param>
        /// <param name="executionOrder">The order in which the callback should be executed when the event is triggered. Defaults to 0.</param>
        public void Subscribe<T>(Action<T> callback, int executionOrder = 0)
        {
            AddListener(typeof(T), (Action<object>)Wrapper, executionOrder);
            return;
            void Wrapper(object o) => callback((T)o);
        }

        /// <summary>
        /// Subscribes a callback to a specific event type with an optional execution order.
        /// </summary>
        /// <typeparam name="T">The type of the event to subscribe to.</typeparam>
        /// <param name="callback">The callback to invoke when the event is published.</param>
        /// <param name="executionOrder">The order in which the callback should be executed when the event is triggered. Defaults to 0.</param>
        public void Subscribe<T>(Action callback, int executionOrder = 0)
        {
            AddListener(typeof(T), (Action<object>)Wrapper, executionOrder);
            return;
            void Wrapper(object o) => callback();
        }

        /// <summary>
        /// Unsubscribes a callback from a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of the event to unsubscribe from.</typeparam>
        /// <param name="callback">The callback to remove.</param>
        public void Unsubscribe<T>(Action<T> callback) => RemoveListeners(typeof(T), callback);

        /// <summary>
        /// Unsubscribes a callback from a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of the event to unsubscribe from.</typeparam>
        /// <param name="callback">The callback to remove.</param>
        public void Unsubscribe<T>(Action callback) => RemoveListeners(typeof(T), callback);

        /// <summary>
        /// Clears all registered event listeners for all event types, resetting the event bus to its initial state.
        /// </summary>
        public void Clear() => listeners.Clear();

        private void AddListener(Type type, Delegate callback, int executionOrder)
        {
            if (!listeners.TryGetValue(type, out var sortedDictionary))
                listeners[type] = sortedDictionary = new SortedDictionary<int, HashSet<Delegate>>();
            if (!sortedDictionary.TryGetValue(executionOrder, out var callbacks))
                sortedDictionary[executionOrder] = callbacks = new HashSet<Delegate>();
            callbacks.Add(callback);
        }

        private void RemoveListeners(Type type, Delegate callback)
        {
            if (!listeners.TryGetValue(type, out var sortedDictionary)) return;
            foreach (var callbacks in sortedDictionary.Values
                         .Where(callbacks => callbacks.Contains(callback)))
            {
                callbacks.Remove(callback);
            }
        }

        /// <summary>
        /// Publishes a message of the specified type to all subscribers.
        /// </summary>
        /// <typeparam name="T">The type of the message being published.</typeparam>
        /// <param name="message">The message to be published to subscribers.</param>
        /// <param name="propagation">Specifies the propagation mode for the message.</param>
        public void Publish<T>(T message, PropagationMode propagation = PropagationMode.Downstream)
        {
            var messageType = typeof(T);

            for (var type = messageType; type != null; type = type.BaseType)
                InvokeForType(type, message);

            foreach (var @interface in messageType.GetInterfaces())
                InvokeForType(@interface, message);
            
            switch (propagation)
            {
                case PropagationMode.Upstream:
                    Parent?.Publish(message, propagation);
                    break;
                case PropagationMode.Downstream:
                {
                    foreach (var child in Children)
                    {
                        child.Publish(message, propagation);
                    }
                    break;
                }
            }
        }

        private void InvokeForType<T>(Type type, T message)
        {
            if (!listeners.TryGetValue(type, out var sorted)) return;
            foreach (var callback in sorted.Values.SelectMany(callbacks => callbacks))
            {
                ((Action<object>)callback).Invoke(message);
            }
        }
    }
}