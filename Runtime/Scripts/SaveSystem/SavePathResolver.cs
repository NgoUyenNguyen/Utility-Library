using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NgoUyenNguyen
{
    public class SavePathResolver<T>
    {
        public string GetSaveFilePath(string slotName, string extension)
        {
            slotName = string.Concat(slotName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
#if UNITY_EDITOR
            var fileNameOption = (FileNameOption)EditorPrefs.GetInt("SaveFileNameOption", (int)FileNameOption.ClassName);
            var typeString = fileNameOption switch
            {
                FileNameOption.ClassName => typeof(T).Name,
                FileNameOption.HashCode => StableHash(typeof(T).AssemblyQualifiedName ?? typeof(T).Name),
                _ => throw new ArgumentOutOfRangeException()
            };
            var folder = EditorPrefs.GetString("SaveFolderPath", "Assets/Editor/Saves");;
#else
            var typeString = StableHash(typeof(T).AssemblyQualifiedName ?? typeof(T).Name);
            var folder = Application.persistentDataPath;
#endif
            return Path.Combine(
                folder,
                $"save_{typeString}_{slotName}{extension}"
            );
        }

        private static string StableHash(string input)
        {
            using var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "");
        }
    }
    
    public enum FileNameOption
    {
        ClassName,
        HashCode
    }
}