using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace NgoUyenNguyen
{
    public class SavePathResolver<T>
    {
        /// <summary>
        /// The folder path used for saving data in the Unity Editor environment.
        /// This is set to "Assets/Saves" by default and serves as the location for serialized data files
        /// when running the application within the Unity Editor.
        /// </summary>
        public string EditorSaveFolder = "Assets/Editor/Saves";

        public string GetSaveFilePath(string slotName, string extension)
        {
            slotName = string.Concat(slotName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
#if UNITY_EDITOR
            var typeString = typeof(T).Name;
            var folder = EditorSaveFolder;
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
}