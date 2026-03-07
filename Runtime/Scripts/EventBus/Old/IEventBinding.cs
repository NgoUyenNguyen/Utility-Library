using System;

namespace NgoUyenNguyen
{
    /// <summary>
    /// Defines a contract for binding event handlers to events with customizable execution order.
    /// </summary>
    /// <typeparam name="T">The type of event associated with the binding.</typeparam>
    public interface IEventBinding<T>
    {
        /// <summary>
        /// The execution order for the event binding.
        /// </summary>
        public int ExecuteOrder { get; set; }
        /// <summary>
        /// The event handler to be invoked when the event is raised.
        /// </summary>
        public Action<T> OnEvent { get; set; }
        /// <summary>
        /// The event handler to be invoked when the event is raised with no arguments.
        /// </summary>
        public Action OnEventNoArgs { get; set; }
    }
}