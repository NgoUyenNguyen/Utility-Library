using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NgoUyenNguyen.Editor
{
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    public class RequireInterfaceDrawer : PropertyDrawer
    {
        private RequireInterfaceAttribute RequireInterfaceAttribute => (RequireInterfaceAttribute)attribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type requiredInterfaceType = RequireInterfaceAttribute.InterfaceType;

            EditorGUI.BeginProperty(position, label, property);
            DrawInterfaceObjectField(position, property, label, requiredInterfaceType);
            EditorGUI.EndProperty();
            
            var args = new InterfaceArgs(GetTypeOrElementType(fieldInfo.FieldType), requiredInterfaceType);
            InterfaceReferenceUtil.OnGUI(position, property, label, args);
        }

        private void DrawInterfaceObjectField(
            Rect position,
            SerializedProperty property,
            GUIContent label,
            Type interfaceType)
        {
            var oldReference = property.objectReferenceValue;
            var newReference = EditorGUI.ObjectField(position, label, oldReference, typeof(Object), true);

            if (newReference == null)
            {
                property.objectReferenceValue = null;
                return;
            }

            if (newReference != oldReference)
            {
                ValidateAndAssignObject(property, newReference, interfaceType);
            }
        }

        private void ValidateAndAssignObject(SerializedProperty property, Object newReference, Type interfaceType)
        {
            if (newReference is GameObject gameObject)
            {
                var component = gameObject.GetComponent(interfaceType);
                if (component != null)
                {
                    property.objectReferenceValue = component;
                }
                else
                {
                    Debug.LogWarning(
                        $"Game Object '{gameObject.name}' does not have Component implement '{interfaceType.Name}'.");
                }
            }
            else if (interfaceType.IsAssignableFrom(newReference.GetType()))
            {
                property.objectReferenceValue = newReference;
            }
            else
            {
                Debug.LogWarning($"The assigned object does not implement '{interfaceType.Name}'.");
            }
        }

        private Type GetTypeOrElementType(Type type)
        {
            if (type.IsArray) return type.GetElementType();
            if (type.IsGenericType) return type.GetGenericArguments()[0];
            return type;
        }
    }
}