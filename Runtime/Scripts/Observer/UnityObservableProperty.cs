using System;
using UnityEngine;
using UnityEngine.Events;

namespace NgoUyenNguyen
{
    /// <summary>
    /// Represents a property that can be observed for changes.
    /// </summary>
    /// <typeparam name="T">Type of observed value</typeparam>
    [Serializable]
    public class UnityObservableProperty<T> : IObservableProperty<T>
    {
        [SerializeField] private T value;
        [SerializeField] private UnityEvent<T, T> onValueChanged;
        
        public static implicit operator T(UnityObservableProperty<T> unityObservableProperty) => 
            unityObservableProperty.Value;
         
        /// <summary>
        /// The observed value.
        /// </summary>
        public T Value
        {
            get => value;
            set
            {
                if (Equals(this.value, value)) return;
                
                Invoke(this.value, value);
                this.value = value;
            }
        }
        
        public UnityObservableProperty(T value, params UnityAction<T, T>[] callbacks)
        {
            this.value = value;
            onValueChanged = new UnityEvent<T, T>();
            foreach (var callback in callbacks)
            {
                AddListener(callback);
            }
        }
        
        private void Invoke(T oldValue, T newValue)
        {
            onValueChanged.Invoke(oldValue, newValue);
        }
        
        /// <summary>
        /// Attempts to change the observed value to the specified new value
        /// and triggers the appropriate events if the value changes.
        /// </summary>
        /// <param name="newValue">The new value to assign to the observed property.</param>
        /// <returns>
        /// Returns true if the value was successfully changed and the change event was triggered;
        /// returns false if the new value is equal to the current value and no change occurred.
        /// </returns>
        public bool ChangeValue(T newValue)
        {
            if (Equals(value, newValue)) return false;

            Invoke(value, newValue);
            value = newValue;
            return true;
        }

        /// <summary>
        /// Attempts to change the observed value to the specified new value
        /// without triggering any change events or notifications.
        /// </summary>
        /// <param name="newValue">The new value to assign to the observed property.</param>
        /// <returns>
        /// Returns true if the value was successfully changed;
        /// returns false if the new value is equal to the current value and no change occurred.
        /// </returns>
        public bool ChangeValueWithoutNotify(T newValue)
        {
            if (Equals(value, newValue)) return false;
            
            value = newValue;
            return true;
        }

        /// <summary>
        /// Registers a callback that will be invoked whenever the value changes.
        /// </summary>
        public void AddListener(Delegate callback)
        {
            if (callback is not UnityAction<T, T> castedCallback) return;
            AddListener(castedCallback);
        }

        /// <summary>
        /// Registers a callback that will be invoked whenever the value changes.
        /// </summary>
        public void AddListener(UnityAction<T, T> callback)
        {
            if (callback == null) return;
            onValueChanged.AddListener(callback);
        }

        /// <summary>
        /// Unregisters a callback.
        /// </summary>
        public void RemoveListener(Delegate callback)
        {
            if (callback is not UnityAction<T, T> castedCallback) return;
            RemoveListener(castedCallback);
        }

        /// <summary>
        /// Unregisters a callback.
        /// </summary>
        public void RemoveListener(UnityAction<T, T> callback)
        {
            if (callback == null) return;
            onValueChanged.RemoveListener(callback);
        }

        /// <summary>
        /// Removes all registered listeners from the event. After calling this method,
        /// no callbacks will be invoked when the observed value changes.
        /// </summary>
        public void RemoveAllListeners()
        {
            onValueChanged.RemoveAllListeners();
        }
    }
}
