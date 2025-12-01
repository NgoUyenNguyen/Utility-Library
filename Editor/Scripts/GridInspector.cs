using System;
using System.Linq;
using NgoUyenNguyen.Grid;
using UnityEditor;
using UnityEngine;

namespace NgoUyenNguyen.Editor
{

    [CustomEditor(typeof(BaseGrid), true), CanEditMultipleObjects]
    public class GridInspector : UnityEditor.Editor
    {
        private const string CELL_PREFAB_PATH = "Packages/com.ngouyennguyen.utilitylibrary/Runtime/Prefabs/Cell.prefab";
        private const string CELL_ICON_PATH = "Packages/com.ngouyennguyen.utilitylibrary/Editor/Textures/Rectangle_18.png";

        private Texture2D activeTex;
        private Cell defaultCellPrefab;

        private SerializedProperty cellMapProp;
        private SerializedProperty cellPrefabProp;
        private SerializedProperty sizeProp;
        private SerializedProperty cellSizeProp;
        private SerializedProperty alignmentProp;
        private SerializedProperty spaceProp;
        private SerializedProperty layoutProp;
        private SerializedProperty prefabInitializedProp;







        


        public void OnEnable()
        {
            AssignProperties();

            // Load default cell prefab and icon
            defaultCellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CELL_PREFAB_PATH).GetComponent<Cell>();
            activeTex = AssetDatabase.LoadAssetAtPath<Texture2D>(CELL_ICON_PATH);
            if (!(target as BaseGrid).prefabInitialized || cellPrefabProp.objectReferenceValue == null)
            {
                cellPrefabProp.objectReferenceValue = defaultCellPrefab;
                prefabInitializedProp.boolValue = true;
            }
            serializedObject.ApplyModifiedProperties();

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }





        // Method to assign properties to equivalent fields
        private void AssignProperties()
        {
            cellMapProp = serializedObject.FindProperty("_cellMap");
            cellPrefabProp = serializedObject.FindProperty("_cellPrefab");
            sizeProp = serializedObject.FindProperty("_size");
            cellSizeProp = serializedObject.FindProperty("_cellSize");
            alignmentProp = serializedObject.FindProperty("_alignment");
            spaceProp = serializedObject.FindProperty("_space");
            layoutProp = serializedObject.FindProperty("_layout");
            prefabInitializedProp = serializedObject.FindProperty("prefabInitialized");
        }

        // Method to handle undo and redo
        private void OnUndoRedo()
        {
            var grid = target as BaseGrid;
            
            // Undo grid size
            if (sizeProp.vector2IntValue != grid.size)
            {
                grid.PrefabCreate(grid.size, grid.cellPrefab.gameObject); // Recreate grid
                var cells = grid.GetComponentsInChildren<Cell>(); // Find all child cell objects
                for (int i = cells.Length - 1; i >= 0; i--)
                {
                    if (cells[i].transform == grid.transform) continue;
                    // If cell is not in the new grid, destroy it
                    if (!grid.cellMap.Contains(cells[i]))
                    {
                        DestroyImmediate(cells[i].gameObject);
                    }
                }
            }
            // Undo cell prefab
            else if (cellPrefabProp.objectReferenceValue != grid.cellPrefab)
            {
                serializedObject.ApplyModifiedProperties();
                for (int x = 0; x < grid.size.x; x++)
                {
                    for (int y = 0; y < grid.size.y; y++)
                    {
                        grid.SpawnCellPrefab(grid.cellPrefab, new Vector2Int(x, y)); // Replace cell prefab
                    }
                }
                
                var cells = grid.GetComponentsInChildren<Cell>(); // Find all child cell objects
                for (int i = cells.Length - 1; i >= 0; i--)
                {
                    if (cells[i].transform == grid.transform) continue;
                    // If cell is not in the new grid, destroy it
                    if (!grid.cellMap.Contains(cells[i]))
                    {
                        DestroyImmediate(cells[i].gameObject);
                    }
                }
                grid.CalculateCellsPosition();
            }
            // Undo cell size, alignment, or space
            else if (cellSizeProp.floatValue != grid.cellSize
                || alignmentProp.intValue != (int)grid.alignment
                || spaceProp.intValue != (int)grid.space)
            {
                serializedObject.ApplyModifiedProperties();
                grid.CalculateCellsPosition(); // Recalculate cell position
            }
            // Undo layout
            else if (layoutProp.intValue != (int)grid.layout)
            {
                serializedObject.ApplyModifiedProperties();
                grid.PrefabCreate(grid.size, grid.cellPrefab.gameObject);
            }
            else
            {
                serializedObject.Update();

                for (int i = 0; i < cellMapProp.arraySize; i++)
                {
                    var cellProp = cellMapProp.GetArrayElementAtIndex(i);
                    grid.cellMap[i] = cellProp.objectReferenceValue as Cell;
                }

                serializedObject.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(grid);
        }

        




        public override void OnInspectorGUI()
        {
            var grid = target as BaseGrid;

            DrawGridSetting(grid);

            DrawDefaultInspector();
        }




        private void DrawGridSetting(BaseGrid grid)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Space(10);
            GUILayout.Label("Grid Setting", EditorStyles.boldLabel);

            GUILayout.Space(10);
            PropertyDrawer(grid);
            GUILayout.Space(10);

            GUILayout.Space(10);
            if (targets.Length == 1)
            {
                GridMapDrawer(grid);
            }
            else
            {
                EditorGUILayout.HelpBox("Grid map editing is disabled when multiple objects are selected.", MessageType.Info);
            }
            GUILayout.Space(10);

            EditorGUILayout.EndVertical();
            GUILayout.Space(20);

            serializedObject.ApplyModifiedProperties();
        }

