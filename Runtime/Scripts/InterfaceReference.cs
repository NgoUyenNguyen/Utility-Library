using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace NgoUyenNguyen
{
    /// <summary>
    /// Represents a Unity serializable reference that enforces a contract requiring the linked object
    /// to implement a specific interface. It ensures type safety and enables the assignment of Unity objects
    /// that adhere to the specified interface.
    /// </summary>
    /// <typeparam name="TInterface">The interface type the referenced object must implement.</typeparam>
    /// <typeparam name="TObject">The specific Unity Object type being referenced.</typeparam>
    [Serializable]
    public class InterfaceReference<TInterface, TObject>
        where TInterface : class
        where TObject : Object
    {
        [SerializeField, HideInInspector] private TObject underlyingValue;

        public TInterface Value
        {
            get => underlyingValue switch
            {
                null => null,
                TInterface @interface => @interface,
                _ => throw new InvalidOperationException($"{underlyingValue} needs to implement {nameof(TInterface)}.")
            };
            set => underlyingValue = value switch
            {
                null => null,
                TObject newValue => newValue,
                _ => throw new InvalidOperationException($"{value} needs to be of type {typeof(TObject)}.")
            };
        }

        public TObject UnderlyingValue
        {
            get => underlyingValue;
            set => underlyingValue = value;
        }

        public InterfaceReference()
        {
        }

        public InterfaceReference(TObject target) => underlyingValue = target;

        public InterfaceReference(TInterface target) => underlyingValue = target as TObject;
    }

    /// <summary>
    /// Represents a Unity serializable reference that enforces a contract requiring the linked object
    /// to implement a specific interface. It ensures type safety and enables the assignment of Unity objects
    /// that adhere to the specified interface.
    /// </summary>
    /// <typeparam name="TInterface">The interface type that the referenced object must implement.</typeparam>
    [Serializable]
    public class InterfaceReference<TInterface> : InterfaceReference<TInterface, Object> where TInterface : class
    {

    }
}
