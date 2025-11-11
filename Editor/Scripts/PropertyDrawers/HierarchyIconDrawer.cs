using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NgoUyenNguyen;
using UnityEditor;
using UnityEngine;

namespace NgoUyenNguyen.Editor
{
    [InitializeOnLoad]
    public static class HierarchyIconDrawer
    {
        private static readonly Texture2D RequiredIcon =
            AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Packages/com.ngouyennguyen.utilitylibrary/Editor/Textures/WarningIcon.png");

        private static readonly Dictionary<Type, FieldInfo[]> CachedFieldInfo = new();

        static HierarchyIconDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is not GameObject gameObject) return;

            if (!HasMissingFieldInHierarchy(gameObject)) return;

            var iconRect = new Rect(selectionRect.xMax - 20, selectionRect.y, 18, 18);
            var tooltipMessage =
                $"Game Object '{gameObject.name}' or its child/children contain empty or unassigned field(s).";
            GUI.Label(iconRect, new GUIContent(RequiredIcon, tooltipMessage));
        }

        private static bool HasMissingFieldInHierarchy(GameObject go)
        {
            if (go == null) return false;

            foreach (var component in go.GetComponents<Component>())
            {
                if (component == null) continue;

                var fields = GetCachedFieldsWithMustAssignAttribute(component.GetType());
                if (fields == null || fields.Length == 0) continue;

                if (fields.Any(field => IsFieldUnassigned(field.GetValue(component))))
                    return true;
            }

            foreach (Transform child in go.transform)
            {
                if (HasMissingFieldInHierarchy(child.gameObject))
                    return true;
            }

            return false;
        }


        private static FieldInfo[] GetCachedFieldsWithMustAssignAttribute(Type componentType)
        {
            if (CachedFieldInfo.TryGetValue(componentType, out var fields)) return fields;

            fields = componentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            List<FieldInfo> mustAssignFields = new();

            foreach (var field in fields)
            {
                var isSerialized = field.IsPublic || field.IsDefined(typeof(SerializeField), true);
                var isMustAssign = field.IsDefined(typeof(MustAssignAttribute), true);

                if (isSerialized && isMustAssign)
                {
                    mustAssignFields.Add(field);
                }
            }

            CachedFieldInfo[componentType] = mustAssignFields.ToArray();
            return CachedFieldInfo[componentType];
        }

        private static bool IsFieldUnassigned(object fieldValue)
        {
            if (fieldValue == null) return true;
            if (fieldValue is int and 0) return true;
            if (fieldValue is float and 0) return true;
            if (fieldValue is uint and 0) return true;
            if (fieldValue is Vector3 && fieldValue.Equals(Vector3.zero)) return true;
            if (fieldValue is Vector3Int && fieldValue.Equals(Vector3Int.zero)) return true;
            if (fieldValue is Vector2 && fieldValue.Equals(Vector2.zero)) return true;
            if (fieldValue is Vector2Int && fieldValue.Equals(Vector2Int.zero)) return true;
            if (fieldValue is Vector4 && fieldValue.Equals(Vector4.zero)) return true;
            if (fieldValue is Quaternion && fieldValue.Equals(Quaternion.identity)) return true;
            if (fieldValue is bool && fieldValue.Equals(false)) return true;
            if (fieldValue is Enum && fieldValue.Equals(0)) return true;
            if (fieldValue is string stringValue && string.IsNullOrEmpty(stringValue)) return true;
            if (fieldValue is System.Collections.IEnumerable enumerable)
            {
                bool hasAny = false;
                foreach (var item in enumerable)
                {
                    hasAny = true;
                    if (IsFieldUnassigned(item)) return true;
                }

                return !hasAny;
            }

            return false;
        }
    }
}