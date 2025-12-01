using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NgoUyenNguyen
{
    internal class ServiceManager
    {
        private readonly Dictionary<Type, object> services = new();
        public IEnumerable<object> RegisteredServices => services.Values;

        public bool Has<T>()
        {
            return services.ContainsKey(typeof(T));
        }

        public bool Has(Type serviceType)
        {
            return services.ContainsKey(serviceType);
        }

        public bool TryGet<T>(out T service)
        {
            if (services.TryGetValue(typeof(T), out var s))
            {
                service = (T)s;
                return true;
            }

            service = default;
            return false;
        }

        public T Get<T>()
        {
            if (services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            return default;
        }

        public void Register<T>(T service)
        {
            if (!services.TryAdd(typeof(T), service))
            {
                Debug.LogError(
                    $"ServiceManager.Register: Service of type '{typeof(T).FullName}' already registered");
            }
        }

        public void Register(object service, params Type[] types)
        {
            var distinctTypes = types.Append(service.GetType()).Distinct();
            foreach (var type in distinctTypes)
            {
                if (!type.IsInstanceOfType(service))
                {
                    throw new ArgumentException($"'{service}' does not implement '{type}'");
                }

                if (!services.TryAdd(type, service))
                {
                    Debug.LogError($"ServiceManager.Register: Service of type {type.FullName} already registered");
                }
            }
        }

        public bool Unregister<T>()
        {
            return services.Remove(typeof(T));
        }

        public bool Unregister(params Type[] types)
        {
            var result = true;
            var distinctTypes = types.Distinct();
            foreach (var type in distinctTypes)
            {
                if (!services.Remove(type))
                {
                    result = false;
                }
            }

            return result;
        }

        public void UnregisterAll()
        {
            services.Clear();
        }
    }
}