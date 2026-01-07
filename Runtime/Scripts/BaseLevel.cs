using UnityEngine;

namespace NgoUyenNguyen
{
    public abstract class BaseLevel : MonoBehaviour
    {
        [SerializeField, HideInInspector] private int index;
        [SerializeField, HideInInspector] private string editorPath;

        public int Index
        {
            get => index;
            set => index = value;
        }

        public string EditorPath
        {
            get => editorPath;
            set => editorPath = value;
        }
    }
}