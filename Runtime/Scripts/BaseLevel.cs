using UnityEngine;

namespace NgoUyenNguyen
{
    public abstract class BaseLevel : MonoBehaviour
    {
        private int _index;

        public int index { get => _index; set => _index = value; }
    }
}
