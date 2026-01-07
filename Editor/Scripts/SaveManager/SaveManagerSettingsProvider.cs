using UnityEditor;

namespace NgoUyenNguyen.Editor
{
    public static class SaveManagerSettingsProvider
    {
        private const string HelpBoxContent = "Change behaviours of SaveManager in Editor.\n" +
                                              "These settings are valid in Editor only, " +
                                              "in Build using permanent settings:\n" +
                                              "   - MessagePack for serrializing\n" +
                                              "   - HashCode for filename\n" +
                                              "   - Application.persistentDataPath for save folder path\n";
        
        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SettingsProvider("Project/NgoUyenNguyen/Save Manager", SettingsScope.Project)
            {
                label = "Save Manager",
                
                
                guiHandler = _ =>
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(HelpBoxContent, MessageType.Info);
                    EditorGUILayout.Space();
                    
                    SaveFolderPathSetting();
                    EditorGUILayout.Space();
                    
                    SerializerSetting();
                    EditorGUILayout.Space();
                    
                    FileNameSetting();
                    EditorGUILayout.Space();
                }
            };
        }

        private static void SaveFolderPathSetting()
        {
            var path = EditorPrefs.GetString("SaveFolderPath", "Assets/Editor/Saves");
            var newPath = EditorGUILayout.TextField("Save Folder Path", path);

            if (newPath == path) return;
            path = newPath;
            EditorPrefs.SetString("SaveFolderPath", path);
        }

        private static void SerializerSetting()
        {
            var option = EditorPrefs.GetInt("SaveSerializerOption", (int)SerializerOption.Json);
            var newOption = (SerializerOption)EditorGUILayout.EnumPopup("Serializer Option", (SerializerOption)option);

            if (option == (int)newOption) return;
            option = (int)newOption;
            EditorPrefs.SetInt("SaveSerializerOption", option);
        }

        private static void FileNameSetting()
        {
            var option = EditorPrefs.GetInt("SaveFileNameOption", (int)FileNameOption.ClassName);
            var newOption = (FileNameOption)EditorGUILayout.EnumPopup("FileName Option", (FileNameOption)option);
            
            if (option == (int)newOption) return;
            option = (int)newOption;
            EditorPrefs.SetInt("SaveFileNameOption", option);
        }
    }
}
