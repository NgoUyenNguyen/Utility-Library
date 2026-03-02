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

            var asset = (LevelEditorSettings)EditorGUILayout.ObjectField(
                "Settings Asset",
                editorSettings.levelEditorSettings,
                typeof(LevelEditorSettings),
                false
            );

            editorSettings.levelEditorSettings = asset;
        }
    }
}
