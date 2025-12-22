using System;
using System.IO;
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
        public static string EditorSaveFolder = "Assets/Saves";

        /// <summary>
        /// Represents the in-memory data loaded from a save file.
        /// This static member is updated during the load operation
        /// and contains the most recently loaded instance of the data type being managed by the SaveManager.
        /// The type of data is specified by the generic parameter <typeparamref name="T"/>.
        /// </summary>
        public static T SavedData;

        /// <summary>
        /// Indicates whether the type <typeparamref name="T"/> is valid for use with the SaveManager.
        /// The type is considered valid if it is marked with the <see cref="MessagePackObjectAttribute"/>.
        /// </summary>
        public static bool IsValidDataType =>
            Attribute.IsDefined(typeof(T), typeof(MessagePackObjectAttribute));
        
        static SaveManager()
        {
            if (!IsValidDataType)
            {
                throw new InvalidOperationException(
                    $"Type {typeof(T)} is not serializable by {nameof(SaveManager<T>)}."
                );
            }
        }

        /// <summary>
        /// Asynchronously saves the given data to the default slot.
        /// </summary>
        /// <param name="data">The data to be saved.</param>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A UniTask representing the asynchronous save operation.</returns>
        public static async Task SaveAsync(T data, CancellationToken token = default) =>
            await SaveAsync(DefaultSlotName, data, token);

        /// <summary>
        /// Asynchronously loads the saved data from the default slot.
        /// </summary>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A UniTask containing a boolean value indicating whether the load operation was successful.</returns>
        public static async Task<bool> LoadAsync(CancellationToken token = default) =>
            await LoadAsync(DefaultSlotName, token);

        /// <summary>
        /// Asynchronously saves the provided data to a specified save slot.
        /// </summary>
        /// <param name="slotName">The name of the save slot where the data will be stored.</param>
        /// <param name="data">The data to be saved.</param>
        /// <param name="token">A cancellation token that can be used to cancel the save operation.</param>
        /// <returns>A UniTask representing the asynchronous save operation.</returns>
        public static async Task SaveAsync(string slotName, T data, CancellationToken token = default)
        {
            slotName ??= DefaultSlotName;
            var serializer = SaveSerializerFactory.Create();
            var bytes = serializer.Serialize(data);
            var path = GetSaveFilePath(slotName, serializer.FileExtension);
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }


            await File.WriteAllBytesAsync(path, bytes, token);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// Asynchronously loads the saved data from the specified slot.
        /// </summary>
        /// <param name="slotName">The name of the slot from which to load the data.</param>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A UniTask containing a boolean value that indicates whether the data was successfully loaded.</returns>
        public static async Task<bool> LoadAsync(string slotName, CancellationToken token = default)
        {
            slotName ??= DefaultSlotName;
            var serializer = SaveSerializerFactory.Create();
            var path = GetSaveFilePath(slotName, serializer.FileExtension);

            if (!File.Exists(path)) return false;
            
            try
            {
                var bytes = await File.ReadAllBytesAsync(path, token);
                SavedData = serializer.Deserialize<T>(bytes);
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
            var typeString = Application.isEditor ? typeof(T).Name : typeof(T).FullName.GetHashCode().ToString("X");
            var folder = Application.isEditor ? EditorSaveFolder : Application.persistentDataPath;
            return Path.Combine(
                folder,
                $"save_{typeString}_{slotName}{extension}"
            );
        }
    }
}