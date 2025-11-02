using System;
using UnityEngine;

namespace NgoUyenNguyen
{
    /// <summary>
    /// An attribute that enforces a field to reference an object implementing a specific interface.
    /// This attribute is used to constrain Unity inspector fields to only allow objects that
    /// implement a specified interface type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RequireInterfaceAttribute : PropertyAttribute
    {
        public readonly Type InterfaceType;

        public RequireInterfaceAttribute(Type interfaceType)
        {
            Debug.Assert(interfaceType.IsInterface, $"{interfaceType.FullName} must be an interface.");
            InterfaceType = interfaceType;
        }
    }
}
