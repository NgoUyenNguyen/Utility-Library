using NgoUyenNguyen.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace NgoUyenNguyen.Editor
{
    public abstract partial class BaseLevelEditor : EditorWindow
    {
        private static Vector2 scrollPos;
        protected BaseLevel currentLevel;
        protected LevelReference levelReference;
        protected abstract GameObject levelTemplate {  get; }




        //[MenuItem("Window/Level Editor")]
        //public static void ShowWindow()
        //{
        //    GetWindow<BaseLevelEditor>("Level Editor");
        //}





        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // Object fields
            GUILayout.Space(10);
            currentLevel = EditorGUILayout.ObjectField($"Current Level: {currentLevel?.index}", currentLevel, typeof(BaseLevel), true) as BaseLevel;
            GUILayout.Space(10);
            levelReference = EditorGUILayout.ObjectField("Level Reference", levelReference, typeof(LevelReference), false) as LevelReference;
            GUILayout.Space(20);
            OnDrawGUI();


            EditorGUILayout.EndScrollView();

            // Save-Remove-Load-Add(SRLA) Buttons
            SaveRemoveLoadAddButtons();
        }

        /// <summary>
        /// Implement your own editor GUI here
        /// </summary>
        protected abstract void OnDrawGUI();
    }
}
