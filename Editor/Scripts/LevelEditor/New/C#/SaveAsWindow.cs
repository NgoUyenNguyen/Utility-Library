using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NgoUyenNguyen.Editor
{
    internal class SaveAsWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTreeAsset;

        public string folderPath;
        public string levelName;
        public event Action SavePerformedEvent;
        
        public static SaveAsWindow GetWindow()
        {
            var rect = new Rect(100, 100, 400, 200);
            var wnd = GetWindowWithRect<SaveAsWindow>(rect, true, "Save As", true);
            
            return wnd;
        }

        public void RefreshUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.Add(visualTreeAsset.CloneTree());

            var saveButton = rootVisualElement.Q<Button>(name: "save");
            saveButton.clicked += OnSaveButtonClicked;

            var cancelButton = rootVisualElement.Q<Button>(name: "cancel");
            cancelButton.clicked += OnCancelButtonClicked;
            
            var selectFolderButton = rootVisualElement.Q<Button>(name: "select-folder");
            selectFolderButton.clicked += OnSelectFolderButtonClicked;
            
            var folderPathLabel = rootVisualElement.Q<Label>(name:"folder-path--label");
            folderPathLabel.text = folderPath;

            var nameTextField = rootVisualElement.Q<TextField>(name: "file-name__text-field");
            nameTextField.value = levelName;
            nameTextField.RegisterValueChangedCallback(evt =>
            {
                levelName = evt.newValue;
            });
        }

        private void OnSelectFolderButtonClicked()
        {
            var absolutePath = EditorUtility.OpenFolderPanel("Select Folder", folderPath, "");

            var relativePath = ConvertAbsoluteToRelativePath(absolutePath);

            if (relativePath != null)
            {
                folderPath = relativePath;
            }
            else if (!string.IsNullOrEmpty(absolutePath))
            {
                if (!EditorUtility.DisplayDialog(
                        "Warning",
                        "Your selected folder is not under Assets folder?\nDo you want to continue?",
                        "Cancel", "Continue"))
                {
                    folderPath = absolutePath;
                }
            }

            rootVisualElement
                .Q<Label>("folder-path--label")
                .text = folderPath;
        }

        private void OnCancelButtonClicked() => Close();
        private void OnSaveButtonClicked()
        {
            if (string.IsNullOrEmpty(levelName))
            {
                EditorUtility.DisplayDialog("Error", "Level name cannot be empty!", "Cancel");
                return;
            }
            Close();
            SavePerformedEvent?.Invoke();
        }

        private static string ConvertAbsoluteToRelativePath(string absolutePath) 
        { 
            if (string.IsNullOrEmpty(absolutePath)) return null; 
            
            // Normalize separators
            absolutePath = absolutePath.Replace('\\', '/'); 
            var dataPath = Application.dataPath.Replace('\\', '/'); 
            
            if (!absolutePath.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase)) return null;
            
            return "Assets" + absolutePath[dataPath.Length..]; 
        }
    }
}