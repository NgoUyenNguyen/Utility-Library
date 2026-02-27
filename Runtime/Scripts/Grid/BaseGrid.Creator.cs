using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NgoUyenNguyen.Grid
{
    public partial class BaseGrid
    {
        /// <summary>
        /// Method to create <c>Grid</c>
        /// </summary>
        /// <param name="width">Width of grid</param>
        /// <param name="height">Height of grid</param>
        /// <param name="cell">Cell Prefab to be spawned</param>
        public void Create(int width, int height, Cell cell = null)
        {
            DestroyOldCells();

            // Set Size
            _size = new Vector2Int(width, height);
            _cellMap = new Cell[Size.x * Size.y];

            // Set Cell Prefab
            if (cell != null)
            {
                CellPrefab = cell;
            }

            CellSpawn();
        }

        /// <summary>
        /// Method to create <c>Grid</c>
        /// </summary>
        /// <param name="map">Map to set each cell spawned in detail.<br/> 
        /// <c>True = spawn in this index</c> <br/>
        /// <c>False = not spawn in this index</c>
        /// </param>
        /// <param name="cell">Cell Prefab to be spawned</param>
        public void Create(bool[,] map, Cell cell = null)
        {
            DestroyOldCells();

            //Set Size
            _size = new Vector2Int(map.GetLength(0), map.GetLength(1));
            _cellMap = new Cell[Size.x * Size.y];

            // Set Cell Prefab
            if (cell != null)
            {
                CellPrefab = cell;
            }

            CellSpawn(map);
        }

        /// <summary>
        /// Method to create <c>Grid</c>
        /// </summary>
        /// <param name="size">Size of grid</param>
        /// <param name="cell"><c>Cell</c> to be spawned</param>
        // ReSharper disable once ParameterHidesMember
        public void Create(Vector2Int size, Cell cell = null)
        {
            Create(size.x, size.y, cell);
        }

        private void DestroyOldCells()
        {
            for (int i = _cellMap.Length - 1; i >= 0; i--)
            {
                if (_cellMap[i] != null)
                {
                    Destroy(_cellMap[i].gameObject);
                }
            }
        }

        /// <summary>
        /// Asynchronously creates a grid with the specified dimensions and cell prefab.
        /// </summary>
        /// <param name="width">The width of the grid.</param>
        /// <param name="height">The height of the grid.</param>
        /// <param name="cell">The prefab for the grid's cells. If null, the current prefab will be used.</param>
        /// <param name="token">The cancellation token used to cancel the operation.</param>
        /// <returns>A UniTask representing the asynchronous operation.</returns>
        public async UniTask CreateAsync(int width, int height, Cell cell = null, CancellationToken token = default)
        {
            DestroyOldCells();

            // Set Size
            _size = new Vector2Int(width, height);
            _cellMap = new Cell[Size.x * Size.y];

            // Set Cell Prefab
            if (cell != null)
            {
                CellPrefab = cell;
            }

            await CellSpawnAsync(token: token);
        }

        /// <summary>
        /// Asynchronously creates a grid based on the provided map and optional cell prefab.
        /// </summary>
        /// <param name="map">A 2D boolean array representing the structure of the grid.</param>
        /// <param name="cell">Optional cell prefab to be used for grid creation. If null, the current cell prefab will be used.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous operation if needed.</param>
        /// <returns>A task representing the asynchronous grid creation process.</returns>
        public async UniTask CreateAsync(bool[,] map, Cell cell = null, CancellationToken token = default)
        {
            DestroyOldCells();

            //Set Size
            _size = new Vector2Int(map.GetLength(0), map.GetLength(1));
            _cellMap = new Cell[Size.x * Size.y];

            // Set Cell Prefab
            if (cell != null)
            {
                CellPrefab = cell;
            }

            await CellSpawnAsync(map, token);
        }

        /// <summary>
        /// Asynchronously creates a grid with the specified size and cell prefab.
        /// </summary>
        /// <param name="size">The size of the grid.</param>
        /// <param name="cell">Optional cell prefab to be used for grid creation. If null, the current cell prefab will be used.</param>
        /// <param name="token">The cancellation token used to cancel the asynchronous operation. Default is none.</param>
        /// <returns>A UniTask representing the asynchronous operation.</returns>
        public async UniTask CreateAsync(Vector2Int size, Cell cell = null, CancellationToken token = default)
        {
            await CreateAsync(size.x, size.y, cell, token);
        }

        private void CellSpawn(bool[,] map = null)
        {
            for (var x = 0; x < Size.x; x++)
            {
                for (var y = 0; y < Size.y; y++)
                {
                    if (map != null && !map[x, y]) continue;

                    _cellMap[x + y * Size.x] = Instantiate(_cellPrefab, transform);
                    _cellMap[x + y * Size.x].grid = this;

                    AssignCellIndex(_cellMap[x + y * Size.x], new Vector2Int(x, y));

                    var localPos = CellToLocal(_cellMap[x + y * Size.x]);

                    if (localPos == null) continue;

                    _cellMap[x + y * Size.x].transform.localPosition = localPos.Value;
                }
            }
        }

        private async UniTask CellSpawnAsync(bool[,] map = null, CancellationToken token = default)
        {
            var instantiateParam = new InstantiateParameters
            {
                parent = transform,
                scene = gameObject.scene,
            };

            int spawnAmount;
            if (map == null)
            {
                spawnAmount = Size.x * Size.y;
            }
            else
            {
                spawnAmount = 0;
                for (var x = 0; x < Size.x; x++)
                {
                    for (var y = 0; y < Size.y; y++)
                    {
                        if (map[x, y]) spawnAmount++;
                    }
                }
            }
            
            var cells = await InstantiateAsync(_cellPrefab, spawnAmount, instantiateParam, token);
            var cellCount = 0;
            for (var x = 0; x < Size.x; x++)
            {
                for (var y = 0; y < Size.y; y++)
                {
                    if (map != null && !map[x, y]) continue;

                    var index1D = x + y * Size.x;
                    _cellMap[index1D] = cells[cellCount++];
                    _cellMap[index1D].grid = this;

                    AssignCellIndex(_cellMap[index1D], new Vector2Int(x, y));

                    var localPos = CellToLocal(_cellMap[x + y * Size.x]);

                    if (localPos == null) continue;

                    _cellMap[index1D].transform.localPosition = localPos.Value;
                }
            }
        }

        /// <summary>
        /// Method to calculate <c>Cells</c> <c>LocalSpace</c> position
        /// </summary>
        public void CalculateCellsPosition()
        {
            foreach (var cell in _cellMap)
            {
                if (cell == null) continue;

                var localPos = CellToLocal(cell);

                if (localPos == null) continue;
                cell.transform.localPosition = localPos.Value;
            }
        }

        /// <summary>
        /// Assigns an index to a cell based on the grid's layout.
        /// </summary>
        /// <param name="cell">The cell to assign an index to.</param>
        /// <param name="index">The 2D index of the cell in the grid.</param>
        public void AssignCellIndex(Cell cell, Vector2Int index)
        {
            cell.index = _layout switch
            {
                CellLayout.Square => index,
                CellLayout.Hexagon => IndexToAxial(index),
                _ => default
            };
        }

        private void RecalculateCellsIndex(CellLayout newLayout)
        {
            if (Layout == newLayout) return;
            foreach (var cell in _cellMap)
            {
                if (cell == null) continue;
                cell.index = _layout switch
                {
                    CellLayout.Square => IndexToAxial(cell.index),
                    CellLayout.Hexagon => AxialToIndex(cell.index),
                    _ => default
                };
            }
        }
    }
}