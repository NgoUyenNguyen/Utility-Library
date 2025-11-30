using System.Collections.Generic;
using System.Linq;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace NgoUyenNguyen
{
    public readonly struct AsyncOperationHandleGroup
    {
        public readonly List<AsyncOperationHandle<SceneInstance>> Operations;

        public float Progress => Operations.Count == 0 ? 1 : Operations.Average(o => o.PercentComplete);
        public bool IsDone => Operations.Count == 0 || Operations.All(o => o.IsDone);

        public AsyncOperationHandleGroup(int initialCapacity)
        {
            Operations = new List<AsyncOperationHandle<SceneInstance>>(initialCapacity);
        }
    }
}