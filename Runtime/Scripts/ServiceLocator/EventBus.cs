using System;
using System.Collections.Generic;
using System.Linq;

namespace NgoUyenNguyen
{
    public partial class EventBus
    {
        private readonly Dictionary<Type, SortedDictionary<int, HashSet<Delegate>>> listeners = new();
        private readonly Dictionary<(Type, Delegate), Delegate> wrapperMap = new();

        /// <summary>
        /// Subscribes a callback to a specific event type with an optional execution order.
        /// </summary>
        /// <typeparam name="T">The type of the event to subscribe to.</typeparam>
        /// <param name="callback">The callback to invoke when the event is published.</param>
        /// <param name="executionOrder">The order in which the callback should be executed when the event is triggered. Defaults to 0.</param>
        public void Subscribe<T>(Action<T> callback, int executionOrder = 0)
        {
            var type = typeof(T);
            
            if (wrapperMap.ContainsKey((type, callback))) return;

            Action<object> wrapper = o => callback((T)o);

            wrapperMap[(type, callback)] = wrapper;
            AddListener(type, wrapper, executionOrder);
        }

        /// <summary>
        /// Subscribes a callback to a specific event type with an optional execution order.
        /// </summary>
        /// <typeparam name="T">The type of the event to subscribe to.</typeparam>
        /// <param name="callback">The callback to invoke when the event is published.</param>
        /// <param name="executionOrder">The order in which the callback should be executed when the event is triggered. Defaults to 0.</param>
        public void Subscribe<T>(Action callback, int executionOrder = 0)
        {
            var type = typeof(T);
            
            if (wrapperMap.ContainsKey((type, callback))) return;

            Action<object> wrapper = _ => callback();

            wrapperMap[(type, callback)] = wrapper;
            AddListener(type, wrapper, executionOrder);
        }

        /// <summary>
        /// Subscribes a callback to a specific event type and returns a disposable object for managing the subscription.
        /// </summary>
        /// <typeparam name="T">The type of the event to subscribe to.</typeparam>
        /// <param name="callback">The callback to invoke when the event is published.</param>
        /// <param name="order">The order in which the callback should be executed when the event is triggered. Defaults to 0.</param>
        /// <returns>A disposable object that can be used to unsubscribe from the event.</returns>
        public IDisposable SubscribeWithDisposable<T>(Action<T> callback, int order = 0)
        {
            Subscribe(callback, order);
            return new Subscription(() => Unsubscribe(callback));
        }

        /// <summary>
        /// Subscribes a callback to a specific event type and returns a disposable object for managing the subscription.
        /// </summary>
        /// <typeparam name="T">The type of the event to subscribe to.</typeparam>
        /// <param name="callback">The callback to invoke when the event is published.</param>
        /// <param name="order">The order in which the callback should be executed when the event is triggered. Defaults to 0.</param>
        /// <returns>A disposable object that can be used to unsubscribe from the event.</returns>
        public IDisposable SubscribeWithDisposable<T>(Action callback, int order = 0)
        {
            Subscribe<T>(callback, order);
            return new Subscription(() => Unsubscribe<T>(callback));
        }

        /// <summary>
        /// Retrieves the total count of listeners registered for a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of the event for which listeners are counted.</typeparam>
        /// <returns>The total number of registered listeners for the given event type.</returns>
        public int ListenerCount<T>()
        {
            var type = typeof(T);
            return listeners.TryGetValue(type, out var sorted)
                ? sorted.Values.Sum(c => c.Count)
                : 0;
        }



        /// <summary>
        /// Unsubscribes a callback from a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of the event to unsubscribe from.</typeparam>
        /// <param name="callback">The callback to remove.</param>
        public void Unsubscribe<T>(Action<T> callback)
        {
            var key = (typeof(T), (Delegate)callback);

            if (!wrapperMap.TryGetValue(key, out var wrapper)) return;
            RemoveListener(typeof(T), wrapper);
            wrapperMap.Remove(key);
        }

        /// <summary>
        /// Unsubscribes a callback from a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of the event to unsubscribe from.</typeparam>
        /// <param name="callback">The callback to remove.</param>
        public void Unsubscribe<T>(Action callback)
        {
            var key = (typeof(T), (Delegate)callback);

            if (!wrapperMap.TryGetValue(key, out var wrapper)) return;
            RemoveListener(typeof(T), wrapper);
            wrapperMap.Remove(key);
        }

        /// <summary>
        /// Clears all registered event listeners for all event types, resetting the event bus to its initial state.
        /// </summary>
        public void Clear()
        {
            listeners.Clear();
            wrapperMap.Clear();
        }

        private void AddListener(Type type, Delegate callback, int executionOrder)
        {
            if (!listeners.TryGetValue(type, out var sortedDictionary))
                listeners[type] = sortedDictionary = new SortedDictionary<int, HashSet<Delegate>>();
            if (!sortedDictionary.TryGetValue(executionOrder, out var callbacks))
                sortedDictionary[executionOrder] = callbacks = new HashSet<Delegate>();
            callbacks.Add(callback);
        }

        private void RemoveListener(Type type, Delegate callback)
        {
            if (!listeners.TryGetValue(type, out var sorted)) return;

            foreach (var (order, callbacks) in sorted.ToArray())
            {
                callbacks.Remove(callback);
                if (callbacks.Count == 0)
                    sorted.Remove(order);
            }

            if (sorted.Count == 0)
                listeners.Remove(type);
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
            foreach (var callback in sorted.Values.SelectMany(callbacks => callbacks.ToArray()))
            {
                ((Action<object>)callback).Invoke(message);
            }
        }
    }
}