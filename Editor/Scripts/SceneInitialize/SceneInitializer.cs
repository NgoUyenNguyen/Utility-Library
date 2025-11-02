using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class SceneInitializer
{
    private const string PreviousSceneFieldName = "PreviousScene";
    private const string ShouldLoadBootstrapFieldName = "ShouldLoadBootsScene";
    
    
    private static string PreviousScene
    {
        get => EditorPrefs.GetString(PreviousSceneFieldName);
        set => EditorPrefs.SetString(PreviousSceneFieldName, value);
    }
    private static bool ShouldLoadBootstrapScene => EditorPrefs.GetBool(ShouldLoadBootstrapFieldName, false);
    private static string BootstrapScene => EditorBuildSettings.scenes[0].path;

    
    static SceneInitializer()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
    {
        if (!ShouldLoadBootstrapScene)
        {
            return;
        }

        switch (playModeStateChange)
        {
            // This loads bootstrap scene when entering Play mode
            case PlayModeStateChange.ExitingEditMode:

                PreviousScene = EditorSceneManager.GetActiveScene().path;

                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() && IsSceneInBuildSettings(BootstrapScene))
                {
                    EditorSceneManager.OpenScene(BootstrapScene);
                }
                break;

            // This restores the PreviousScene when exiting Play mode
            case PlayModeStateChange.EnteredEditMode:

                if (!string.IsNullOrEmpty(PreviousScene))
                {
                    EditorSceneManager.OpenScene(PreviousScene);
                }
                break;
        }
    }
    
    private static bool IsSceneInBuildSettings(string scenePath)
    {
        return !string.IsNullOrEmpty(scenePath) && EditorBuildSettings.scenes.Any(scene => scene.path == scenePath);
    }
}
