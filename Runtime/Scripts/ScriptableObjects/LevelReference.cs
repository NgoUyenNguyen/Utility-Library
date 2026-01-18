using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace NgoUyenNguyen.ScriptableObjects
{
    [CreateAssetMenu(fileName = "LevelReference", menuName = "Scriptable Objects/Level Reference")]
    public class LevelReference : ScriptableObject, IDisposable, IEnumerable<AssetReference>
    {
        [SerializeField] private List<AssetReference> references = new();

        private readonly Dictionary<int, AsyncOperationHandle> loadedHandles = new();

        public int Count => references.Count;
        public List<AssetReference> References => references;
        
        public bool Remove(AssetReference reference) => references.Remove(reference);
        public void RemoveAt(int index) => references.RemoveAt(index);
        
        public void Add(AssetReference reference) => references.Add(reference);

        public AssetReference this[int index] => index >= 0 && index < references.Count
                ? references[index]
                : null;

        #region IEnumerable

        public IEnumerator<AssetReference> GetEnumerator() => references.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        public AssetReference GetReferenceFromGUID(string guid) =>
            references.FirstOrDefault(r => r != null && r.AssetGUID == guid);

        public async UniTask<T> LoadAsync<T>(int index, 
            float delaySeconds = 0f, 
            CancellationToken cancellationToken = default) where T : Object
        {
            if (index < 0 || index >= references.Count) return null;

            var reference = references[index];
            if (reference == null) return null;

            if (loadedHandles.TryGetValue(index, out var cached)) return cached.Result as T;

            var handle = reference.LoadAssetAsync<T>();
            loadedHandles[index] = handle;

            try
            {
                await handle.ToUniTask(cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);

                loadedHandles.Remove(index);
                return null;
            }


            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogException(handle.OperationException);
                loadedHandles.Remove(index);
                return null;
            }

            if (delaySeconds > 0f) 
                await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken: cancellationToken);

            return handle.Result;
        }

        public async UniTask<T> InstantiateAsync<T>(int index, 
            float delaySeconds = 0f,
            CancellationToken cancellationToken = default) where T : Object
        {
            var instance = await LoadAsync<T>(index, delaySeconds, cancellationToken);
            return Instantiate(instance);
        }

        public async UniTask<T> InstantiateAsync<T>(int index, 
            Scene scene,
            float delaySeconds = 0f,
            CancellationToken cancellationToken = default) where T : Object
        {
            var instance = await LoadAsync<T>(index, delaySeconds, cancellationToken);
            return Instantiate(instance, scene) as T;
        }

        public async UniTask<T> InstantiateAsync<T>(int index, 
            Transform parent,
            bool worldPositionStays,
            float delaySeconds = 0f,
            CancellationToken cancellationToken = default) where T : Object
        {
            var instance = await LoadAsync<T>(index, delaySeconds, cancellationToken);
            return Instantiate(instance, parent, worldPositionStays);
        }
        
        public async UniTask<T> InstantiateAsync<T>(int index, 
            Vector3 position, 
            Quaternion rotation,
            float delaySeconds = 0f,
            CancellationToken cancellationToken = default) where T : Object
        {
            var instance = await LoadAsync<T>(index, delaySeconds, cancellationToken);
            return Instantiate(instance, position, rotation);
        }
        
        public async UniTask<T> InstantiateAsync<T>(int index, 
            Vector3 position, 
            Quaternion rotation,
            Transform parent, 
            float delaySeconds = 0f,
            CancellationToken cancellationToken = default) where T : Object
        {
            var instance = await LoadAsync<T>(index, delaySeconds, cancellationToken);
            return Instantiate(instance, position, rotation, parent);
        }

        public async UniTask<T> InstantiateAsync<T>(int index,
            InstantiateParameters parameters,
            float delaySeconds = 0f,
            CancellationToken cancellationToken = default) where T : Object
        {
            var instance = await LoadAsync<T>(index, delaySeconds, cancellationToken);
            return Instantiate(instance, parameters);
        }
        
        public async UniTask<T> InstantiateAsync<T>(int index, 
            Vector3 position, 
            Quaternion rotation,
            InstantiateParameters parameters,
            float delaySeconds = 0f,
            CancellationToken cancellationToken = default) where T : Object
        {
            var instance = await LoadAsync<T>(index, delaySeconds, cancellationToken);
            return Instantiate(instance, position, rotation, parameters);
        }

        public void Release(int index)
        {
            if (!loadedHandles.TryGetValue(index, out var handle))
                return;

            if (handle.IsValid())
                Addressables.Release(handle);

            loadedHandles.Remove(index);
        }

        public void ReleaseAll()
        {
            foreach (var handle in loadedHandles.Values)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }

            loadedHandles.Clear();
        }

        public void Dispose() => ReleaseAll();
    }
}