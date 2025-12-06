using UnityEngine;

namespace NgoUyenNguyen
{
    public abstract class BaseLevel : MonoBehaviour
    {
        [SerializeField, HideInInspector] private int index;

        public int Index { get => index; set => index = value; }
    }
}
