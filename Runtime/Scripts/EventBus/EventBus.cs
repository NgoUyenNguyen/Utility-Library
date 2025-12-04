using System.Collections.Generic;

namespace NgoUyenNguyen
{
    /// <summary>
    /// Represents a marker interface for events.
    /// </summary>
    /// <remarks>
    /// This interface is designed to be implemented by any event type that will be
    /// used within the event-driven architecture in the application.
    /// </remarks>
    public interface IEvent
    {
    }

    /// <summary>
    /// Provides a static implementation of an event bus for subscribing, unsubscribing,
    /// and publishing events of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of event that implements <see cref="IEvent"/>.</typeparam>
    /// <remarks>
    /// The EventBus is a generic static class that facilitates event-driven programming
    /// by serving as a central hub for event management. It maintains a list of event bindings,
    /// allowing subscribers to register and deregister their handlers for specific events.
    /// When an event is published, all subscribed handlers are invoked in the order
    /// they are registered.
    /// </remarks>
    public static class EventBus<T> where T : IEvent
    {
        private static readonly List<IEventBinding<T>> Bindings = new();

        /// <summary>
        /// Subscribes the specified event binding to the event bus,
        /// allowing it to handle events of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of event that implements <see cref="IEvent"/>.</typeparam>
        /// <param name="binding">The event binding to subscribe,
        /// containing the actions to be executed when the event is triggered.</param>
        public static void Subscribe(EventBinding<T> binding) => Bindings.Add(binding);

        /// <summary>
        /// Unsubscribes the specified event binding from the event bus,
        /// preventing it from handling events of the specified type.
        /// </summary>
        /// <param name="binding">The event binding to unsubscribe,
        /// containing the actions previously registered for the event type.</param>
        public static void Unsubscribe(EventBinding<T> binding) => Bindings.Remove(binding);

        /// <summary>
        /// Publishes an event of the specified type to all subscribed event handlers.
        /// </summary>
        /// <param name="event">The event instance to be published, containing data associated with the event.</param>
        public static void Publish(T @event)
        {
            Bindings.Sort((a, b) => a.ExecuteOrder.CompareTo(b.ExecuteOrder));
            foreach (var binding in Bindings)
            {
                binding.OnEvent(@event);
                binding.OnEventNoArgs();
            }
        }
    }
}