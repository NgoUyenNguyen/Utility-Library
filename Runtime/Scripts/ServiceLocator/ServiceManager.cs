using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NgoUyenNguyen
{
    internal class ServiceManager
    {
        private readonly Dictionary<Type, object> services = new();
        public IEnumerable<object> RegisteredServices => services.Values.Distinct();

        public bool Has<T>() => services.ContainsKey(typeof(T));

        public bool Has(Type serviceType) => services.ContainsKey(serviceType);

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

        public bool Register<T>(T service) => service != null && services.TryAdd(typeof(T), service);

        public bool Register(object service, params Type[] types)
        {
            if (service == null) return false;

            var allTypes = types
                .Append(service.GetType())
                .Distinct()
                .ToArray();

            if (allTypes.Any(t =>
                    !t.IsInstanceOfType(service) ||
                    services.ContainsKey(t)))
            {
                return false;
            }

            foreach (var type in allTypes)
            {
                services.Add(type, service);
            }

            return true;
        }

        public bool Unregister<T>() => services.Remove(typeof(T));

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

        public void UnregisterAll() => services.Clear();
    }
}