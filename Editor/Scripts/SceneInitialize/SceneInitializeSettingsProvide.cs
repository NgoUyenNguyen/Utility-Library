using UnityEditor;

public static class SceneInitializeSettingsProvide
{
    private const string HelpBoxContent = "Force Unity Editor to always load the first scene in Build Setting.\n" +
                                          "Notice that this feature is available in the Editor only!";
    
    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        var provider = new SettingsProvider("Project/Scene Initialize", SettingsScope.Project)
        {
            label = "Scene Initialize",
            
            // Display GUI
            guiHandler = (_) =>
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(HelpBoxContent, MessageType.Info);
                EditorGUILayout.Space();
                
                var enableFeature = EditorPrefs.GetBool("ShouldLoadBootsScene", false);
                var newValue = EditorGUILayout.Toggle("Enable Scene Initialize", enableFeature);

                if (newValue != enableFeature)
                    EditorPrefs.SetBool("ShouldLoadBootsScene", newValue);
            },
        };
        
        return provider;
    }
}
