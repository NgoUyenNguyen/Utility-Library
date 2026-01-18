using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace NgoUyenNguyen.Editor
{
    public abstract partial class LevelEditorWindow<TLevelData> : EditorWindow
    {
        private const string UxmlAssetPath =
            "Packages/com.ngouyennguyen.utilitylibrary/Editor/Scripts/LevelEditor/New/UXML/LevelEditorWindow.uxml";
        
        private string windowName;
        private string levelFolderPath;
        private string levelName;
        private object levelData;

        internal static LevelEditorSettings Settings
        {
            get
            {
                var guid = 
                    EditorPrefs.GetString(LevelEditorSettingsProvider.SettingsGuidKey, string.Empty);
                
                var path = LevelEditorSettingWindow.DefaultSettingsPath;
                if (!string.IsNullOrEmpty(guid))
                {
                    path = AssetDatabase.GUIDToAssetPath(guid);
                }
                
                var settings = AssetDatabase.LoadAssetAtPath<LevelEditorSettings>(path);
                if (settings == null)
                {
                    settings = CreateInstance<LevelEditorSettings>();
                    AssetDatabase.CreateAsset(settings, LevelEditorSettingWindow.DefaultSettingsPath);
                    Debug.Log($"Created new settings asset at '{LevelEditorSettingWindow.DefaultSettingsPath}'");
                }

                return settings;
            }
        }

        public TLevelData LevelData
        {
            get
            {
                ChangeTitle(true);
                return (TLevelData)levelData;
            }
            set
            {
                ChangeTitle(true);
                levelData = value;
            }
        }

        private void OnGUI()
        {
            var e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.S && e.control)
            {
                OnRequestSave();
                e.Use();
            }
            else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.N && e.control)
            {
                InternalNew();
                e.Use();
            }
            else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.L && e.control)
            {
                OnRequestLoad();
                e.Use();
            }
        }

        protected virtual void CreateGUI()
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlAssetPath);
            rootVisualElement.Add(visualTreeAsset.CloneTree());

            Init();
            SetupElements(rootVisualElement.Q<ToolbarButton>(name: "save-button"),
                rootVisualElement.Q<ToolbarButton>(name: "setting-button"),
                rootVisualElement.Q<ToolbarMenu>(name: "option"),
                rootVisualElement.Q<IMGUIContainer>(name: "content--IMGUI"),
                rootVisualElement.Q<VisualElement>(name: "content"),
                rootVisualElement.Q<Toolbar>(name: "toolbar-left"),
                rootVisualElement.Q<Toolbar>(name: "toolbar-right"),
                rootVisualElement.Q<IMGUIContainer>(name: "toolbar-left--IMGUI"),
                rootVisualElement.Q<IMGUIContainer>(name: "toolbar-right--IMGUI"));
        }

        private void Init()
        {
            windowName = titleContent.text;
            ChangeTitle(true);
            minSize = new Vector2(750, 750);
        }

        private void SetupElements(ToolbarButton saveButton,
            ToolbarButton settingButton,
            ToolbarMenu optionMenu,
            IMGUIContainer contentIMGUI,
            VisualElement content,
            Toolbar toolbarLeft,
            Toolbar toolbarRight,
            IMGUIContainer toolbarLeftIMGUI,
            IMGUIContainer toolbarRightIMGUI)
        {
            saveButton.clicked += OnRequestSave;
            settingButton.clicked += OpenSettingWindow;

            optionMenu.menu.AppendAction("New", _ => InternalNew());
            optionMenu.menu.AppendAction("Load", _ => OnRequestLoad());
            optionMenu.menu.AppendSeparator();
            optionMenu.menu.AppendAction("Save",
                _ => InternalSave(),
                _ => string.IsNullOrEmpty(levelFolderPath) || string.IsNullOrEmpty(levelName)
                    ? DropdownMenuAction.Status.Disabled
                    : DropdownMenuAction.Status.Normal);
            optionMenu.menu.AppendAction("Save As...",
                _ => SaveAs(),
                _ => levelData != null
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);

            contentIMGUI.onGUIHandler += OnDrawIMGUIContent;
            toolbarLeftIMGUI.onGUIHandler += OnDrawIMGUIToolbarLeft;
            toolbarRightIMGUI.onGUIHandler += OnDrawIMGUIToolbarRight;

            toolbarLeft.Add(AppendLeftToolbar());
            toolbarRight.Add(AppendRightToolbar());
            content.Add(CreateContent());
        }

        private static void OpenSettingWindow() => LevelEditorSettingWindow.GetWindow(Settings);

        private void OnRequestSave()
        {
            if (levelData == null)
            {
                Debug.LogWarning("No level data to save!");
                return;
            }
            
            if (string.IsNullOrEmpty(levelFolderPath) || string.IsNullOrEmpty(levelName))
            {
                SaveAs();
            }
            else
            {
                InternalSave();
            }
        }

        private void OnRequestLoad()
        {
            if (Settings != null && Settings.levelReference != null)
            {
                PopupWindow.Show(new Rect(), new LevelSelectWindow(Settings.levelReference, InternalLoad));
            }
            else
            {
                InternalLoad(null);
            }
        }
        
        private void InternalSave()
        {
            Save(new LevelSaveDescription<TLevelData>((TLevelData)levelData, levelName, levelFolderPath));
            ChangeTitle(false);
        }

        private void InternalLoad(AssetReference levelReference)
        {
            var levelDescription = Load(levelReference);
            levelData = levelDescription.Data;
            levelFolderPath = Path.GetDirectoryName(levelDescription.FilePath);
            levelName = Path.GetFileNameWithoutExtension(levelDescription.FilePath);

            ChangeTitle(true);
        }

        private void InternalNew()
        {
            levelData = New();
            
            ChangeTitle(true);
        }

        private void SaveAs()
        {
            var wnd = SaveAsWindow.GetWindow();
            if (!string.IsNullOrEmpty(levelFolderPath) && !string.IsNullOrEmpty(levelName))
            {
                wnd.folderPath = new string(levelFolderPath);
                wnd.levelName = new string(levelName);
            }
            else
            {
                wnd.folderPath = Settings.defaultLevelFolder;
            }

            wnd.RefreshUI();

            wnd.SavePerformedEvent += () =>
            {
                levelFolderPath = new string(wnd.folderPath);
                levelName = new string(wnd.levelName);
                InternalSave();
            };
        }

        private void ChangeTitle(bool isDirty)
        {
            titleContent.text = isDirty switch
            {
                true when !titleContent.text.EndsWith("*")
                          && !titleContent.text.EndsWith(" ")
                    => windowName + "*",
                false when titleContent.text.EndsWith("*")
                           && !titleContent.text.EndsWith(" ")
                    => windowName + " ",
                _ => titleContent.text
            };
        }
    }
}