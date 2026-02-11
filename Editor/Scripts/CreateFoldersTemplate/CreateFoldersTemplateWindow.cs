using System;
using System.Collections.Generic;
using System.IO;
using Unity.Properties;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NgoUyenNguyen.Editor
{
    public class CreateFoldersTemplateWindow : EditorWindow
    {
        #region Readonly
        private const string DefaultRootFolder = "Assets/_Project";

        private readonly Folder[] folderStructure =
        {
            new("Art", children: new[]
                {
                    new Folder("Materials"),
                    new Folder("Models"),
                    new Folder("Textures"),
                    new Folder("Animations"),
                    new Folder("Particles"),
                    new Folder("Fonts")
                }
            ),
            new("Audio", children: new[]
                {
                    new Folder("Music"),
                    new Folder("Sound")
                }
            ),
            new("Code", children: new[]
                {
                    new Folder("Scripts", children: new[]
                        {
                            new Folder("Entities"),
                            new Folder("Systems"),
                            new Folder("ScriptableObjects"),
                            new Folder("UI")
                        }
                    ),
                    new Folder("Shaders")
                }
            ),
            new("Docs"),
            new("Designs", children: new[]
                {
                    new Folder("Prefabs", children: new[]
                        {
                            new Folder("Entities"),
                            new Folder("UI")
                        }
                    ),
                    new Folder("Scenes")
                }
            ),
            new("Editor")
        };
        #endregion

        [SerializeField] private VisualTreeAsset visualTreeAsset;
        [SerializeField, HideInInspector] private string rootFolderPath = DefaultRootFolder;
        private List<TreeViewItemData<Folder>> rootFolder;
        private SerializedObject serializedObject;

        [MenuItem("Tools/NgoUyenNguyen/Create Folders Template")]
        public static void GetWindow()
        {
            var wnd = GetWindow<CreateFoldersTemplateWindow>(true, "Create Folders Template", true);
            wnd.minSize = new Vector2(1000, 200);
        }

        public void CreateGUI()
        {
            InitializeVisualTree();
            InitializeFolderPath();
            InitializeButtons();
            CalculateTreeItems();
            InitializeFolderPreview();
            
            serializedObject = new SerializedObject(this);
            rootVisualElement.Bind(serializedObject);
        }

        private void InitializeButtons()
        {
            rootVisualElement.Q<Button>(name: "select-folder").clicked += OnSelectFolderClicked;
            rootVisualElement.Q<Button>(name: "include-all").clicked += OnIncludeAllClicked;
            rootVisualElement.Q<Button>(name: "exclude-all").clicked += OnExcludeAllClicked;
            rootVisualElement.Q<Button>(name: "create").clicked += OnCreateClicked;
        }

        private void OnSelectFolderClicked()
        {
            var absolutePath = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");

            var relativePath = ConvertAbsoluteToRelativePath(absolutePath);
            if (string.IsNullOrEmpty(absolutePath)) relativePath = DefaultRootFolder;
            
            if (relativePath == null
                && EditorUtility.DisplayDialog("Warning",
                    "The selected folder is outside folder Assets!", 
                    "Cancel"))
            {
                return;
            }

            SetRootFolderPath(relativePath);
            CalculateTreeItems();
            InitializeFolderPreview();
        }
        
        private void SetRootFolderPath(string value)
        {
            serializedObject.Update();
            var prop = serializedObject.FindProperty(nameof(rootFolderPath));
            prop.stringValue = value;
            serializedObject.ApplyModifiedProperties();
        }


        private void OnIncludeAllClicked() => SetIncludeValue(true);

        private void OnExcludeAllClicked() => SetIncludeValue(false);

        private void SetIncludeValue(bool value)
        {
            foreach (var root in rootFolder)
            {
                SetIncludeValueRecursive(root.data, value);
            }

            var preview = rootVisualElement.Q<MultiColumnTreeView>("preview");
            preview.RefreshItems();
        }
        
        private void SetIncludeValueRecursive(Folder folder, bool value)
        {
            folder.IncludeGitKeep = value;
            if (folder.Children == null) return;

            foreach (var child in folder.Children)
            {
                SetIncludeValueRecursive(child, value);
            }
        }

        private void OnCreateClicked()
        {
            Close();
            
            foreach (var root in rootFolder)
            {
                CreateFolderRecursive(null, root.data);
            }

            AssetDatabase.Refresh();
        }

        private void CreateFolderRecursive(string parentPath, Folder folder)
        {
            var currentPath = parentPath == null
                ? folder.Name
                : $"{parentPath}/{folder.Name}";

            if (!AssetDatabase.IsValidFolder(currentPath))
            {
                AssetDatabase.CreateFolder(parentPath ?? "Assets", folder.Name);
            }

            if (folder.Children == null || folder.Children.Length == 0)
            {
                if (!folder.IncludeGitKeep) return;
                var gitKeepPath = Path.Combine(currentPath, ".gitkeep");
                File.WriteAllText(gitKeepPath, string.Empty);

                return;
            }

            foreach (var child in folder.Children)
            {
                CreateFolderRecursive(currentPath, child);
            }
        }

        private void InitializeFolderPreview()
        {
            var preview = rootVisualElement.Q<MultiColumnTreeView>(name: "preview");
            preview.Clear();
            
            preview.SetRootItems(rootFolder);
            preview.ExpandAll();
            
            preview.columns[0].bindCell = (element, id) =>
            {
                var folderName = element.Q<Label>();
                folderName.text = preview.GetItemDataForIndex<Folder>(id).Name;
            };
            preview.columns[1].bindCell = (element, id) =>
            {
                var data = preview.GetItemDataForIndex<Folder>(id);

                element.style.display = data.Children?.Length > 0
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;

                var toggle = element.Q<Toggle>();

                toggle.SetValueWithoutNotify(data.IncludeGitKeep);
                toggle.UnregisterCallback<ChangeEvent<bool>>(OnToggleChanged);
                toggle.RegisterCallback<ChangeEvent<bool>>(OnToggleChanged);
                return;

                void OnToggleChanged(ChangeEvent<bool> evt)
                {
                    data.IncludeGitKeep = evt.newValue;
                }
            };
        }

        private void CalculateTreeItems()
        {
            var rootFolderNames = rootFolderPath.Split('/');
            var rootFolders = new Folder[rootFolderNames.Length];
            for (var i = 0; i < rootFolders.Length; i++)
            {
                rootFolders[i] = new Folder(rootFolderNames[i]);
                if (i > 0)
                {
                    rootFolders[i - 1].Children = new []{rootFolders[i]};
                }
            }
            
            rootFolders[^1].Children = folderStructure;
            rootFolder = BuildTreeViewItemData(rootFolders[0]);
        }

        private List<TreeViewItemData<Folder>> BuildTreeViewItemData(Folder rootFolder)
        {
            var id = 0;
            return new List<TreeViewItemData<Folder>>
            {
                BuildTreeViewItemDataRecursive(rootFolder, ref id)
            };
        }

        private TreeViewItemData<Folder> BuildTreeViewItemDataRecursive(
            Folder folder,
            ref int id)
        {
            var children = new List<TreeViewItemData<Folder>>();

            if (folder.Children != null)
            {
                foreach (var child in folder.Children)
                {
                    children.Add(BuildTreeViewItemDataRecursive(child, ref id));
                }
            }

            return new TreeViewItemData<Folder>(id++, folder, children);
        }

        private void InitializeFolderPath()
        {
            rootVisualElement.Q<Label>(name:"folder-path").bindingPath = nameof(rootFolderPath);
        }

        private void InitializeVisualTree()
        {
            var visualTree = visualTreeAsset.CloneTree();
            visualTree.style.width = new StyleLength(Length.Percent(100));
            visualTree.style.height = new StyleLength(Length.Percent(100));
            visualTree.dataSource = this;
            
            rootVisualElement.Add(visualTree);
        }
        
        private static string ConvertAbsoluteToRelativePath(string absolutePath) 
        { 
            if (string.IsNullOrEmpty(absolutePath)) return null; 
            
            // Normalize separators
            absolutePath = absolutePath.Replace('\\', '/'); 
            var dataPath = Application.dataPath.Replace('\\', '/'); 
            
            if (absolutePath == Path.GetDirectoryName(dataPath)?.Replace('\\', '/')) return "Assets";
            
            if (!absolutePath.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase)) return null;
            
            return "Assets" + absolutePath[dataPath.Length..]; 
        }
        
        private class Folder
        {
            public readonly string Name;
            public Folder[] Children;
            public bool IncludeGitKeep;

            public Folder(string name, bool includeGitKeep = true, params Folder[] children)
            {
                Name = name;
                Children = children;
                IncludeGitKeep = includeGitKeep;
            }
        }
    }
}