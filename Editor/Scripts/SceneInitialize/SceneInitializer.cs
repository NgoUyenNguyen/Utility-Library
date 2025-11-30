using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class SceneInitializer
{
    public const string PreviousSceneFieldName = "PreviousScene";
    public const string ShouldLoadBootstrapFieldName = "ShouldLoadBootsScene";
    public const string ScenePathFieldName = "ScenePathToLoad";


    private static string PreviousScene
    {
        get => EditorPrefs.GetString(PreviousSceneFieldName);
        set => EditorPrefs.SetString(PreviousSceneFieldName, value);
    }

    private static bool ShouldLoadBootstrapScene => EditorPrefs.GetBool(ShouldLoadBootstrapFieldName, false);
    private static string ScenePathToLoad => EditorPrefs.GetString(ScenePathFieldName, null);


    static SceneInitializer()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
    {
        if (!ShouldLoadBootstrapScene || ScenePathToLoad == string.Empty) return;

        switch (playModeStateChange)
        {
            // This loads bootstrap scene when entering Play mode
            case PlayModeStateChange.ExitingEditMode:
                EditorSceneManager.SaveOpenScenes();
                PreviousScene = SceneManager.GetActiveScene().path;
                EditorSceneManager.OpenScene(ScenePathToLoad);
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
}