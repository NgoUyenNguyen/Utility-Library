using UnityEditor;
using UnityEngine;

namespace NgoUyenNguyen.Editor
{
    public class EditorSettings : ScriptableObject
    {
        [HideInInspector]
        public bool sceneInitializeEnable;
        [HideInInspector]
        public SceneAsset sceneInitializeSceneAsset;
        
        [HideInInspector]
        public LevelEditorSettings levelEditorSettings;
        
        
        
        public static EditorSettings GetOrCreate()
        {
            const string settingsPath = "Assets/Settings/NgoUyenNguyen.EditorSettings.asset";
            var settings = AssetDatabase.LoadAssetAtPath<EditorSettings>(settingsPath);
            
            // ReSharper disable once InvertIf
            if (settings == null)
            {
                settings = CreateInstance<EditorSettings>();
                if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                    AssetDatabase.CreateFolder("Assets", "Settings");
                
                AssetDatabase.CreateAsset(settings, settingsPath);
                AssetDatabase.ImportAsset(settingsPath);
            }
            return settings;
        }
    }
}
