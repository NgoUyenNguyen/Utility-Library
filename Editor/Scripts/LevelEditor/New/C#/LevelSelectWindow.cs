using System;
using System.Collections.Generic;
using NgoUyenNguyen.ScriptableObjects;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace NgoUyenNguyen.Editor
{
    public class LevelSelectWindow : PopupWindowContent
    {
        private const string VisualTreeAssetPath =
            "Packages/com.ngouyennguyen.utilitylibrary/Editor/Scripts/LevelEditor/New/UXML/LevelSelectWindow.uxml";

        private readonly LevelReference levelReference;
        private readonly Action<AssetReference> onLevelSelected;
        private readonly List<AssetReference> filteredReferences;
        private ListView listView;

        public LevelSelectWindow(LevelReference levelReference, Action<AssetReference> onLevelSelected)
        {
            this.levelReference = levelReference;
            this.onLevelSelected = onLevelSelected;
            
            filteredReferences = new List<AssetReference>(levelReference.References);
        }

        public override void OnOpen()
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(VisualTreeAssetPath);
            var root = editorWindow.rootVisualElement;

            root.Add(visualTreeAsset.CloneTree());
            
            var searchField = root.Q<ToolbarSearchField>(name:"search-field");
            listView = root.Q<ListView>();
            
            ConfigureSearchArea(searchField);
            ConfigureListView();
        }

        private void ConfigureSearchArea(ToolbarSearchField searchField)
        {
            searchField.RegisterValueChangedCallback(ApplyFilter);
        }
        
        private void ApplyFilter(ChangeEvent<string> evt)
        {
            filteredReferences.Clear();

            if (string.IsNullOrWhiteSpace(evt.newValue))
            {
                filteredReferences.AddRange(levelReference.References);
            }
            else
            {
                var searchText = evt.newValue.ToLowerInvariant();

                foreach (var reference in levelReference.References)
                {
                    if (reference == null) continue;

                    var name = GetFileNameFromGuid(reference.AssetGUID);
                    if (name.ToLowerInvariant().Contains(searchText))
                        filteredReferences.Add(reference);
                }
            }

            listView.RefreshItems();
        }

        private void ConfigureListView()
        {
            listView.itemsSource = filteredReferences;
            listView.bindItem = (element, index) =>
            {
                SetLabel(element.Q<Label>(), index);
                SetButton(element.Q<Button>(), filteredReferences[index]);
                element.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    evt.menu.AppendAction("Load",
                        _ => 
                        {
                            editorWindow.Close();
                            onLevelSelected?.Invoke(filteredReferences[index]);
                        },
                        _ => filteredReferences[index] == null 
                             || filteredReferences[index].editorAsset == null
                            ? DropdownMenuAction.Status.Disabled
                            : DropdownMenuAction.Status.Normal);
                    evt.menu.AppendAction("Delete",
                        _ =>
                        {
                            filteredReferences.RemoveAt(index);
                            listView.RefreshItems();
                        });
                }));
            };
        }

        private void SetButton(Button button, AssetReference assetReference)
        {
            button.clicked -= button.userData as Action;

            if (assetReference == null || assetReference.editorAsset == null)
            {
                button.style.display = DisplayStyle.None;
                return;
            }

            button.style.display = DisplayStyle.Flex;
            button.text = GetFileNameFromGuid(assetReference.AssetGUID);

            Action click = () =>
            {
                editorWindow.Close();
                onLevelSelected?.Invoke(assetReference);
            };

            button.userData = click;
            button.clicked += click;
        }


        private static void SetLabel(Label label, int index) => label.text = $"{index}.";

        private static string GetFileNameFromGuid(string guid)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            return System.IO.Path.GetFileNameWithoutExtension(assetPath);
        }

        public override Vector2 GetWindowSize() => new(400, 1000);
    }
}