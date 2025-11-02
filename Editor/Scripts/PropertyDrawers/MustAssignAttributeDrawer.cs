using NgoUyenNguyen;
using UnityEditor;
using UnityEngine;

namespace NgoUyenNguyen.Editor
{
    [CustomPropertyDrawer(typeof(MustAssignAttribute))]
    public class MustAssignAttributeDrawer : DecoratorDrawer
    {
        public override float GetHeight() => 20;

        public override void OnGUI(Rect position)
        {
            EditorGUI.HelpBox(position, "Must Assigned Property", MessageType.Info);
        }
    }
}
