using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace NgoUyenNguyen.ScriptableObjects
{
    [CreateAssetMenu(fileName ="LevelReference", menuName = "Scriptable Objects/Level Reference")]
    public class LevelReference : ScriptableObject, IDisposable
    {
        public List<AssetReference> references;

        public AssetReference this[int index]
        {
            get => references[index];
        }
        
        public int Count => references.Count;

        public AssetReference GetReferenceFromGUID(string guid)
        {
            foreach (AssetReference reference in references)
            {
                if (reference.AssetGUID == guid)
                {
                    return reference;
                }
            }

            return null;
        }
        
        public async Task<T> LoadAsync<T>(int index, float delay = 0f, Action onComplete = null) where T : Object
        {
            if (index < 0 || index >= references.Count || references[index] == null) return null;
            
            var loadTime = 0f;
            var loadHandle = references[index].LoadAssetAsync<T>();
            while (!loadHandle.IsDone || loadTime < delay)
            {
                loadTime += .1f;
                await Task.Delay(100);
            }

            if (loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                onComplete?.Invoke();
                return loadHandle.Result;
            }
            Debug.LogException(loadHandle.OperationException);
            return null;
        }
        
        public async Task<GameObject> InstantiateAsync(int index, Action onComplete = null)
        {
            if (index < 0 || index >= references.Count || references[index] == null) return null;

            var instantiateHandle = references[index].InstantiateAsync();
            while (!instantiateHandle.IsDone)
            {
                await Task.Delay(100);
            }
            
            if (instantiateHandle.Status == AsyncOperationStatus.Succeeded)
            {
                onComplete?.Invoke();
                return instantiateHandle.Result;
            }
            Debug.LogException(instantiateHandle.OperationException);
            return null;
        }

        public async Task<GameObject> InstantiateAsync(int index, Vector3 position, Quaternion rotation, 
            Transform parent = null, Action onComplete = null)
        {
            if (index < 0 || index >= references.Count || references[index] == null) return null;

            var instantiateHandle = references[index].InstantiateAsync(position, rotation, parent);
            while (!instantiateHandle.IsDone)
            {
                await Task.Delay(100);
            }
            
            if (instantiateHandle.Status == AsyncOperationStatus.Succeeded)
            {
                onComplete?.Invoke();
                return instantiateHandle.Result;
            }
            Debug.LogException(instantiateHandle.OperationException);
            return null;
        }

        public async Task<GameObject> InstantiateAsync(int index, Transform parent, 
            bool instantiateInWorldSpace = false, Action onComplete = null)
        {
            if (index < 0 || index >= references.Count || references[index] == null) return null;

            var instantiateHandle = references[index].InstantiateAsync(parent, instantiateInWorldSpace);
            while (!instantiateHandle.IsDone)
            {
                await Task.Delay(100);
            }
            
            if (instantiateHandle.Status == AsyncOperationStatus.Succeeded)
            {
                onComplete?.Invoke();
                return instantiateHandle.Result;
            }
            Debug.LogException(instantiateHandle.OperationException);
            return null;
        }

        public void Dispose()
        {
            foreach (var reference in references)
            {
                reference.ReleaseAsset();
            }
        }
    }
}
