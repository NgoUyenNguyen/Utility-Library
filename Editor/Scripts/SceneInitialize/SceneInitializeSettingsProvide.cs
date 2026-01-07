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

                    var enableFeature = EditorPrefs.GetBool(SceneInitializer.ShouldLoadBootstrapFieldName, false);
                    var newEnableValue = EditorGUILayout.Toggle("Enable Scene Initialize", enableFeature);

                    if (newEnableValue != enableFeature)
                    {
                        enableFeature = newEnableValue;
                        EditorPrefs.SetBool(SceneInitializer.ShouldLoadBootstrapFieldName, newEnableValue);
                    }

                    if (!enableFeature) return;

                    var scenePath = EditorPrefs.GetString(SceneInitializer.ScenePathFieldName, string.Empty);
                    var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                    var newSceneAsset =
                        EditorGUILayout.ObjectField("Scene to Load", sceneAsset, typeof(SceneAsset), false);
                    if (newSceneAsset != sceneAsset)
                    {
                        EditorPrefs.SetString(SceneInitializer.ScenePathFieldName, newSceneAsset != null
                            ? AssetDatabase.GetAssetPath(newSceneAsset)
                            : string.Empty);
                    }
                },
            };

            return provider;
        }
    }
}
