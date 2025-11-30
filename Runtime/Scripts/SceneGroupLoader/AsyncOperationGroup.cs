using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NgoUyenNguyen
{
    public readonly struct AsyncOperationGroup
    {
        public readonly List<AsyncOperation> Operations;

        public float Progress =>
            Operations.Count == 0
                ? 1
                : Mathf.Clamp01(Operations.Average(o => Remap(o.progress, 0, .9f, 0, 1)));

        public bool IsDone => Operations.Count == 0 || Operations.All(o => o.progress >= 0.9f);

        public AsyncOperationGroup(int initialCapacity)
        {
            Operations = new List<AsyncOperation>(initialCapacity);
        }

        public static float Remap(float value, float fromIn, float toIn, float fromOut, float toOut)
            => Mathf.Lerp(fromOut, toOut, Mathf.InverseLerp(fromIn, toIn, value));
    }
}