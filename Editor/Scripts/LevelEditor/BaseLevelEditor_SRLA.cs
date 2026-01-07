using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace NgoUyenNguyen.Editor
{
    public partial class BaseLevelEditor
    {
        private bool isLoadingLevel;
        private static Vector2 levelScrollPos;
        public event Action RemoveLevelPerformed;
        public event Action LoadLevelPerformed;
        public event Action SaveLevelPerformed;
        public event Action AddLevelPerformed;

        public string LevelFolderPath { get; set; } = "Assets/Levels";
        public string LevelAddressableGroup { get; set; } = "Levels";






        private void SaveRemoveLoadAddButtons()
        {
            EditorGUILayout.BeginHorizontal();
            SaveLevelButton();
            RemoveLevelButton();
            LoadLevelButton();
            UnloadLevelButton();
            AddLevelButton();
            EditorGUILayout.EndHorizontal();


            if (!isLoadingLevel || levelReferences == null || levelReferences.references == null) return;
            
            levelScrollPos = EditorGUILayout.BeginScrollView(levelScrollPos);
            GUILayout.Space(10);

            foreach (var levelReference in levelReferences)
            {
                if (levelReference == null || levelReference.editorAsset == null)
                {
                    Debug.LogWarning("LevelReference or its Asset is null!");
                    continue;
                }

                var level = (levelReference.editorAsset as GameObject)?.GetComponent<BaseLevel>();
                if (level == null)
                {
                    Debug.LogWarning($"Component {nameof(BaseLevel)} not found on {levelReference.editorAsset.name}");
                    continue;
                }
                if (GUILayout.Button($"{level.name}"))
                {
                    LoadLevel(levelReference.editorAsset as GameObject);
                }
            }

            GUILayout.Space(10);
            EditorGUILayout.EndScrollView();
        }

        









        private void AddLevelButton()
        {
            if (GUILayout.Button("New Level", GUILayout.Height(40)))
            {
                AddLevel();
            }
        }

        private void LoadLevelButton()
        {
            isLoadingLevel = GUILayout.Toggle(isLoadingLevel, "Load Level", "Button", GUILayout.Height(40));
        }

        private void RemoveLevelButton()
        {
            if (currentLevel == null) return;
            if (GUILayout.Button("Remove Level", GUILayout.Height(40)))
            {
                RemoveLevel(currentLevel);
            }
        }

        private void SaveLevelButton()
        {
            if (currentLevel == null) return;
            if (GUILayout.Button("Save Level", GUILayout.Height(40)))
            {
                SaveLevel(currentLevel);
            }
        }

        private void UnloadLevelButton()
        {
            if (currentLevel == null) return;
            if (GUILayout.Button("Unload Level", GUILayout.Height(40)))
            {
                UnloadLevel();
            }
        }

        protected virtual void RemoveLevel(BaseLevel level)
        {
            // If Level not exist in Assets
            if (!System.IO.File.Exists(GetLevelPath(level)))
            {
                UnloadLevel();
            }
            // If Level exist in Assets
            else if (System.IO.File.Exists(GetLevelPath(level)))
            {
                var levelGUID = AssetDatabase.AssetPathToGUID(GetLevelPath(level));
                levelReferences.references.Remove(levelReferences.GetReferenceFromGUID(levelGUID));
                AssetDatabase.DeleteAsset(GetLevelPath(level));

                UnloadLevel();

                EditorUtility.SetDirty(levelReferences);
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.LogWarning("Level do not exist in Assets!");
            }

            RemoveLevelPerformed?.Invoke();
        }

        protected virtual void AddLevel()
        {
            // If exist, destroy current level on scene
            if (currentLevel != null)
            {
                DestroyImmediate(currentLevel.gameObject);
                currentLevel = null;
            }

            // spawn new one
            if (LevelTemplate != null && LevelTemplate.TryGetComponent<BaseLevel>(out _))
            {
                currentLevel = (PrefabUtility.InstantiatePrefab(LevelTemplate) as GameObject)?.GetComponent<BaseLevel>();
                currentLevel.name = "New Level";
            }
            else
            {
                Debug.LogError($"{nameof(LevelTemplate)} is null or not contains {nameof(BaseLevel)}");
                return;
            }

            AddLevelPerformed?.Invoke();
        }

        protected virtual void SaveLevel(BaseLevel level)
        {
            // Make sure LevelFolder exist
            System.IO.Directory.CreateDirectory(LevelFolderPath);
            level.Index = currentLevelIndex;
            var levelPath = GetLevelPath(level);

            // if level already exist, show dialog
            if (System.IO.File.Exists(levelPath))
            {
                if (!EditorUtility.DisplayDialog("Level Already Exist", "Do you want to replace the existed", "Yes", "No"))
                {
                    return;
                }
            }

            level.name = $"Level {currentLevel.Index}";
            level.EditorPath = LevelFolderPath;
            EditorUtility.SetDirty(level);

            // Save level as prefab
            PrefabUtility.SaveAsPrefabAssetAndConnect(level.gameObject, levelPath, InteractionMode.UserAction);

            // Add to Addressable
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            var levelGUID = AssetDatabase.AssetPathToGUID(levelPath);
            var entry = settings.CreateOrMoveEntry(levelGUID, GetAddressableGroup(LevelAddressableGroup));
            entry.address = levelPath;

            // Save to LevelReference
            if (!System.IO.File.Exists(levelPath) || levelReferences.GetReferenceFromGUID(levelGUID) == null)
            {
                levelReferences.references.Add(new AssetReference(levelGUID));
                EditorUtility.SetDirty(levelReferences);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SaveLevelPerformed?.Invoke();
        }

        protected virtual void LoadLevel(GameObject level)
        {
            // If exist, destroy current level on scene
            if (currentLevel != null)
            {
                DestroyImmediate(currentLevel.gameObject);
                currentLevel = null;
            }

            // Spawn level
            currentLevel = (PrefabUtility.InstantiatePrefab(level) as GameObject)?.GetComponent<BaseLevel>();
            if (currentLevel == null)
            {
                Debug.LogError($"Failed to load level {level.name} because it does not contain {nameof(BaseLevel)} component");
                return;
            }

            currentLevelIndex = currentLevel.Index;
            LevelFolderPath = currentLevel.EditorPath;

            LoadLevelPerformed?.Invoke();
        }

        protected virtual void UnloadLevel()
        {
            DestroyImmediate(currentLevel.gameObject);
            currentLevel = null;
            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }




        private string GetLevelPath(BaseLevel level)
        {
            // Create level path
            var levelName = $"Level {level.Index}.prefab";
            var levelPath = System.IO.Path.Combine(LevelFolderPath, levelName);
            return levelPath;
        }

        private AddressableAssetGroup GetAddressableGroup(string groupName)
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);

            var group = settings.FindGroup(groupName);
            if (group != null) return group;
            
            // Create new group
            group = settings.CreateGroup(
                groupName,
                false,   // readOnly
                false,   // isStaticContent
                true,    // postEvent (notify modification)
                null,    // templates (AddressableAssetGroupTemplate[])
                typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema),
                typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.ContentUpdateGroupSchema)
            );

            Debug.Log($"Created new Addressable group: {groupName}");

            return group;
        }
    }
}
