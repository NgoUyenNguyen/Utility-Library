using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NgoUyenNguyen.Editor
{
    [CustomPropertyDrawer(typeof(InterfaceReference<,>))]
    [CustomPropertyDrawer(typeof(InterfaceReference<>))]
    public class InterfaceReferenceDrawer : PropertyDrawer
    {
        private const string UnderlyingValueFieldName = "underlyingValue";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var underlyingValueProperty = property.FindPropertyRelative(UnderlyingValueFieldName);
            var args = GetArguments(fieldInfo);

            EditorGUI.BeginProperty(position, label, property);
            var assignedObject =
                EditorGUI.ObjectField(position, label, underlyingValueProperty.objectReferenceValue, args.ObjectType,
                    true);

            if (assignedObject != null)
            {
                if (assignedObject is GameObject gameObject)
                {
                    ValidateAndAssignObject(underlyingValueProperty, gameObject.GetComponent(args.InterfaceType), args,
                        gameObject.name);
                }
                else
                {
                    ValidateAndAssignObject(underlyingValueProperty, assignedObject, args);
                }
            }
            else
            {
                underlyingValueProperty.objectReferenceValue = null;
            }

            EditorGUI.EndProperty();
            InterfaceReferenceUtil.OnGUI(position, underlyingValueProperty, label, args);
        }

        private InterfaceArgs GetArguments(FieldInfo fieldInfo)
        {
            Type objectType = null, interfaceType = null;
            Type fieldType = fieldInfo.FieldType;

            if (!TryGetTypesFromInterfaceReference(fieldType, out objectType, out interfaceType))
            {
                GetTypesFromList(fieldType, out objectType, out interfaceType);
            }

            return new InterfaceArgs(objectType, interfaceType);


            void GetTypesFromList(Type type, out Type objType, out Type infType)
            {
                objType = infType = null;

                if (type.IsArray)
                {
                    var elementType = type.GetElementType();
                    TryGetTypesFromInterfaceReference(elementType, out objType, out infType);
                    return;
                }

                var listInterface = type.GetInterfaces()
                    .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));

                if (listInterface != null)
                {
                    var elementType = listInterface.GetGenericArguments()[0];
                    TryGetTypesFromInterfaceReference(elementType, out objType, out infType);
                }
            }

            bool TryGetTypesFromInterfaceReference(Type type, out Type objType, out Type intfType)
            {
                objType = intfType = null;

                if (type?.IsGenericType != true) return false;

                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(InterfaceReference<>)) type = type.BaseType;

                if (type?.GetGenericTypeDefinition() == typeof(InterfaceReference<,>))
                {
                    var types = type.GetGenericArguments();
                    intfType = types[0];
                    objType = types[1];
                    return true;
                }

                return false;
            }
        }

        private void ValidateAndAssignObject(
            SerializedProperty property,
            Object targetObject,
            InterfaceArgs args,
            string gameObjectName = null)
        {
            if (targetObject != null)
            {
                if (!args.InterfaceType.IsAssignableFrom(targetObject.GetType()))
                {
                    Debug.LogWarning($"The assigned object does not implement {args.InterfaceType.Name}.");
                    return;
                }

                property.objectReferenceValue = targetObject;
            }
            else
            {
                var message = gameObjectName != null
                    ? $"Game Object '{gameObjectName}' does not have Component implement '{args.InterfaceType.Name}'."
                    : $"The assigned object does not implement '{args.InterfaceType.Name}'.";
                Debug.LogWarning(message);
            }
        }
    }

    public struct InterfaceArgs
    {
        public readonly Type ObjectType;
        public readonly Type InterfaceType;

        public InterfaceArgs(Type objectType, Type interfaceType)
        {
            Debug.Assert(typeof(Object).IsAssignableFrom(objectType),
                $"{nameof(objectType)} needs to be of Type {typeof(Object)}.");
            Debug.Assert(interfaceType.IsInterface, $"{nameof(interfaceType)} needs to be an interface.");

            ObjectType = objectType;
            InterfaceType = interfaceType;
        }
    }
}