using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NgoUyenNguyen
{
    internal static class ServiceLocatorRuntimeInitialize
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            ServiceLocator.Global = null;
            ServiceLocator.SceneContainers = new Dictionary<Scene, ServiceLocator>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BootstrapGlobal()
        {
            var go = new GameObject(ServiceLocator.GlobalServiceLocatorName)
            {
                transform =
                {
                    position = Vector3.zero,
                    rotation = Quaternion.identity,
                    localScale = Vector3.one
                },
                isStatic = true
            };
            ServiceLocator.Global = go.AddComponent<ServiceLocator>();
            Object.DontDestroyOnLoad(go);
        }
    }
}