using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePack;

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
        #region Fields
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
        #endregion

        #region Events
        /// <summary>
        /// Invoked on the Unity main thread after a successful save operation.
        /// </summary>
        public static event Action<string> OnSaveCompleted = delegate { };
        /// <summary>
        /// Invoked on the Unity main thread after a fail save operation.
        /// </summary>
        public static event Action<string, Exception> OnSaveFailed = delegate { };
        /// <summary>
        /// Invoked on the Unity main thread after a successful load operation.
        /// </summary>
        public static event Action<string> OnLoadCompleted = delegate { };
        /// <summary>
        /// Invoked on the Unity main thread after a fail load operation.
        /// </summary>
        public static event Action<string, Exception> OnLoadFailed = delegate { };
        #endregion

        #region Exist
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
        #endregion

        #region Save
        /// <summary>
        /// Asynchronously saves the default data to the default slot.
        /// </summary>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task representing the asynchronous save operation.</returns>
        public static async UniTask<bool> SaveAsync(CancellationToken token = default) =>
            await SaveAsync(Cache.DefaultSlotName, Cache.DefaultSavedData, token);

        /// <summary>
        /// Asynchronously saves the specified data to the specified slot.
        /// </summary>
        /// <param name="slotName">The name of the slot where the data will be saved.</param>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task representing the asynchronous save operation.</returns>
        public static async UniTask<bool> SaveAsync(string slotName, CancellationToken token = default)
        {
            if (!Cache.TryGetAtSlot(slotName, out var data))
            {
                await UniTask.SwitchToMainThread();
                OnSaveFailed(slotName, new SaveSlotNotFoundException(slotName));
                return false;
            }
            
            var result = await SaveAsync(slotName, data, token);
            return result;
        }
        
        /// <summary>
        /// Asynchronously saves the given data to the default slot.
        /// </summary>
        /// <param name="data">The data to be saved.</param>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task representing the asynchronous save operation.</returns>
        public static async UniTask<bool> SaveAsync(T data, CancellationToken token = default) =>
            await SaveAsync(Cache.DefaultSlotName, data, token);

        /// <summary>
        /// Asynchronously saves the provided data to a specified save slot.
        /// </summary>
        /// <param name="slotName">The name of the save slot where the data will be stored.</param>
        /// <param name="data">The data to be saved.</param>
        /// <param name="token">A cancellation token that can be used to cancel the save operation.</param>
        /// <returns>A Task representing the asynchronous save operation.</returns>
        public static async UniTask<bool> SaveAsync(string slotName, T data, CancellationToken token = default)
        {
            EnsureValidType();
            if (data is null) return false;
            
            slotName ??= Cache.DefaultSlotName;
            
            var path = PathResolver.GetSaveFilePath(slotName, serializer.FileExtension);
            var tempPath = path + ".tmp";
            
            try
            {
                await UniTask.RunOnThreadPool(() =>
                {
                    token.ThrowIfCancellationRequested();
                    
                    var bytes = serializer.Serialize(data);
                    
                    var dir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                    
                    File.WriteAllBytes(tempPath, bytes);
                    
                    if (File.Exists(path)) File.Delete(path);
                    
                    File.Move(tempPath, path);
                }, cancellationToken: token);

                await UniTask.SwitchToMainThread();
                OnSaveCompleted(slotName);
                
#if UNITY_EDITOR
                if (path.StartsWith("Assets")) AssetDatabase.ImportAsset(path);
#endif
                return true;
            }
            catch (OperationCanceledException)
            {
                CleanupTemp(tempPath);
                throw;
            }
            catch (Exception e)
            {
                CleanupTemp(tempPath);

                await UniTask.SwitchToMainThread();
                OnSaveFailed(slotName, e);
                return false;
            }
        }
        
        private static void CleanupTemp(string tempPath)
        {
            try
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
            catch { /* ignored */ }
        }
        #endregion

        #region Load
        /// <summary>
        /// Asynchronously loads the saved data from the default slot.
        /// </summary>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task containing a boolean value indicating whether the load operation was successful.</returns>
        public static async UniTask<bool> LoadAsync(CancellationToken token = default) =>
            await LoadAsync(Cache.DefaultSlotName, token);

        /// <summary>
        /// Asynchronously loads the saved data from the specified slot.
        /// </summary>
        /// <param name="slotName">The name of the slot from which to load the data.</param>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task containing a boolean value that indicates whether the data was successfully loaded.</returns>
        public static async UniTask<bool> LoadAsync(string slotName, CancellationToken token = default)
        {
            EnsureValidType();
            slotName ??= Cache.DefaultSlotName;
            var path = PathResolver.GetSaveFilePath(slotName, serializer.FileExtension);

            if (!File.Exists(path))
            {
                await UniTask.SwitchToMainThread();
                OnLoadFailed(slotName, new Exception("File not found"));
                return false;
            }

            try
            {
                var bytes = await UniTask.RunOnThreadPool(
                    () =>
                    {
                        token.ThrowIfCancellationRequested();
                        return File.ReadAllBytes(path);
                    },
                    cancellationToken: token
                );
                var data = serializer.Deserialize<T>(bytes);
                
                await UniTask.SwitchToMainThread();
                
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
                await UniTask.SwitchToMainThread();
                OnLoadFailed(slotName, e);
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
        #endregion
        
        public static void ClearEvents()
        {
            OnSaveCompleted = delegate { };
            OnSaveFailed = delegate { };
            OnLoadCompleted = delegate { };
            OnLoadFailed = delegate { };
        }
    }
}