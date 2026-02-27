using NgoUyenNguyen.Grid;
using UnityEditor;
using UnityEngine;

namespace NgoUyenNguyen.Editor
{
    /// <summary>
    /// Provides static extension methods for creating and managing grids in the editor.
    /// </summary>
    public static class GridEditorExten
    {
        /// <summary>
        /// Method to create a grid with specified dimensions and a given prefab for the cells.
        /// This method establishes a new grid configuration, destroys previous cell prefabs,
        /// and spawns new cell prefabs based on the specified width, height, and prefab.
        /// </summary>
        /// <param name="grid">The grid object where the cells will be created.</param>
        /// <param name="width">The number of columns for the grid.</param>
        /// <param name="height">The number of rows for the grid.</param>
        /// <param name="cellPrefab">The cell prefab to instantiate for each grid cell.</param>
        public static void PrefabCreate(this BaseGrid grid, int width, int height, Cell cellPrefab)
        {
            grid.PrefabCreate(new Vector2Int(width, height), cellPrefab);
        }

        /// <summary>
        /// Method to create a grid with specified dimensions and a given prefab for the cells.
        /// This method establishes a new grid configuration, destroys previous cell prefabs,
        /// and spawns new cell prefabs based on the specified width, height, and prefab.
        /// </summary>
        /// <param name="grid">The target grid to be modified.</param>
        /// <param name="size">The size of the grid.</param>
        /// <param name="cellPrefab">The cell prefab to be instantiated within the grid.</param>
        public static void PrefabCreate(this BaseGrid grid, Vector2Int size, Cell cellPrefab)
        {
            grid.DestroyOldCellPrefabs();

            // Set Size
            grid.Size = size;
            grid.CellMap = new Cell[size.x * size.y];

            grid.CellPrefabSpawn(cellPrefab);
        }

        /// <summary>
        /// Creates a grid using a two-dimensional boolean map to define the placement of cell prefabs.
        /// This method destroys any existing cell prefabs in the grid, updates the grid's size,
        /// and spawns new cell prefabs based on the provided map.
        /// </summary>
        /// <param name="grid">The grid object where the cells will be created.</param>
        /// <param name="map">A two-dimensional boolean array representing the grid layout, where true indicates the presence of a cell.</param>
        /// <param name="cellPrefab">The cell prefab to instantiate for each active grid cell based on the map.</param>
        public static void PrefabCreate(this BaseGrid grid, bool[,] map, Cell cellPrefab)
        {
            grid.DestroyOldCellPrefabs();

            // Set Size
            grid.Size = new Vector2Int(map.GetLength(0), map.GetLength(1));
            grid.CellMap = new Cell[grid.Size.x * grid.Size.y];

            grid.CellPrefabSpawn(cellPrefab, map);
        }

        private static void DestroyOldCellPrefabs(this BaseGrid grid)
        {
            for (var i = grid.CellMap.Length - 1; i >= 0; i--)
            {
                if (grid.CellMap[i] != null)
                {
                    Object.DestroyImmediate(grid.CellMap[i].gameObject);
                }
            }
        }

        private static void CellPrefabSpawn(this BaseGrid grid , Cell cellPrefab, bool[,] map = null)
        {
            grid.CellPrefab = cellPrefab;

            for (var x = 0; x < grid.Size.x; x++)
            {
                for (var y = 0; y < grid.Size.y; y++)
                {
                    if (map != null && !map[x, y]) continue;
                    grid.CellMap[x + y * grid.Size.x] = (PrefabUtility.InstantiatePrefab(cellPrefab.gameObject, grid.transform) as GameObject)!
                        .GetComponent<Cell>();
                    grid.CellMap[x + y * grid.Size.x].grid = grid;
                    grid.AssignCellIndex(grid.CellMap[x + y * grid.Size.x], new Vector2Int(x, y));
                    
                    var localPos = grid.CellToLocal(grid.CellMap[x + y * grid.Size.x]);
                    if (localPos == null) continue;
                    grid.CellMap[x + y * grid.Size.x].transform.localPosition = localPos.Value;
                }
            }
        }
        
        internal static Cell SpawnCellPrefab(this BaseGrid grid, Cell cellPrefab, Vector2Int index)
        {
            if (cellPrefab == null) return null;
            if (grid.CellMap[index.x + index.y * grid.Size.x] != null)
                Object.DestroyImmediate(grid.CellMap[index.x + index.y * grid.Size.x].gameObject);

            var cell = (PrefabUtility.InstantiatePrefab(cellPrefab.gameObject, grid.transform) as GameObject)!
                .GetComponent<Cell>();
            cell.index = grid.Layout switch
            {
                CellLayout.Hexagon => grid.IndexToAxial(index),
                _ => index
            };
            cell.grid = grid;
            grid.CellMap[index.x + index.y * grid.Size.x] = cell;

            return cell;
        }
    }
}