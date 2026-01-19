using UnityEngine;

namespace NgoUyenNguyen.Editor
{
    [CreateAssetMenu(fileName = "LevelEditorSettings", menuName = "Scriptable Objects/Default Level Editor Settings")]
    public class DefaultLevelEditorSettings : LevelEditorSettings
    {
        public GameObject levelTemplate;
    }
}