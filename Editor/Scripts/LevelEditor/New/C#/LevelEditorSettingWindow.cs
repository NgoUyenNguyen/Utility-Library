using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NgoUyenNguyen.Editor
{
    internal class LevelEditorSettingWindow : EditorWindow
    {
        public const string DefaultSettingsPath = "Assets/Editor/LevelEditorSettings.asset";
        
        [SerializeField] private VisualTreeAsset visualTreeAsset;
        private LevelEditorSettings settings;
        
        public static LevelEditorSettingWindow GetWindow(LevelEditorSettings settings)
        {
            var wnd = GetWindow<LevelEditorSettingWindow>(true, "Level Editor Settings", true);
            wnd.settings = settings;
            wnd.minSize = new Vector2(750, 600);
            
            wnd.RefreshUI();
            return wnd;
        }
        
        private void RefreshUI()
        {
            rootVisualElement.Clear();

            if (settings == null)
            {
                rootVisualElement.Add(new Label("No settings assigned."));
                return;
            }
            
            var serializedObject = new SerializedObject(settings);
            rootVisualElement.Add(visualTreeAsset.CloneTree());
            var inspectorElement = rootVisualElement.Q<InspectorElement>(name:"content");
            inspectorElement.Unbind();
            inspectorElement.Bind(serializedObject);
        }
    }
}