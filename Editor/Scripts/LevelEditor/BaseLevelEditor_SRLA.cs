using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NgoUyenNguyen.Editor
{
    public partial class BaseLevelEditor
    {
        private const string LEVEL_FOLDER_PATH = "Assets/Levels";
        private const string LEVEL_ADDRESSABLES_GROUP = "Levels";

        private bool isLoadingLevel;
        private static Vector2 levelScrollPos;
        public event Action removeLevelPerformed;
        public event Action loadLevelPerformed;
        public event Action saveLevelPerformed;
        public event Action addLevelPerformed;







        private void SaveRemoveLoadAddButtons()
        {
            EditorGUILayout.BeginHorizontal();
            SaveLevelButton();
            RemoveLevelButton();
            LoadLevelButton();
            AddLevelButton();
            EditorGUILayout.EndHorizontal();


            if (isLoadingLevel && levelReference != null)
            {
                levelScrollPos = EditorGUILayout.BeginScrollView(levelScrollPos);
                GUILayout.Space(10);

                foreach (var levelReference in levelReference.references)
                {
                    if (levelReference == null || levelReference.editorAsset == null)
                    {
                        Debug.LogWarning("LevelReference or its editorAsset is null!");
                        continue;
                    }

                    BaseLevel level = (levelReference.editorAsset as GameObject).GetComponent<BaseLevel>();
                    if (GUILayout.Button($"Level {level.index}"))
                    {
                        LoadLevel(levelReference.editorAsset as GameObject);
                    }
                }

                GUILayout.Space(10);
                EditorGUILayout.EndScrollView();
            }
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
            if (currentLevel != null)
            {
                if (GUILayout.Button("Remove Level", GUILayout.Height(40)))
                {
                    RemoveLevel(currentLevel);
                }
            }
        }

        private void SaveLevelButton()
        {
            if (currentLevel != null)
            {
                if (GUILayout.Button("Save Level", GUILayout.Height(40)))
                {
                    SaveLevel(currentLevel.gameObject);
                }
            }
        }










        private void RemoveLevel(BaseLevel level)
        {
            // If Level not exist in Assets
            if (!System.IO.File.Exists(GetLevelPath(level)))
            {
                DestroyImmediate(currentLevel.gameObject);
                currentLevel = null;
            }
            // If Level exist in Assets
            else if (System.IO.File.Exists(GetLevelPath(level)))
            {
                string levelGUID = AssetDatabase.AssetPathToGUID(GetLevelPath(level));
                levelReference.references.Remove(levelReference.GetReferenceFromGUID(levelGUID));
                AssetDatabase.DeleteAsset(GetLevelPath(level));

                DestroyImmediate(currentLevel.gameObject);
                currentLevel = null;

                EditorUtility.SetDirty(levelReference);
            }
            else
            {
                Debug.LogWarning("Level do not exist in Assets!");
            }

            removeLevelPerformed?.Invoke();
        }

        private void AddLevel()
        {
            // If exist, destroy current level on scene
            if (currentLevel != null)
            {
                DestroyImmediate(currentLevel.gameObject);
                currentLevel = null;
            }

            // spawn new one
            if (levelTemplate != null && levelTemplate.TryGetComponent<BaseLevel>(out BaseLevel level))
            {
                currentLevel = (PrefabUtility.InstantiatePrefab(levelTemplate) as GameObject).GetComponent<BaseLevel>();
                currentLevel.name = "New Level";
            }
            else
            {
                Debug.LogError($"{nameof(levelTemplate)} is null or not contains {nameof(BaseLevel)}");
                return;
            }

            addLevelPerformed?.Invoke();
        }

        private void SaveLevel(GameObject level)
        {
            // Make sure LevelFolder exist
            System.IO.Directory.CreateDirectory(LEVEL_FOLDER_PATH);
            string levelPath = GetLevelPath(currentLevel);

            // if level already exist, show dialog
            if (System.IO.File.Exists(levelPath))
            {
                if (!EditorUtility.DisplayDialog("Level Already Exist", "Do you want to replace the existed", "Yes", "No"))
                {
                    return;
                }
            }

            // Change level name on scene
            currentLevel.name = $"Level {currentLevel.index}";
            EditorUtility.SetDirty(currentLevel);

            // Save level as prefab
            PrefabUtility.SaveAsPrefabAssetAndConnect(level, levelPath, InteractionMode.UserAction);

            // Add to Addressables
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            string levelGUID = AssetDatabase.AssetPathToGUID(levelPath);
            var entry = settings.CreateOrMoveEntry(levelGUID, GetAddressablesGroup(LEVEL_ADDRESSABLES_GROUP));
            entry.address = levelPath;

            // Save to LevelReference
            if (!System.IO.File.Exists(levelPath) || levelReference.GetReferenceFromGUID(levelGUID) == null)
            {
                levelReference.references.Add(new AssetReference(levelGUID));
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            saveLevelPerformed?.Invoke();
        }

        private void LoadLevel(GameObject level)
        {
            // If exist, destroy current level on scene
            if (currentLevel != null)
            {
                DestroyImmediate(currentLevel.gameObject);
                currentLevel = null;
            }

            // Spawn level
            currentLevel = (PrefabUtility.InstantiatePrefab(level) as GameObject).GetComponent<BaseLevel>();
            currentLevel.name = level.name;

            loadLevelPerformed?.Invoke();
        }




        private string GetLevelPath(BaseLevel level)
        {
            // Create level path
            string levelName = $"Level {level.index}.prefab";
            string levelPath = System.IO.Path.Combine(LEVEL_FOLDER_PATH, levelName);
            return levelPath;
        }

        private AddressableAssetGroup GetAddressablesGroup(string groupName)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings not found. Did you set up Addressables?");
                return null;
            }

            var group = settings.FindGroup(groupName);
            if (group == null)
            {
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

                Debug.Log($"Created new Addressables group: {groupName}");
            }

            return group;
        }
    }
}
