using System.IO;
using NgoUyenNguyen;
using UnityEditor;
using UnityEngine;

namespace NgoUyenNguyen.Editor
{
    public static class SceneLoaderCreater
    {
        [MenuItem("Assets/Create/Scene Group Loader", false, 1)]
        public static void CreatePrefab()
        {
            const string folderPath = "Assets/Resources";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var prefabPath = $"{folderPath}/SceneGroupLoader.prefab";
            if (File.Exists(prefabPath))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
                return;
            }

            var temp = new GameObject("SceneGroupLoader");

            temp.AddComponent<SceneGroupLoader>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(temp, prefabPath, out bool success);

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