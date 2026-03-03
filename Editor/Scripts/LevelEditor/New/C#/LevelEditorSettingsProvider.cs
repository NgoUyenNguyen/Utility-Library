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
                },
            };
        }

        private static void ProvideSettings()
        {
            var editorSettings = EditorSettings.GetOrCreate();

            using var check = new EditorGUI.ChangeCheckScope();
            var asset = (LevelEditorSettings)EditorGUILayout.ObjectField(
                "Settings Asset",
                editorSettings.levelEditorSettings,
                typeof(LevelEditorSettings),
                false
            );

            if (!check.changed) return;
            editorSettings.levelEditorSettings = asset;

            EditorUtility.SetDirty(editorSettings);

            AssetDatabase.SaveAssets();
        }
    }
}
