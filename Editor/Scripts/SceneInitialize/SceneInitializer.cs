using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace NgoUyenNguyen.Editor
{
    [InitializeOnLoad]
    public static class SceneInitializer
    {
        public const string PreviousScenesFieldName = "PreviousScenes";
        public const string ActiveSceneFieldName = "ActiveScene";
        public const string ShouldLoadBootstrapFieldName = "ShouldLoadBootsScene";
        public const string ScenePathFieldName = "ScenePathToLoad";

        private static string PreviousScenes
        {
            get => EditorPrefs.GetString(PreviousScenesFieldName);
            set => EditorPrefs.SetString(PreviousScenesFieldName, value);
        }

        private static string ActiveScene
        {
            get => EditorPrefs.GetString(ActiveSceneFieldName);
            set => EditorPrefs.SetString(ActiveSceneFieldName, value);
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
                // Load the bootstrap scene when entering Play mode
                case PlayModeStateChange.ExitingEditMode:
                    EditorSceneManager.SaveOpenScenes();

                    // Save all opened scenes
                    var openedScenes = new string[SceneManager.sceneCount];
                    for (int i = 0; i < SceneManager.sceneCount; i++)
                    {
                        openedScenes[i] = SceneManager.GetSceneAt(i).path;
                    }

                    PreviousScenes = string.Join("|", openedScenes);

                    // Save which one is the active scene
                    ActiveScene = SceneManager.GetActiveScene().path;

                    EditorSceneManager.OpenScene(ScenePathToLoad);
                    break;

                // This restores all previous scenes when exiting Play mode
                case PlayModeStateChange.EnteredEditMode:
                    if (!string.IsNullOrEmpty(PreviousScenes))
                    {
                        var scenePaths = PreviousScenes.Split('|');

                        // Open one scene to replace the bootstrap scene
                        if (scenePaths.Length > 0 && !string.IsNullOrEmpty(scenePaths[0]))
                        {
                            EditorSceneManager.OpenScene(scenePaths[0], OpenSceneMode.Single);
                        }

                        // Open all other scenes
                        for (int i = 1; i < scenePaths.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(scenePaths[i]))
                            {
                                EditorSceneManager.OpenScene(scenePaths[i], OpenSceneMode.Additive);
                            }
                        }

                        // Reset the active scene to the one before
                        if (!string.IsNullOrEmpty(ActiveScene))
                        {
                            var activeScene = SceneManager.GetSceneByPath(ActiveScene);
                            if (activeScene.IsValid())
                            {
                                SceneManager.SetActiveScene(activeScene);
                            }
                        }
                    }

                    break;
            }
        }
    }
}