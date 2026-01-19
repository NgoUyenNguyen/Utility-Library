using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NgoUyenNguyen.Editor
{
    public abstract class DefaultLevelEditorWindow : LevelEditorWindow<BaseLevel>
    {
        public event Action AddLevelPerformed;
        public event Action LoadLevelPerformed;
        public event Action SaveLevelPerformed;
        
        protected override void Save(LevelSaveDescription<BaseLevel> levelSaveDescription)
        {
            // Make sure LevelFolder exist
            System.IO.Directory.CreateDirectory(levelSaveDescription.FolderPath);
            var levelPath = GetFullPath(levelSaveDescription, ".prefab");

            LevelData.name = $"{levelSaveDescription.Name}";
            EditorUtility.SetDirty(LevelData);

            SaveAsPrefab(LevelData.gameObject, levelPath);
            SaveToLevelReference(levelPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SaveLevelPerformed?.Invoke();
        }

        protected override LevelLoadDescription<BaseLevel> Load(AssetReference levelReference)
        {
            // If exist, destroy current level on scene
            if (LevelData != null)
            {
                DestroyImmediate(LevelData.gameObject);
                LevelData = null;
            }
            
            var description = levelReference != null 
                ? LoadFromReference(levelReference) 
                : LoadFromFilePanel();

            if (description == null)
            {
                Debug.LogError("Failed to load level " +
                               $"because it does not contain {nameof(BaseLevel)} component");
            }

            LoadLevelPerformed?.Invoke();
            return description;
        }

        protected virtual LevelLoadDescription<BaseLevel> LoadFromFilePanel()
        {
            var absolutePath = EditorUtility.OpenFilePanel(
                "Load Level",
                Settings.defaultLevelFolder,
                "prefab"
            );
            
            TryGetAssetPath(absolutePath, out var assetPath);
            
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath)?.GetComponent<BaseLevel>();
            if (prefab == null) return null;
            
            var data = (PrefabUtility.InstantiatePrefab(prefab) as GameObject)?
                .GetComponent<BaseLevel>();
            return new LevelLoadDescription<BaseLevel>(data, assetPath);
        }

        protected virtual LevelLoadDescription<BaseLevel> LoadFromReference(AssetReference levelReference)
        {
            if (levelReference == null) return null;
            
            var prefab = levelReference.editorAsset as GameObject;
            if (prefab == null) return null;
            
            var data = (PrefabUtility.InstantiatePrefab(prefab) as GameObject)?
                .GetComponent<BaseLevel>();
            return new LevelLoadDescription<BaseLevel>(data, 
                AssetDatabase.GUIDToAssetPath(levelReference.AssetGUID));
        }

        protected override BaseLevel New()
        {
            if (Settings is not DefaultLevelEditorSettings defaultSettings) return null;
            
            // If exist, destroy current level on scene
            if (LevelData != null)
            {
                DestroyImmediate(LevelData.gameObject);
                LevelData = null;
            }

            BaseLevel data = null;
            // spawn new one
            if (defaultSettings.levelTemplate != null && defaultSettings.levelTemplate.TryGetComponent<BaseLevel>(out _))
            {
                data = (PrefabUtility
                    .InstantiatePrefab(defaultSettings.levelTemplate) as GameObject)?
                    .GetComponent<BaseLevel>();
                data!.name = "New Level";
            }
            else
            {
                Debug.LogError($"{nameof(defaultSettings.levelTemplate)} is null or not contains {nameof(BaseLevel)}");
            }

            AddLevelPerformed?.Invoke();
            return data;
        }
    }
}