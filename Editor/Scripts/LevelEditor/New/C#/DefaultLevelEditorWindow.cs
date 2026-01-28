using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NgoUyenNguyen.Editor
{
    /// <summary>
    /// Represents a default editor window for managing levels in the Unity Editor.
    /// Inherits from <see cref="LevelEditorWindow{BaseLevel}"/> and provides
    /// functionality for creating, saving, and loading levels.
    /// </summary>
    /// <remarks>
    /// This class is designed to work with level data of type <see cref="BaseLevel"/> and
    /// includes both file-based and reference-based level loading capabilities.
    /// </remarks>
    /// <seealso cref="LevelEditorWindow{TLevelData}"/>
    public abstract class DefaultLevelEditorWindow : LevelEditorWindow<BaseLevel>
    {
        /// <summary>
        /// Event triggered when a new level is successfully added in the level editor.
        /// </summary>
        /// <seealso cref="DefaultLevelEditorWindow.New"/>
        public event Action AddLevelPerformed;

        /// <summary>
        /// Event triggered when a level is successfully loaded in the level editor.
        /// </summary>
        /// <seealso cref="DefaultLevelEditorWindow.Load"/>
        public event Action LoadLevelPerformed;

        /// <summary>
        /// Event triggered upon successfully saving a level in the level editor.
        /// </summary>
        /// <seealso cref="DefaultLevelEditorWindow.Save"/>
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
            
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
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