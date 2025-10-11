using System.IO;
using NgoUyenNguyen;
using UnityEditor;
using UnityEngine;

namespace NgoUyenNguyen.Editor
{
    public static class SceneLoaderCreater
    {
        [MenuItem("Assets/Create/Scene Loader", false, 1)]
        public static void CreatePrefab()
        {
            string folderPath = "Assets/Resources";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string prefabPath = $"{folderPath}/SceneLoader.prefab";
            if (File.Exists(prefabPath))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
                return;
            }

            GameObject temp = new GameObject("SceneLoader");

            temp.AddComponent<SceneLoader>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, prefabPath, out bool success);

            Object.DestroyImmediate(temp);

            if (success)
            {
                Debug.Log($"✅ Prefab created at {prefabPath}");
                EditorGUIUtility.PingObject(prefab);
            }
            else
            {
                Debug.LogError("❌ Failed to create prefab!");
            }
        }
    }
}