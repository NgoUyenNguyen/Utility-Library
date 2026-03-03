using UnityEditor;
using UnityEngine;

namespace NgoUyenNguyen.Editor
{
    public static class SceneInitializeSettingsProvide
    {
        private const string HelpBoxContent = "Force Unity Editor to always load a Scene when entering PlayMode.\n" +
                                              "Notice that this feature is available in the Editor only!";

        private static SerializedObject serializedSettings;

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider("Project/NgoUyenNguyen/Scene Initialize", SettingsScope.Project)
            {
                label = "Scene Initialize",
                guiHandler = (_) =>
                {
                    var editorSettings = EditorSettings.GetOrCreate();
            
                    if (serializedSettings == null || serializedSettings.targetObject != editorSettings)
                    {
                        serializedSettings = new SerializedObject(editorSettings);
                    }

                    serializedSettings.Update();

                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(HelpBoxContent, MessageType.Info);
                    EditorGUILayout.Space();

                    var enableProp = serializedSettings.FindProperty("sceneInitializeEnable");
                    EditorGUILayout.PropertyField(enableProp, new GUIContent("Enable Scene Initialize"));

                    if (enableProp.boolValue)
                    {
                        var sceneProp = serializedSettings.FindProperty("sceneInitializeSceneAsset");
                        EditorGUILayout.PropertyField(sceneProp, new GUIContent("Scene to Load"));
                    }


                    if (serializedSettings.ApplyModifiedProperties())
                    {
                        AssetDatabase.SaveAssets();
                    }
                },
            };
            return provider;
        }
    }
}