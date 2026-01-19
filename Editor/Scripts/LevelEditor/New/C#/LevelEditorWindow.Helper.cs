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

        
        public static string GetFullPath(LevelSaveDescription<TLevelData> saveDescription, string extension)
        {
            if (!extension.StartsWith('.'))
            {
                extension = '.' + extension;
            }
            
            return System.IO.Path.Combine(saveDescription.FolderPath, saveDescription.Name + extension);
        }
        
        public static GameObject SaveAsPrefab(GameObject gameObject, string fullPath)
            => PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, fullPath, InteractionMode.UserAction);

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