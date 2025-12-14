using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NgoUyenNguyen.Editor
{
    public static class ServiceLocatorCreator
    {
        [MenuItem("GameObject/Service Locator/Global Scope")]
        private static void AddGlobal()
        {
            var go = new GameObject(ServiceLocator.GlobalServiceLocatorName, typeof(GlobalServiceLocatorBootstrapper));
            EditorSceneManager.MarkSceneDirty(go.scene);
        }

        [MenuItem("GameObject/Service Locator/Scene Scope")]
        private static void AddScene()
        {
            var go = new GameObject(ServiceLocator.SceneServiceLocatorName, typeof(SceneServiceLocatorBootstrapper));
            EditorSceneManager.MarkSceneDirty(go.scene);
        }
    }
}
