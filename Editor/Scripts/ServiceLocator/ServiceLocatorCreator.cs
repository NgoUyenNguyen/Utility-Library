using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NgoUyenNguyen.Editor
{
    public static class ServiceLocatorCreator
    {
        [MenuItem("GameObject/Service Locator/GameObject Scope")]
        private static void AddGameObjectScope()
        {
            var go = new GameObject(ServiceLocator.GameObjectServiceLocatorName, typeof(ServiceLocator));
            EditorSceneManager.MarkSceneDirty(go.scene);
        }

        [MenuItem("GameObject/Service Locator/Scene Scope")]
        private static void AddSceneScope()
        {
            var go = new GameObject(ServiceLocator.SceneServiceLocatorName, typeof(SceneServiceLocatorBootstrapper))
            {
                transform =
                {
                    position = Vector3.zero,
                    rotation = Quaternion.identity,
                    localScale = Vector3.one
                },
                isStatic = true
            };
            EditorSceneManager.MarkSceneDirty(go.scene);
        }
    }
}