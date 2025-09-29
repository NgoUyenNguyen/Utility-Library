using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace NgoUyenNguyen.GridSystem
{
    public partial class BaseGrid
    {
        private struct MonoPath
        {
            public bool IsGoal;
            public int2 Index;
            public int2 ParentIndex;
            public bool Walkable;
            public int G;
            public int H;
            public int F => G + H;
        }

        private NativeArray<MonoPath> Bake()
        {
            throw new System.NotImplementedException();
        }
    }
}
