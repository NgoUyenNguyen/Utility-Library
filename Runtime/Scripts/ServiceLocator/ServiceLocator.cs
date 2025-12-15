using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NgoUyenNguyen
{
    [DefaultExecutionOrder(-900)]
    public partial class ServiceLocator : MonoBehaviour
    {
        public const string GlobalServiceLocatorName = "ServiceLocator [Global]";
        public const string SceneServiceLocatorName = "ServiceLocator [Scene]";
        public const string GameObjectServiceLocatorName = "ServiceLocator [GameObject]";

        internal static Dictionary<Scene, ServiceLocator> SceneContainers;

        private readonly ServiceManager services = new();

        internal void ConfigureForScene()
        {
            var scene = gameObject.scene;

            if (SceneContainers.ContainsKey(scene))
            {
                Debug.LogWarning(
                    "ServiceLocator.ConfigureAsScene: Another ServiceLocator is already configured for this scene",
                    this);
                return;
            }

            SceneContainers.Add(scene, this);
        }

        /// <summary>
        /// Provides a global <see cref="ServiceLocator"/> instance, acting as a shared container for services that
        /// are not specific to any particular scene or object. The global instance is automatically created
        /// on first access and can be configured to persist across scene changes via DontDestroyOnLoad.
        /// </summary>
        public static ServiceLocator Global { get; internal set; }

        /// <summary>
        /// Retrieves the closest <see cref="ServiceLocator"/> instance associated with the specified <see cref="MonoBehaviour"/>.
        /// </summary>
        /// <param name="mb">The MonoBehaviour instance for which the ServiceLocator is being retrieved.</param>
        /// <returns>
        /// Returns the <see cref="ServiceLocator"/> found for the specified MonoBehaviour.
        /// This method checks the parent hierarchy for a ServiceLocator, falls back to the scene-specific ServiceLocator,
        /// and ultimately defaults to the global instance if no other locator is found.
        /// </returns>
        public static ServiceLocator For(MonoBehaviour mb)
        {
            return mb.GetComponentInParent<ServiceLocator>() ?? ForSceneOf(mb) ?? Global;
        }

        /// <summary>
        /// Retrieves the <see cref="ServiceLocator"/> instance associated with the scene of the specified <see cref="MonoBehaviour"/>.
        /// If no dedicated scene-specific ServiceLocator is found, returns the global instance.
        /// </summary>
        /// <param name="mb">The MonoBehaviour for which the scene's ServiceLocator is being retrieved.</param>
        /// <returns>
        /// The <see cref="ServiceLocator"/> instance associated with the scene containing the specified MonoBehaviour.
        /// If no appropriate ServiceLocator exists in the scene, the method falls back to the global instance.
        /// </returns>
        public static ServiceLocator ForSceneOf(MonoBehaviour mb)
        {
            var scene = mb.gameObject.scene;

            if (SceneContainers.TryGetValue(scene, out var container) 
                && container != mb)
            {
                return container;
            }

            return Global;
        }

        /// <summary>
        /// Registers a service instance of type <typeparamref name="T"/> with the current <see cref="ServiceLocator"/>.
        /// </summary>
        /// <param name="service">The service instance to register.</param>
        /// <typeparam name="T">The type of the service being registered.</typeparam>
        /// <returns>
        /// Returns the current <see cref="ServiceLocator"/> instance, enabling method chaining.
        /// </returns>
        public ServiceLocator Register<T>(T service)
        {
            services.Register(service);
            return this;
        }

        /// <summary>
        /// Registers a service instance with the service locator for the specified types or its own type if no additional types are specified.
        /// </summary>
        /// <param name="service">The service instance to register.</param>
        /// <param name="types">The types to associate with the service. If not specified, the service's own type will be used.</param>
        /// <returns>
        /// The current instance of <see cref="ServiceLocator"/> to allow method chaining.
        /// </returns>
        public ServiceLocator Register(object service, params Type[] types)
        {
            services.Register(service, types);
            return this;
        }

        /// <summary>
        /// Retrieves an instance of the specified service type <typeparamref name="T"/> that has been registered
        /// with this <see cref="ServiceLocator"/> or is available in the hierarchy.
        /// </summary>
        /// <typeparam name="T">The type of the service to retrieve.</typeparam>
        /// <returns>
        /// Returns an instance of the requested service type <typeparamref name="T"/>.
        /// If the service is not directly available in the current locator, this method checks
        /// higher levels in the hierarchy of service locators as a fallback.
        /// </returns>
        public T Get<T>()
        {
            if (services.Has<T>()) return services.Get<T>();
            if (TryGetNextInHierarchy(out var container))
            {
                return container.Get<T>();
            }

            Debug.LogWarning($"ServiceLocator.Get: Service of type {typeof(T).FullName} not registered");
            return default;
        }

        /// <summary>
        /// Attempts to retrieve a registered service of type <typeparamref name="T"/> from current or the next available
        /// <see cref="ServiceLocator"/> in the hierarchy.
        /// </summary>
        /// <param name="service">When the method returns, contains the service instance of type <typeparamref name="T"/> if found, or the default value of type <typeparamref name="T"/> otherwise.</param>
        /// <typeparam name="T">The type of the service to retrieve.</typeparam>
        /// <returns>
        /// Returns <c>true</c> if a service of type <typeparamref name="T"/> is successfully found; otherwise, <c>false</c>.
        /// The search progresses from the current <see cref="ServiceLocator"/> to the next in the hierarchy.
        /// </returns>
        public bool TryGet<T>(out T service)
        {
            if (services.TryGet(out service)) return true;
            return TryGetNextInHierarchy(out var container) && container.TryGet(out service);
        }

        /// <summary>
        /// Determines whether a service of the specified type is registered within the current <see cref="ServiceLocator"/> or its parent hierarchy.
        /// </summary>
        /// <typeparam name="T">The type of the service to check for presence.</typeparam>
        /// <returns>
        /// Returns <c>true</c> if a service of the specified type is registered in the current <see cref="ServiceLocator"/>
        /// or any parent locators in the hierarchy; otherwise, returns <c>false</c>.
        /// </returns>
        public bool Has<T>()
        {
            if (services.Has<T>()) return true;
            return TryGetNextInHierarchy(out var container) && container.Has<T>();
        }

        /// <summary>
        /// Determines whether the specified service type is registered within the <see cref="ServiceLocator"/> hierarchy.
        /// </summary>
        /// <param name="serviceType">The <see cref="Type"/> of the service to check for.</param>
        /// <returns>
        /// Returns <c>true</c> if the specified service type is registered either in the current <see cref="ServiceLocator"/>
        /// or any parent in the hierarchy; otherwise, <c>false</c>.
        /// </returns>
        public bool Has(Type serviceType)
        {
            if (services.Has(serviceType)) return true;
            return TryGetNextInHierarchy(out var container) && container.Has(serviceType);
        }

        /// <summary>
        /// Unregisters a service of the specified type <typeparamref name="T"/> from the ServiceLocator.
        /// </summary>
        /// <typeparam name="T">The type of the service to unregister.</typeparam>
        /// <returns>
        /// Returns true if the service of type <typeparamref name="T"/> was successfully unregistered;
        /// otherwise, false if the service was not registered or could not be removed.
        /// </returns>
        public bool Unregister<T>()
        {
            return services.Unregister<T>();
        }

        /// <summary>
        /// Unregisters one or more service types from the ServiceLocator.
        /// </summary>
        /// <param name="types">
        /// An array of <see cref="Type"/> objects representing the service types to be unregistered.
        /// </param>
        /// <returns>
        /// Returns a boolean indicating whether all the provided service types were successfully unregistered.
        /// </returns>
        public bool Unregister(params Type[] types)
        {
            return services.Unregister(types);
        }

        public void UnregisterAll()
        {
            services.UnregisterAll();
        }

        private bool TryGetNextInHierarchy(out ServiceLocator container)
        {
            if (this == Global)
            {
                container = null;
                return false;
            }

            container = transform.parent.OrNull()?.GetComponentInParent<ServiceLocator>().OrNull() ?? ForSceneOf(this);
            return container != null;
        }

        private void OnDestroy()
        {
            DetachEventBus();
            if (this == Global)
            {
                Global = null;
            }
            else if (SceneContainers != null && SceneContainers.ContainsValue(this))
            {
                SceneContainers.Remove(gameObject.scene);
            }
        }
    }
}