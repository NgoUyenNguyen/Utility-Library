using UnityEditor;
using UnityEditor.UI;

namespace NgoUyenNguyen.Editor
{
    [CustomEditor(typeof(TabButton)), CanEditMultipleObjects]
    public class TabButtonEditor : ImageEditor
    {
        private bool showEvents;
        private bool showInteractOptions;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.Space(20, true);
            
            EditorGUILayout.HelpBox("TabButton only works when belongs to a TabGroup.", MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tabGroup"));
            
            showInteractOptions = EditorGUILayout.BeginFoldoutHeaderGroup(showInteractOptions, "Interact Options");
            if (showInteractOptions)
            {
                var interactTypeProp = serializedObject.FindProperty("interactType");
                EditorGUILayout.PropertyField(interactTypeProp);
                switch (interactTypeProp.enumValueIndex)
                {
                    case (int)TabButton.InteractType.ChangeColor:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedColor"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("hoveredColor"));
                        break;
                    case (int)TabButton.InteractType.ChangeSprite:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedSprite"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("hoveredSprite"));
                        break;
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(showEvents, "Events");
            if (showEvents)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onSelected"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onDeselected"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onHovered"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onUnhovered"));
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
