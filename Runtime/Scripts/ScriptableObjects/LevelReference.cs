using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NgoUyenNguyen.ScriptableObjects
{
    [CreateAssetMenu(fileName ="LevelReference", menuName = "Scriptable Objects/Level Reference")]
    public class LevelReference : ScriptableObject
    {
        public List<AssetReference> references;

        public AssetReference this[int index]
        {
            get => references[index];
        }

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
    }
}
