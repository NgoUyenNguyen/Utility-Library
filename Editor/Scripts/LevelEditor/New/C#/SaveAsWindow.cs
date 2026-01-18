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
            var absolutePath = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
            var dataPath = Application.dataPath;
            
            if (absolutePath.StartsWith(dataPath))
            {
                folderPath = "Assets" + absolutePath[dataPath.Length..];
            }
            else if (!string.IsNullOrEmpty(absolutePath))
            {
                if (!EditorUtility.DisplayDialog(
                        "Warning", 
                        "Your selected folder is not under Assets folder?\n Do you want to continue?", 
                        "Cancel", "Continue"))
                {
                    folderPath = absolutePath;
                }
            }
            
            var folderPathLabel = rootVisualElement.Q<Label>(name:"folder-path--label");
            folderPathLabel.text = folderPath;
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
    }
}