        private void GridMapDrawer(BaseGrid grid)
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();

            for (int y = sizeProp.vector2IntValue.y - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < sizeProp.vector2IntValue.x; x++)
                {
                    // Icon
                    Texture2D tex = cellMapProp.GetArrayElementAtIndex(sizeProp.vector2IntValue.x * y + x)
                        .objectReferenceValue != null
                        ? activeTex : null;

                    // Button
                    if (GUILayout.Button(tex, GUILayout.Width(40), GUILayout.Height(40)))
                    {
                        var cellProp = cellMapProp.GetArrayElementAtIndex(sizeProp.vector2IntValue.x * y + x);
                        if (cellProp.objectReferenceValue != null)
                        {
                            // Delete Cell
                            Undo.RecordObject(cellProp.objectReferenceValue, "Delete Cell");
                            Undo.DestroyObjectImmediate((cellProp.objectReferenceValue as Cell).gameObject);
                            cellProp.objectReferenceValue = null;
                            serializedObject.ApplyModifiedProperties();
                        }
                        else
                        {
                            // Add Cell
                            Undo.RegisterCreatedObjectUndo(grid.SpawnCellPrefab(grid.cellPrefab, new Vector2Int(x, y)).gameObject, "Instantiate Prefab");
                            serializedObject.ApplyModifiedProperties();
                            grid.CalculateCellsPosition();
                        }
                        EditorUtility.SetDirty(grid);
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(grid.gameObject.scene);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void PropertyDrawer(BaseGrid grid)
        {
            serializedObject.Update();

            #region Prefab Setting

            // Set Prefab
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(cellPrefabProp);
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                if (cellPrefabProp.objectReferenceValue == null)
                {
                    cellPrefabProp.objectReferenceValue = defaultCellPrefab;
                    serializedObject.ApplyModifiedProperties();
                }

                foreach (var t in targets)
                {
                    for (int x = 0; x < grid.size.x; x++)
                    {
                        for (int y = 0; y < grid.size.y; y++)
                        {
                            if (grid.cellMap[x + y * grid.size.x] == null) continue;
                            grid.SpawnCellPrefab(grid.cellPrefab, new Vector2Int(x, y));
                        }
                    }
                    grid.CalculateCellsPosition();
                }
            }

            #endregion

            #region Layout Setting

            //Set Layout
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(layoutProp);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var t in targets)
                {
                    serializedObject.ApplyModifiedProperties();
                    grid.PrefabCreate(grid.size, grid.cellPrefab.gameObject);
                }
            }

            #endregion


            #region Alignment, Space, and Cell Size Setting

            EditorGUI.BeginChangeCheck();
            // Set Alignment
            EditorGUILayout.PropertyField(alignmentProp);
            // Set Space
            EditorGUILayout.PropertyField(spaceProp);
            // Set Cell Size
            EditorGUILayout.PropertyField(cellSizeProp);
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var t in targets)
                {
                    grid.CalculateCellsPosition();
                }
                EditorUtility.SetDirty(grid);
            }

            #endregion

            #region Size Setting

            EditorGUI.BeginChangeCheck();
            // Set Size
            EditorGUILayout.PropertyField(sizeProp);
            
            if (sizeProp.vector2IntValue.x < 0) 
                sizeProp.vector2IntValue = new Vector2Int(0, sizeProp.vector2IntValue.y);
            if (sizeProp.vector2IntValue.y < 0)
                sizeProp.vector2IntValue = new Vector2Int(sizeProp.vector2IntValue.x, 0);
            
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var t in targets)
                {
                    grid.PrefabCreate(grid.size, grid.cellPrefab.gameObject);
                }
                var cells = grid.GetComponentsInChildren<Cell>(); // Find all child cell objects
                for (int i = cells.Length - 1; i >= 0; i--)
                {
                    if (cells[i].transform == grid.transform) continue;
                    // If cell is not in the new grid, destroy it
                    if (!grid.cellMap.Contains(cells[i]))
                    {
                        DestroyImmediate(cells[i].gameObject);
                    }
                }
                EditorUtility.SetDirty(grid);
            }

            #endregion
        }
    }
}


