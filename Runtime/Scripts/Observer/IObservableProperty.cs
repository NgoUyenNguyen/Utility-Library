using System;

namespace NgoUyenNguyen
{
    /// <summary>
    /// Represents an interface that defines a property whose changes can be observed.
    /// Designed to notify listeners when the property's value changes.
    /// </summary>
    /// <typeparam name="T">The type of the property's value.</typeparam>
    public interface IObservableProperty<T>
    {
        /// <summary>
        /// The observed value.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Attempts to change the value of the property to the specified new value
        /// and triggers a notification if the value changes.
        /// </summary>
        /// <param name="newValue">The new value to assign to the property.</param>
        /// <returns>
        /// Returns true if the value was successfully changed and a notification was triggered;
        /// otherwise, returns false if the new value is equal to the current value and no change occurred.
        /// </returns>
        bool ChangeValue(T newValue);

        /// <summary>
        /// Attempts to change the observed value to the specified new value
        /// without triggering any change events or notifications.
        /// </summary>
        /// <param name="newValue">The new value to assign to the observed property.</param>
        /// <returns>
        /// Returns true if the value was successfully changed;
        /// returns false if the new value is equal to the current value and no change occurred.
        /// </returns>
        bool ChangeValueWithoutNotify(T newValue);

        /// <summary>
        /// Registers a callback that will be invoked whenever the value changes.
        /// </summary>
        /// <param name="callback"></param>
        void AddListener(Delegate callback);
        
        /// <summary>
        /// Unregisters a callback.
        /// </summary>
        /// <param name="callback"></param>
        void RemoveListener(Delegate callback);
    }
}