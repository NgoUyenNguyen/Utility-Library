using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NgoUyenNguyen.Editor
{
    public partial class LevelEditorWindow<TLevelData>
    {
        /// <summary>
        /// Attempts to convert an absolute file path to a relative Unity asset path.
        /// </summary>
        /// <param name="absolutePath">
        /// The absolute file path to be converted.
        /// </param>
        /// <param name="relativePath">
        /// When this method returns, contains the relative Unity asset path if the conversion is successful; otherwise, null.
        /// </param>
        /// <returns>
        /// true if the conversion is successful; otherwise, false.
        /// </returns>
        public static bool TryGetAssetPath(
            string absolutePath,
            out string relativePath)
        {
            relativePath = null;
            if (string.IsNullOrEmpty(absolutePath)) return false;

            absolutePath = absolutePath.Replace('\\', '/');
            var dataPath = Application.dataPath.Replace('\\', '/');

            if (!absolutePath.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase))
                return false;

            relativePath = "Assets" + absolutePath[dataPath.Length..];
            return true;
        }

        /// <summary>
        /// Constructs the full file path for a given level save description and file extension.
        /// </summary>
        /// <param name="saveDescription">
        /// An object containing the metadata about the level, including the folder path and file name.
        /// </param>
        /// <param name="extension">
        /// The file extension to append to the file name. If the extension does not start with a '.', it will be automatically prefixed.
        /// </param>
        /// <returns>
        /// The full file path as a string, combining the folder path, file name, and extension.
        /// </returns>
        public static string GetFullPath(LevelSaveDescription<TLevelData> saveDescription, string extension)
        {
            if (!extension.StartsWith('.'))
            {
                extension = '.' + extension;
            }
            
            return System.IO.Path.Combine(saveDescription.FolderPath, saveDescription.Name + extension);
        }

        /// <summary>
        /// Saves the given GameObject as a prefab at the specified file path.
        /// </summary>
        /// <param name="gameObject">
        /// The GameObject to be saved as a prefab.
        /// </param>
        /// <param name="fullPath">
        /// The file path where the prefab will be saved.
        /// </param>
        /// <returns>
        /// The saved GameObject as a prefab asset.
        /// </returns>
        public static GameObject SaveAsPrefab(GameObject gameObject, string fullPath)
            => PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, fullPath, InteractionMode.UserAction);

        /// <summary>
        /// Saves the specified level asset to the level reference system and configures it to be addressable.
        /// </summary>
        /// <param name="fullPath">
        /// The full file path of the level asset to save.
        /// </param>
        /// <param name="addressableGroup">
        /// The addressable group name to assign to the level asset. Defaults to "Levels" if not specified.
        /// </param>
        /// <returns>
        /// true if the level asset is successfully saved and configured; otherwise, false.
        /// </returns>
        public bool SaveToLevelReference(string fullPath, string addressableGroup = "Levels")
        {
            var levelReferences = Settings.levelReference;
            if (levelReferences == null) return false;
            
            var levelGuid = AssetDatabase.AssetPathToGUID(fullPath);
            if (System.IO.File.Exists(fullPath) 
                && levelReferences.GetReferenceFromGUID(levelGuid) != null)
                return false;
            
            AddToAddressable(fullPath, addressableGroup);
            
            levelReferences.Add(new AssetReference(levelGuid));
            EditorUtility.SetDirty(levelReferences);
            return true;
        }

        /// <summary>
        /// Adds an asset to the Addressable settings and assigns it to the specified Addressable group.
        /// </summary>
        /// <param name="fullPath">
        /// The full path of the asset to be added to the Addressable system.
        /// </param>
        /// <param name="addressableGroup">
        /// The name of the Addressable group the asset should be assigned to. Defaults to "Levels" if not specified.
        /// </param>
        public static void AddToAddressable(string fullPath, string addressableGroup = "Levels")
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            var levelGuid = AssetDatabase.AssetPathToGUID(fullPath);
            var entry = settings.CreateOrMoveEntry(levelGuid, GetAddressableGroup(addressableGroup));
            entry.address = fullPath;
            
            EditorUtility.SetDirty(settings);
        }
        
        private static AddressableAssetGroup GetAddressableGroup(string groupName)
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            
            if (string.IsNullOrEmpty(groupName))
                return settings.DefaultGroup;

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