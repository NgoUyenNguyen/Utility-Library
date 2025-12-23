using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NgoUyenNguyen
{
    /// <summary>
    /// The SaveManager class provides static methods for saving and loading data of a specified type T.
    /// </summary>
    /// <typeparam name="T">The data type to be saved and loaded. It must be serializable and decorated with MessagePackObjectAttribute.</typeparam>
    public static class SaveManager<T>
    {
        /// <summary>
        /// The default slot name used for saving and loading operations within the SaveManager.
        /// This value is set to "default" and acts as the primary identifier for the save slot
        /// when no other specific slot name is provided.
        /// </summary>
        public static string DefaultSlotName = "default";

        /// <summary>
        /// The folder path used for saving data in the Unity Editor environment.
        /// This is set to "Assets/Saves" by default and serves as the location for serialized data files
        /// when running the application within the Unity Editor.
        /// </summary>
        public static string EditorSaveFolder = "Assets/Editor/Saves";

        /// <summary>
        /// A dictionary that stores the saved data instances grouped by their corresponding slot names.
        /// This serves as the runtime in-memory representation of the saved data loaded from storage or added via the SaveManager.
        /// </summary>
        public static readonly Dictionary<string, T> SavedData = new();

        /// <summary>
        /// Represents the default saved data of type T, associated with the default save slot name.
        /// This property serves as a convenient way to get or set the data stored in the default save slot.
        /// If no data exists in the default slot, the getter will return the default value of T.
        /// When setting this property, the corresponding data in the default save slot is updated.
        /// </summary>
        public static T DefaultSavedData
        {
            get => SavedData.GetValueOrDefault(DefaultSlotName);
            set => SavedData[DefaultSlotName] = value;
        }

        /// <summary>
        /// Checks if the default save file exists.
        /// </summary>
        /// <returns>A boolean indicating whether the default save file exists.</returns>
        public static bool ExistSaveFile() => ExistSaveFile(DefaultSlotName);

        /// <summary>
        /// Checks if the save file exists.
        /// </summary>
        /// <returns>A boolean indicating whether the save file exists.</returns>
        public static bool ExistSaveFile(string slotName) =>
            File.Exists(GetSaveFilePath(slotName, SaveSerializerFactory.Create().FileExtension));

        /// <summary>
        /// Indicates whether the type <typeparamref name="T"/> is valid for use with the SaveManager.
        /// The type is considered valid if it is marked with the <see cref="MessagePackObjectAttribute"/>.
        /// </summary>
        public static bool IsValidDataType =>
            Attribute.IsDefined(typeof(T), typeof(MessagePackObjectAttribute));

        /// <summary>
        /// Asynchronously saves the default data to the default slot.
        /// </summary>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task representing the asynchronous save operation.</returns>
        public static async Task SaveAsync(CancellationToken token = default) =>
            await SaveAsync(DefaultSlotName, DefaultSavedData, token);

        /// <summary>
        /// Asynchronously saves the specified data to the specified slot.
        /// </summary>
        /// <param name="slotName">The name of the slot where the data will be saved.</param>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task representing the asynchronous save operation.</returns>
        public static async Task SaveAsync(string slotName, CancellationToken token = default) =>
            await SaveAsync(slotName, SavedData[slotName], token);

        /// <summary>
        /// Asynchronously saves the given data to the default slot.
        /// </summary>
        /// <param name="data">The data to be saved.</param>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task representing the asynchronous save operation.</returns>
        public static async Task SaveAsync(T data, CancellationToken token = default) =>
            await SaveAsync(DefaultSlotName, data, token);

        /// <summary>
        /// Asynchronously loads the saved data from the default slot.
        /// </summary>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task containing a boolean value indicating whether the load operation was successful.</returns>
        public static async Task<bool> LoadAsync(CancellationToken token = default) =>
            await LoadAsync(DefaultSlotName, token);

        /// <summary>
        /// Asynchronously saves the provided data to a specified save slot.
        /// </summary>
        /// <param name="slotName">The name of the save slot where the data will be stored.</param>
        /// <param name="data">The data to be saved.</param>
        /// <param name="token">A cancellation token that can be used to cancel the save operation.</param>
        /// <returns>A Task representing the asynchronous save operation.</returns>
        public static async Task SaveAsync(string slotName, T data, CancellationToken token = default)
        {
            EnsureValidType();
            slotName ??= DefaultSlotName;
            var serializer = SaveSerializerFactory.Create();
            var bytes = serializer.Serialize(data);
            var path = GetSaveFilePath(slotName, serializer.FileExtension);
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempPath = path + ".tmp";
            try
            {
                await File.WriteAllBytesAsync(tempPath, bytes, token);
                if (File.Exists(path)) File.Delete(path);

                File.Move(tempPath, path);
            }
            catch
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
                throw;
            }

#if UNITY_EDITOR
            if (!path.StartsWith("Assets")) return;
            AssetDatabase.ImportAsset(path);
#endif
        }

        /// <summary>
        /// Asynchronously loads the saved data from the specified slot.
        /// </summary>
        /// <param name="slotName">The name of the slot from which to load the data.</param>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task containing a boolean value that indicates whether the data was successfully loaded.</returns>
        public static async Task<bool> LoadAsync(string slotName, CancellationToken token = default)
        {
            EnsureValidType();
            slotName ??= DefaultSlotName;
            var serializer = SaveSerializerFactory.Create();
            var path = GetSaveFilePath(slotName, serializer.FileExtension);

            if (!File.Exists(path)) return false;

            try
            {
                var bytes = await File.ReadAllBytesAsync(path, token);
                var data = serializer.Deserialize<T>(bytes);
                lock (SavedData)
                {
                    SavedData[slotName] = data;
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load save '{slotName}': {e}");
                return false;
            }
        }

        /// <summary>
        /// Constructs and returns the full file path for a save file, based on the specified slot name and file extension.
        /// </summary>
        /// <param name="slotName">The name of the save slot to uniquely identify the save file.</param>
        /// <param name="extension">The file extension to be used for the save file.</param>
        /// <returns>The full file path for the specified save file.</returns>
        public static string GetSaveFilePath(string slotName, string extension)
        {
            slotName = string.Concat(slotName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
            var typeString = Application.isEditor ? typeof(T).Name : StableHash(typeof(T).FullName);
            var folder = Application.isEditor ? EditorSaveFolder : Application.persistentDataPath;
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

        private static void EnsureValidType()
        {
            if (!IsValidDataType)
                throw new InvalidOperationException(
                    $"Type {typeof(T)} must be marked with [MessagePackObject]."
                );
        }
    }
}