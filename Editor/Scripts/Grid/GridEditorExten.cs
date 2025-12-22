using System;
using NgoUyenNguyen.Grid;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NgoUyenNguyen.Editor
{
    /// <summary>
    /// Provides static extension methods for creating and managing grids in the editor.
    /// </summary>
    public static class GridEditorExten
    {
        /// <summary>
        /// <c>ONLY USE IN EDITOR</c><br/>
        /// Method to create <c>Grid</c> which remaining its connecting to original <c>Cell</c> prefab
        /// </summary>
        /// <param name="width">Width of grid</param>
        /// <param name="height">Height of grid</param>
        /// <param name="cellPrefab">Cell Prefab to be spawned</param>
        /// <param name="map">Map to set each cell spawned in detail.<br/> 
        /// <c>True = spawn in this index</c> <br/>
        /// <c>False = not spawn in this index</c>
        /// </param>
        public static void PrefabCreate(this BaseGrid grid, int width, int height, GameObject cellPrefab)
        {
            PrefabCreate(grid, new Vector2Int(width, height), cellPrefab);
        }

        /// <summary>
        /// <c>ONLY USE IN EDITOR</c><br/>
        /// Method to create <c>Grid</c> which remaining its connecting to original <c>Cell</c> prefab
        /// </summary>
        /// <param name="size">Size of grid</param>
        /// <param name="cellPrefab">Cell Prefab to be spawned</param>
        /// <param name="map">Map to set each cell spawned in detail.<br/> 
        /// <c>True = spawn in this index</c> <br/>
        /// <c>False = not spawn in this index</c>
        /// </param>
        public static void PrefabCreate(this BaseGrid grid, Vector2Int size, GameObject cellPrefab)
        {
            if (!cellPrefab.TryGetComponent<Cell>(out _))
            {
                throw new ArgumentException("Prefab does not have Cell component!", nameof(cellPrefab));
            }

            DestroyOldCellPrefabs(grid);

            // Set Size
            grid.size = size;
            grid.cellMap = new Cell[size.x * size.y];

            CellPrefabSpawn(grid, cellPrefab);
        }

        /// <summary>
        /// <c>ONLY USE IN EDITOR</c><br/>
        /// Method to create <c>Grid</c> which remaining its connecting to original <c>Cell</c> prefab
        /// </summary>
        /// <param name="map">Map to set each cell spawned in detail.<br/> 
        /// <c>True = spawn in this index</c> <br/>
        /// <c>False = not spawn in this index</c></param>
        /// <param name="cellPrefab">Cell Prefab to be spawned</param>
        public static void PrefabCreate(this BaseGrid grid, bool[,] map, GameObject cellPrefab)
        {
            if (!cellPrefab.TryGetComponent<Cell>(out _))
            {
                throw new ArgumentException("Prefab does not have Cell component!", nameof(cellPrefab));
            }

            DestroyOldCellPrefabs(grid);

            // Set Size
            grid.size = new Vector2Int(map.GetLength(0), map.GetLength(1));
            grid.cellMap = new Cell[grid.size.x * grid.size.y];

            CellPrefabSpawn(grid, cellPrefab, map);
        }

        private static void DestroyOldCellPrefabs(this BaseGrid grid)
        {
            for (int i = grid.cellMap.Length - 1; i >= 0; i--)
            {
                if (grid.cellMap[i] != null)
                {
                    Object.DestroyImmediate(grid.cellMap[i].gameObject);
                }
            }
        }

        private static void CellPrefabSpawn(this BaseGrid grid , GameObject cellPrefab, bool[,] map = null)
        {
            grid.cellPrefab = cellPrefab.GetComponent<Cell>();

            for (int x = 0; x < grid.size.x; x++)
            {
                for (int y = 0; y < grid.size.y; y++)
                {
                    if (map != null && !map[x, y]) continue;
                    grid.cellMap[x + y * grid.size.x] = (PrefabUtility.InstantiatePrefab(cellPrefab, grid.transform) as GameObject)
                        .GetComponent<Cell>();
                    grid.cellMap[x + y * grid.size.x].grid = grid;
                    grid.AssignCellIndex(grid.cellMap[x + y * grid.size.x], new Vector2Int(x, y));
                    grid.cellMap[x + y * grid.size.x].transform.localPosition = grid.CellToLocal(grid.cellMap[x + y * grid.size.x]);
                }
            }
        }

        /// <summary>
        /// Spawns a cell prefab at the specified index within the grid, replacing any existing cell at that position.
        /// </summary>
        /// <param name="grid">The grid in which the cell should be spawned.</param>
        /// <param name="cellPrefab">The cell prefab to be instantiated.</param>
        /// <param name="index">The index within the grid where the cell should be placed.</param>
        /// <returns>The newly instantiated cell.</returns>
        public static Cell SpawnCellPrefab(this BaseGrid grid, Cell cellPrefab, Vector2Int index)
        {
            if (cellPrefab == null) return null;
            if (grid.cellMap[index.x + index.y * grid.size.x] != null)
                Object.DestroyImmediate(grid.cellMap[index.x + index.y * grid.size.x].gameObject);

            Cell cell = (PrefabUtility.InstantiatePrefab(cellPrefab.gameObject, grid.transform) as GameObject)
                .GetComponent<Cell>();
            cell.index = grid.layout switch
            {
                CellLayout.Hexagon => grid.IndexToAxial(index),
                _ => index
            };
            cell.grid = grid;
            grid.cellMap[index.x + index.y * grid.size.x] = cell;

            return cell;
        }
    }
}