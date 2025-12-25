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
        private static readonly ISaveSerializer serializer = SaveSerializerFactory.Create();
        
        /// <summary>
        /// Cache for storing saved data of type <typeparamref name="T"/>.
        /// </summary>
        public static readonly SaveCache<T> Cache = new();
        
        /// <summary>
        /// Resolver for determining the file paths for saving and loading data of type <typeparamref name="T"/>.
        /// </summary>
        public static readonly SavePathResolver<T> PathResolver = new();
        
        /// <summary>
        /// Indicates whether the type <typeparamref name="T"/> is valid for use with the SaveManager.
        /// The type is considered valid if it is marked with the <see cref="MessagePackObjectAttribute"/>.
        /// </summary>
        public static readonly bool IsValidDataType = Attribute.IsDefined(typeof(T), typeof(MessagePackObjectAttribute));
        
        public static event Action<string> OnSaveCompleted = delegate { };
        public static event Action<string> OnSaveFailed = delegate { };
        public static event Action<string> OnLoadCompleted = delegate { };
        public static event Action<string> OnLoadFailed = delegate { };

        
        /// <summary>
        /// Checks if the default save file exists.
        /// </summary>
        /// <returns>A boolean indicating whether the default save file exists.</returns>
        public static bool ExistsSaveFile() => ExistsSaveFile(Cache.DefaultSlotName);

        /// <summary>
        /// Checks if the save file exists.
        /// </summary>
        /// <returns>A boolean indicating whether the save file exists.</returns>
        public static bool ExistsSaveFile(string slotName) =>
            File.Exists(PathResolver.GetSaveFilePath(slotName, serializer.FileExtension));

        /// <summary>
        /// Asynchronously saves the default data to the default slot.
        /// </summary>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task representing the asynchronous save operation.</returns>
        public static async Task SaveAsync(CancellationToken token = default) =>
            await SaveAsync(Cache.DefaultSlotName, Cache.DefaultSavedData, token);

        /// <summary>
        /// Asynchronously saves the specified data to the specified slot.
        /// </summary>
        /// <param name="slotName">The name of the slot where the data will be saved.</param>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task representing the asynchronous save operation.</returns>
        public static async Task SaveAsync(string slotName, CancellationToken token = default)
        {
            if (!Cache.TryGetAtSlot(slotName, out var data))
                throw new InvalidOperationException($"No cached data at slot '{slotName}'");

            await SaveAsync(slotName, data, token);
        }


        /// <summary>
        /// Asynchronously saves the given data to the default slot.
        /// </summary>
        /// <param name="data">The data to be saved.</param>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task representing the asynchronous save operation.</returns>
        public static async Task SaveAsync(T data, CancellationToken token = default) =>
            await SaveAsync(Cache.DefaultSlotName, data, token);

        /// <summary>
        /// Asynchronously loads the saved data from the default slot.
        /// </summary>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task containing a boolean value indicating whether the load operation was successful.</returns>
        public static async Task<bool> LoadAsync(CancellationToken token = default) =>
            await LoadAsync(Cache.DefaultSlotName, token);

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
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            slotName ??= Cache.DefaultSlotName;
            var bytes = serializer.Serialize(data);
            var path = PathResolver.GetSaveFilePath(slotName, serializer.FileExtension);
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempPath = path + ".tmp";
            try
            {
                await Task.Run(() =>
                {
                    File.WriteAllBytes(tempPath, bytes);
                    
                    if (File.Exists(path)) File.Delete(path);
                    
                    File.Move(tempPath, path);
                }, token);

                OnSaveCompleted(slotName);
            }
            catch
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
                OnSaveFailed(slotName);
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
            slotName ??= Cache.DefaultSlotName;
            var path = PathResolver.GetSaveFilePath(slotName, serializer.FileExtension);

            if (!File.Exists(path))
            {
                OnLoadFailed(slotName);
                return false;
            }

            try
            {
                var bytes = await File.ReadAllBytesAsync(path, token);
                var data = serializer.Deserialize<T>(bytes);
                Cache.SetAtSlot(slotName, data);

                OnLoadCompleted(slotName);
                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load save '{slotName}': {e}");
                OnLoadFailed(slotName);
                return false;
            }
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