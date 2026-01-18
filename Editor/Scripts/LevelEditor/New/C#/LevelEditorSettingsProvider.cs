using UnityEditor;

namespace NgoUyenNguyen.Editor
{
    public static class LevelEditorSettingsProvider
    {
        public const string SettingsGuidKey = "LevelEditor.SettingsGUID";

        [SettingsProvider]
        private static SettingsProvider Create()
        {
            return new SettingsProvider(
                "Project/NgoUyenNguyen/Level Editor Settings",
                SettingsScope.Project)
            {
                label = "Level Editor Settings",
                guiHandler = _ =>
                {
                    EditorGUILayout.Space();
                    ProvideSettings();
                    EditorGUILayout.Space();
                }
            };
        }

        private static void ProvideSettings()
        {
            var settingsAsset = LoadSettingsAsset();

            var newAsset = (LevelEditorSettings)EditorGUILayout.ObjectField(
                "Settings Asset",
                settingsAsset,
                typeof(LevelEditorSettings),
                false
            );


            if (newAsset == settingsAsset) return;

            if (newAsset == null)
            {
                EditorPrefs.DeleteKey(SettingsGuidKey);
            }
            else
            {
                var path = AssetDatabase.GetAssetPath(newAsset);
                var guid = AssetDatabase.AssetPathToGUID(path);
                EditorPrefs.SetString(SettingsGuidKey, guid);
            }
        }

        private static LevelEditorSettings LoadSettingsAsset()
        {
            if (!EditorPrefs.HasKey(SettingsGuidKey))
                return null;

            var guid = EditorPrefs.GetString(SettingsGuidKey);
            var path = AssetDatabase.GUIDToAssetPath(guid);

            return string.IsNullOrEmpty(path) 
                ? null 
                : AssetDatabase.LoadAssetAtPath<LevelEditorSettings>(path);
        }
    }
}
