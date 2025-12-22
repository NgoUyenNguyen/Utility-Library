using NgoUyenNguyen.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace NgoUyenNguyen.Editor
{
    public abstract partial class BaseLevelEditor : EditorWindow
    {
        private static Vector2 scrollPos;
        protected BaseLevel currentLevel;
        protected int currentLevelIndex;
        protected LevelReference levelReferences;
        
        protected abstract GameObject LevelTemplate {  get; set; }




        // [MenuItem("Window/Level Editor")]
        // public static void ShowWindow()
        // {
        //     GetWindow<BaseLevelEditor>("Level Editor");
        // }





        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // Object fields
            LevelFolderPath = EditorGUILayout.TextArea(LevelFolderPath, GUILayout.Height(20));
            GUILayout.Space(20);
            currentLevel = EditorGUILayout.ObjectField("Current Level: ", currentLevel, typeof(BaseLevel), true) as BaseLevel;
            currentLevelIndex = EditorGUILayout.IntField("Level Index", currentLevelIndex);
            GUILayout.Space(20);
            levelReferences = EditorGUILayout.ObjectField("Level Reference", levelReferences, typeof(LevelReference), false) as LevelReference;
            
            
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
