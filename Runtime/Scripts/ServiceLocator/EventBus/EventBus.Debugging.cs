using System;
using System.Collections.Generic;
using System.Linq;

namespace NgoUyenNguyen
{
    public partial class EventBus
    {
        /// <summary>
        /// Determines whether a specific listener is registered for a given event type.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        /// <param name="listener">The delegate representing the listener.</param>
        public bool Contains<T>(Delegate listener) =>
            listener != null
            && wrapperMap.ContainsKey((typeof(T), listener));

        /// <summary>
        /// Determines whether a listener with a specific method name is registered for a given event type.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        /// <param name="methodName">The name of the method.</param>
        public bool Contains<T>(string methodName) =>
            !string.IsNullOrEmpty(methodName)
            && Listeners<T>().Any(callback => callback.Method.Name == methodName);

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
        /// Retrieves callbacks registered for a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        public IEnumerable<Delegate> Listeners<T>()
        {
            var type = typeof(T);
            if (!listeners.TryGetValue(type, out var sorted)) yield break;

            var reverseMap = wrapperMap
                .Where(pair => pair.Key.eventType == type)
                .ToDictionary(pair => pair.Value, pair => pair.Key.origin);

            foreach (var callbacks in sorted.Values)
            {
                foreach (var callback in callbacks)
                {
                    if (reverseMap.TryGetValue(callback, out var original))
                        yield return original;
                }
            }
        }

        /// <summary>
        /// Retrieves the names of callback methods registered for a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of the event for which callback method names are retrieved.</typeparam>
        public IEnumerable<string> ListenerNames<T>() =>
            Listeners<T>().Select(callback => callback.Method.Name);

        /// <summary>
        /// Retrieves the fully qualified names of all registered listener methods for a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of the event for which listener names are retrieved.</typeparam>
        /// <param name="declareTypeFullName">
        /// A boolean indicating whether the fully qualified name of the declaring type
        /// should be included in the result.
        /// If true, the full type name is included; otherwise, only the type name is used.
        /// </param>
        public IEnumerable<string> ListenerFullNames<T>(bool declareTypeFullName = false)
        {
            foreach (var callback in Listeners<T>())
            {
                var declareName = declareTypeFullName 
                    ? callback.Method.DeclaringType?.FullName
                    : callback.Method.DeclaringType?.Name 
                      ?? "null";
                yield return $"{declareName}.{callback.Method.Name}";
            }
        }
    }
}
