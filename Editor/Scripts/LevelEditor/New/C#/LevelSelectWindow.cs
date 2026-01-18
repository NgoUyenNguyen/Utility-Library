using System;
using NgoUyenNguyen.ScriptableObjects;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace NgoUyenNguyen.Editor
{
    public class LevelSelectWindow : PopupWindowContent
    {
        private const string VisualTreeAssetPath =
            "Packages/com.ngouyennguyen.utilitylibrary/Editor/Scripts/LevelEditor/New/UXML/LevelSelectWindow.uxml";

        private const string ButtonTemplatePath =
            "Packages/com.ngouyennguyen.utilitylibrary/Editor/Scripts/LevelEditor/New/UXML/LevelReferenceButton.uxml";

        private readonly LevelReference levelReference;
        private readonly Action<AssetReference> onLevelSelected;

        public LevelSelectWindow(LevelReference levelReference, Action<AssetReference> onLevelSelected)
        {
            this.levelReference = levelReference;
            this.onLevelSelected = onLevelSelected;
        }

        public override void OnOpen()
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(VisualTreeAssetPath);
            var buttonTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ButtonTemplatePath);
            var root = editorWindow.rootVisualElement;

            root.Add(visualTreeAsset.CloneTree());
            var listView = root.Q<ListView>();
            listView.itemsSource = levelReference.References;
            listView.makeItem = buttonTemplate.CloneTree;
            listView.bindItem = (element, index) =>
            {
                SetLabel(element.Q<Label>(), index);
                SetButton(element.Q<Button>(), levelReference[index]);
                element.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    evt.menu.AppendAction("Load",
                        _ => 
                        {
                            editorWindow.Close();
                            onLevelSelected?.Invoke(levelReference[index]);
                        },
                        _ => levelReference[index] == null || levelReference[index].editorAsset == null
                            ? DropdownMenuAction.Status.Disabled
                            : DropdownMenuAction.Status.Normal);
                    evt.menu.AppendAction("Delete",
                        _ =>
                        {
                            levelReference.RemoveAt(index);
                            listView.RefreshItems();
                        });
                }));
            };
        }

        private void SetButton(Button button, AssetReference assetReference)
        {
            if (assetReference == null || assetReference.editorAsset == null)
            {
                button.style.display = DisplayStyle.None;
                return;
            }

            button.style.display = DisplayStyle.Flex;
            button.text = GetFileNameFromGuid(assetReference.AssetGUID);

            button.clicked += () =>
            {
                editorWindow.Close();
                onLevelSelected?.Invoke(assetReference);
            };
        }

        private static void SetLabel(Label label, int index)
        {
            label.text = $"{index}.";
        }

        private static string GetFileNameFromGuid(string guid)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            return System.IO.Path.GetFileNameWithoutExtension(assetPath);
        }

        public override Vector2 GetWindowSize() => new(400, 1000);
    }
}