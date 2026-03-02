using System.IO;
using UnityEditor;
namespace NgoUyenNguyen.Editor
{
    public static class EditorSettingsProvider
    {
        public const string DefaultSettingsFolderPath = "Assets/NgoUyenNguyen.EditorSettings.asset";

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SettingsProvider("Project/NgoUyenNguyen", SettingsScope.Project)
            {
                label = "Save Manager",
                
                
                guiHandler = _ =>
                {
                    EditorGUILayout.Space();
                    
                    SettingsPath();
                    EditorGUILayout.Space();
                },
            };
        }

        private static void SettingsPath()
        {
            var path = EditorGUILayout.TextField("Settings Path", DefaultSettingsFolderPath);

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                
            }
            else
            {
                
            }
        }
    }
}