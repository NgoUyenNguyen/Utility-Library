using NgoUyenNguyen.ScriptableObjects;
using UnityEngine;

namespace NgoUyenNguyen.Editor
{
    public class LevelEditorSettings : ScriptableObject
    {
        public LevelReference levelReference;
        public string defaultLevelFolder = "Assets/Levels";
    }
}