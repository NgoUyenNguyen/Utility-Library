using UnityEditor;

namespace NgoUyenNguyen.Editor
{
    public static class SceneInitializeSettingsProvide
    {
        private const string HelpBoxContent = "Force Unity Editor to always load a Scene when entering PlayMode.\n" +
                                              "Notice that this feature is available in the Editor only!";

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider("Project/NgoUyenNguyen/Scene Initialize", SettingsScope.Project)
            {
                label = "Scene Initialize",

                // Display GUI
                guiHandler = (_) =>
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(HelpBoxContent, MessageType.Info);
                    EditorGUILayout.Space();

                    var editorSettings = EditorSettings.GetOrCreate();

                    var enable = EditorGUILayout
                        .Toggle("Enable Scene Initialize", editorSettings.sceneInitializeEnable);

                    editorSettings.sceneInitializeEnable = enable;

                    if (!enable) return;

                    var sceneAsset =
                        EditorGUILayout.ObjectField(
                            "Scene to Load",
                            editorSettings.sceneInitializeSceneAsset,
                            typeof(SceneAsset),
                            false
                        );

                    editorSettings.sceneInitializeSceneAsset = (SceneAsset)sceneAsset;
                },
            };

            return provider;
        }
    }
}