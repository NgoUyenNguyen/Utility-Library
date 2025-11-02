using UnityEngine;

namespace NgoUyenNguyen
{
    public static class SceneLoaderBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var sceneLoaderPrefab = Resources.Load<GameObject>("SceneGroupLoader");
            if (sceneLoaderPrefab)
            {
                var sceneLoader = Object.Instantiate(sceneLoaderPrefab);
                sceneLoader.isStatic = true;
                Object.DontDestroyOnLoad(sceneLoader);
                var sceneLoaderComponent = sceneLoader.GetComponent<SceneGroupLoader>();
                SceneGroupLoader.Instance = sceneLoaderComponent;
            }
        }
    }
}