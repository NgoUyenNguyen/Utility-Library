using UnityEditor;
using UnityEngine;

namespace NgoUyenNguyen.Editor
{
    public static class ServiceLocatorCreator
    {
        [MenuItem("GameObject/ServiceLocator/Global Scope")]
        private static void AddGlobal()
        {
            var go = new GameObject(ServiceLocator.GlobalServiceLocatorName, typeof(GlobalServiceLocatorBootstrapper));
        }

        [MenuItem("GameObject/ServiceLocator/Scene Scope")]
        private static void AddScene()
        {
            var go = new GameObject(ServiceLocator.SceneServiceLocatorName, typeof(SceneServiceLocatorBootstrapper));
        }
    }
}
