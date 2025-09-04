using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Assertions;

namespace NgoUyenNguyen.GridSystem
{

    /// <summary>
    /// Represents a grid structure composed of cells, supporting various alignment and spacing options.
    /// </summary>
    /// <remarks>The <see cref="BaseGrid"/> class provides functionality for creating, managing, and
    /// interacting with a grid of cells. Likely a collection, 
    /// <see cref="BaseGrid"/> can access cells through index like a 2D array and iterated by foreach loop</remarks>
    public abstract class BaseGrid : MonoBehaviour, IEnumerable<Cell>
    {
        public enum Alignment
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Center,
        }

        public enum Space
        {
            Horizontal,
            Vertical
        }

#if UNITY_EDITOR
        [HideInInspector] public bool prefabInitialized;
#endif






        [SerializeField] private Cell[] _cellMap = new Cell[0];
        [SerializeField, HideInInspector] private Cell _cellPrefab;
        [SerializeField, HideInInspector] private Vector2Int _size;
        [SerializeField, HideInInspector] private float _cellSize = 1;
        [SerializeField, HideInInspector] private Alignment _alignment;
        [SerializeField, HideInInspector] private Space _space;

        /// <summary>
        /// Prefab of <c>Cell</c> spawned in <c>Grid</c>
        /// </summary>
        public Cell cellPrefab { get => _cellPrefab; private set => _cellPrefab = value; }

        /// <summary>
        /// Size of <c>Grid</c>
        /// </summary>
        public Vector2Int size { get => _size; set => _size = value; }

        /// <summary>
        /// Size of each <c>Cell</c> in <c>Grid</c>
        /// </summary>
        public float cellSize { get => _cellSize; set => _cellSize = value; }

        /// <summary>
        /// Alignment of <c>Grid</c>
        /// </summary>
        /// <remarks>
        /// Define where <c>Cells</c> are spawned from <c>Grid</c> position
        /// </remarks>
        public Alignment alignment { get => _alignment; set => _alignment = value; }

        /// <summary>
        /// Coordinate space in which the operation is performed.
        /// </summary>
        public Space space { get => _space; set => _space = value; }

        /// <summary>
        /// Represents the collection of <c>Cells</c> in the <c>Grid</c>
        /// </summary>
        /// <remarks>This array holds all references to <c>Cells</c> in <c>Grid</c>.</remarks>
        public Cell[] cellMap { get => _cellMap; set => _cellMap = value; }





        /// <summary>
        /// Access <c>Cell</c> in <c>Grid</c> through (<paramref name="x"/>, <paramref name="y"/>) coordinate
        /// </summary>
        /// <param name="x">Row index (0 to <see cref="size"/> - 1)</param>
        /// <param name="y">Column index (0 to <see cref="size"/> - 1)</param>
        /// <returns><see cref="Cell"/> in <paramref name="x"/>, <paramref name="y"/></returns>
        public Cell this[int x, int y]
        {
            get
            {
                return _cellMap[size.x * y + x];
            }
            set
            {
                _cellMap[size.x * y + x] = value;
            }
        }

        /// <summary>
        /// Access <c>Cell</c> in <c>Grid</c> through index
        /// </summary>
        /// <param name="index">Index of <c>Cell</c></param>
        /// <returns>Cell in <paramref name="index"/></returns>
        public Cell this[Vector2Int index]
        {
            get
            {
                return _cellMap[size.x * index.y + index.x];
            }
            set
            {
                _cellMap[size.x * index.y + index.x] = value;
            }
        }





        /// <summary>
        /// Method to create <c>Grid</c>
        /// </summary>
        /// <param name="width">Width of grid</param>
        /// <param name="height">Height of grid</param>
        /// <param name="cell"><c>Cell</c> to be spawned</param>
        public void Create(int width, int height, Cell cell = null)
        {
            DestroyOldCells();

            // Set Size
            size = new Vector2Int(width, height);
            _cellMap = new Cell[size.x * size.y];

            // Set Cell Prefab
            if (cell != null)
            {
                cellPrefab = cell;
            }

            CellSpawn(cellPrefab);
        }

        /// <summary>
        /// Method to create <c>Grid</c>
        /// </summary>
        /// <param name="map">Map to set each cell spawned in detail.<br/> 
        /// <c>True = spawn in this index</c> <br/>
        /// <c>False = not spawn in this index</c>
        /// </param>
        /// <param name="cell"><c>Cell</c> to be spawned</param>
        public void Create(bool[,] map, Cell cell = null)
        {
            DestroyOldCells();

            //Set Size
            size = new Vector2Int(map.GetLength(0), map.GetLength(1));
            _cellMap = new Cell[size.x * size.y];

            // Set Cell Prefab
            if (cell != null)
            {
                cellPrefab = cell;
            }

            CellSpawn(cellPrefab, map);
        }

        /// <summary>
        /// Method to create <c>Grid</c>
        /// </summary>
        /// <param name="size">Size of grid</param>
        /// <param name="cell"><c>Cell</c> to be spawned</param>
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

        private void CellSpawn(Cell cell, bool[,] map = null)
        {
            if (map != null)
            {
                Assert.AreEqual(size.x, map.GetLength(0));
                Assert.AreEqual(size.y, map.GetLength(1));
            }
            if (cell != null)
            {
                cellPrefab = cell;
            }

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    if (map != null && !map[x, y]) continue;
                    this[x, y] = Instantiate(cellPrefab.gameObject, transform).GetComponent<Cell>();
                    this[x, y].index = new Vector2Int(x, y);
                    this[x, y].grid = this;
                }
            }

            CalculateCellsPosition();
        }

        /// <summary>
        /// Method to calculate <c>Cells</c> <c>LocalSpace</c> position
        /// </summary>
        public void CalculateCellsPosition()
        {
            foreach (var cell in this)
            {
                if (cell == null) continue;
                cell.transform.localPosition = CellToLocal(cell);
            }
        }




#if UNITY_EDITOR
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
        public void PrefabCreate(int width, int height, GameObject cellPrefab)
        {
            PrefabCreate(new Vector2Int(width, height), cellPrefab);
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
        public void PrefabCreate(Vector2Int size, GameObject cellPrefab)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("Can Not Create Prefab In Runtime!");
            }
            else if (!cellPrefab.TryGetComponent<Cell>(out _))
            {
                Debug.LogError("Prefab does not have Cell component!");
            }
            else
            {
                DestroyOldCellPrefabs();

                // Set Size
                this.size = size;
                _cellMap = new Cell[size.x * size.y];

                CellPrefabSpawn(cellPrefab);
            }
        }

        /// <summary>
        /// <c>ONLY USE IN EDITOR</c><br/>
        /// Method to create <c>Grid</c> which remaining its connecting to original <c>Cell</c> prefab
        /// </summary>
        /// <param name="map">Map to set each cell spawned in detail.<br/> 
        /// <c>True = spawn in this index</c> <br/>
        /// <c>False = not spawn in this index</c></param>
        /// <param name="cellPrefab">Cell Prefab to be spawned</param>
        public void PrefabCreate(bool[,] map, GameObject cellPrefab)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("Can Not Create Prefab In Runtime!");
            }
            else if (!cellPrefab.TryGetComponent<Cell>(out _))
            {
                Debug.LogError("Prefab does not have Cell component!");
            }
            else
            {
                DestroyOldCellPrefabs();

                // Set Size
                size = new Vector2Int(map.GetLength(0), map.GetLength(1));
                _cellMap = new Cell[size.x * size.y];

                CellPrefabSpawn(cellPrefab, map);
            }
        }

        private void DestroyOldCellPrefabs()
        {
            for (int i = _cellMap.Length - 1; i >= 0; i--)
            {
                if (_cellMap[i] != null)
                {
                    DestroyImmediate(_cellMap[i].gameObject);
                }
            }
        }

        private void CellPrefabSpawn(GameObject cellPrefab, bool[,] map = null)
        {
            if (map != null)
            {
                Assert.AreEqual(size.x, map.GetLength(0));
                Assert.AreEqual(size.y, map.GetLength(1));
            }

            this.cellPrefab = cellPrefab.GetComponent<Cell>();

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    if (map != null && !map[x, y]) continue;
                    this[x, y] = (PrefabUtility.InstantiatePrefab(cellPrefab, transform) as GameObject).GetComponent<Cell>();
                    this[x, y].index = new Vector2Int(x, y);
                    this[x, y].grid = this;
                }
            }

            CalculateCellsPosition();
        }

        /// <summary>
        /// <c>ONLY USE IN EDITOR</c><br/>
        /// Method to spawn prefab at specific <c>Index</c>
        /// </summary>
        /// <param name="cellPrefab"></param>
        /// <param name="index"></param>
        /// <returns><c>Cell</c> at <c>Index</c></returns>
        public Cell SpawnCellPrefab(Cell cellPrefab, Vector2Int index)
        {
            if (cellPrefab == null) return null;
            if (this[index]  != null) DestroyImmediate(this[index].gameObject);
            Cell cell = (PrefabUtility.InstantiatePrefab(cellPrefab.gameObject, transform) as GameObject).GetComponent<Cell>();
            cell.index = index;
            cell.grid = this;
            this[index] = cell;

            return cell;
        }
#endif








        /// <summary>
        /// Method to get <c>LocalSpace</c> position from <c>Cell</c>
        /// </summary>
        /// <param name="cell"></param>
        /// <returns><c>LocalSpace</c> position</returns>
        public Vector3 CellToLocal(Cell cell)
        {
            if (!cellMap.Contains(cell))
            {
                Debug.LogError($"{this} not contail {cell}");
                return default;
            }
            return IndexToLocal(cell.index);
        }

        /// <summary>
        /// Method to get <c>WolrdSpace</c> position from <c>Cell</c>
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>WorldSpace position</returns>
        public Vector3 CellToWorld(Cell cell)
        {
            return transform.TransformPoint(CellToLocal(cell));
        }

        /// <summary>
        /// Method to get <c>Cell</c> from <c>LocalSpace</c> position
        /// </summary>
        /// <remarks>
        /// If input position is out of bounds, return the nearest <c>Cell</c>
        /// </remarks>
        /// <param name="localPos"></param>
        /// <returns>Nearest <c>Cell</c></returns>
        public Cell LocalToCell(Vector3 localPos)
        {
            return this[LocalToIndex(localPos)];
        }

        /// <summary>
        /// Method to get <c>Cell</c> from <c>WorldSpace</c> position
        /// </summary>
        /// <remarks>
        /// If input position is out of bounds, return the nearest <c>Cell</c>
        /// </remarks>
        /// <param name="worldPos"></param>
        /// <returns>Nearest <c>Cell</c></returns>
        public Cell WorldToCell(Vector3 worldPos)
        {
            return LocalToCell(transform.InverseTransformPoint(worldPos));
        }

        /// <summary>
        /// Method to get <c>Index</c> in <see cref="_cellMap"/> from <c>WorldSpace</c> position
        /// </summary>
        /// <remarks>
        /// If input position is out of bounds, return the nearest <c>Index</c>
        /// </remarks>
        /// <param name="worldPos"></param>
        /// <returns>Nearest <c>Index</c></returns>
        public Vector2Int WorldToIndex(Vector3 worldPos)
        {
            return LocalToIndex(transform.InverseTransformPoint(worldPos));
        }

        /// <summary>
        /// Method to get <c>Index</c> in <see cref="_cellMap"/> from <c>LocalSpace</c> position
        /// </summary>
        /// <remarks>
        /// If input position is out of bounds, return the nearest <c>Index</c>
        /// </remarks>
        /// <param name="localPos"></param>
        /// <returns>Nearest <c>Index</c></returns>
        public Vector2Int LocalToIndex(Vector3 localPos)
        {
            float xPos = localPos.x;
            float yPos = space switch
            {
                Space.Horizontal => localPos.z,
                Space.Vertical => localPos.y,
                _ => 0
            };

            int colIndex = 0, rowIndex = 0;
            switch (alignment)
            {
                case Alignment.BottomLeft:
                    colIndex = Mathf.FloorToInt(xPos / cellSize);
                    rowIndex = Mathf.FloorToInt(yPos / cellSize);
                    break;

                case Alignment.BottomRight:
                    colIndex = (size.x - 1) - Mathf.FloorToInt(xPos / cellSize);
                    rowIndex = Mathf.FloorToInt(yPos / cellSize);
                    break;

                case Alignment.TopLeft:
                    colIndex = Mathf.FloorToInt(xPos / cellSize);
                    rowIndex = (size.y - 1) - Mathf.FloorToInt(yPos / cellSize);
                    break;

                case Alignment.TopRight:
                    colIndex = (size.x - 1) - Mathf.FloorToInt(xPos / cellSize);
                    rowIndex = (size.y - 1) - Mathf.FloorToInt(yPos / cellSize);
                    break;

                case Alignment.Center:
                    colIndex = Mathf.FloorToInt((xPos + (size.x * cellSize) / 2f) / cellSize);
                    rowIndex = Mathf.FloorToInt((yPos + (size.y * cellSize) / 2f) / cellSize);
                    break;
            }

            colIndex = Mathf.Clamp(colIndex, 0, size.x - 1);
            rowIndex = Mathf.Clamp(rowIndex, 0, size.y - 1);

            return new Vector2Int(colIndex, rowIndex);
        }

        /// <summary>
        /// Get <c>WorldSpace</c> position from <c>Index</c>
        /// </summary>
        /// <param name="index"></param>
        /// <returns><c>WorldSpace</c> position</returns>
        public Vector3 IndexToWorld(Vector2Int index)
        {
            return transform.TransformPoint(IndexToLocal(index));
        }

        /// <summary>
        /// Get <c>LocalSpace</c> position from <c>Index</c>
        /// </summary>
        /// <param name="index"></param>
        /// <returns><c>LocalSpace</c> position</returns>
        public Vector3 IndexToLocal(Vector2Int index)
        {
            Assert.IsTrue(index.x >= 0 && index.y >= 0
                && index.x < size.x && index.y < size.y, "Index Out Of Bounds!");

            Vector2 local2D = alignment switch
            {
                Alignment.TopLeft => new Vector2(
                    index.x * cellSize + cellSize / 2,
                    -size.y * cellSize + index.y * cellSize + cellSize / 2),

                Alignment.TopRight => new Vector2(
                    -size.x * cellSize + index.x * cellSize + cellSize / 2,
                    -size.y * cellSize + index.y * cellSize + cellSize / 2),

                Alignment.BottomLeft => new Vector2(
                    index.x * cellSize + cellSize / 2,
                    index.y * cellSize + cellSize / 2),

                Alignment.BottomRight => new Vector2(
                    -size.x * cellSize + index.x * cellSize + cellSize / 2,
                    index.y * cellSize + cellSize / 2),

                Alignment.Center => new Vector2(
                    (index.x - (size.x - 1) / 2f) * cellSize,
                    (index.y - (size.y - 1) / 2f) * cellSize),

                _ => Vector2.zero
            };

            return space switch
            {
                Space.Horizontal => new Vector3(local2D.x, 0, local2D.y),
                Space.Vertical => new Vector3(local2D.x, local2D.y, 0),
                _ => Vector3.zero
            };
        }















        /// <summary>
        /// Method to get cell from <paramref name="cell"/> and <paramref name="indexDelta"/>
        /// </summary>
        /// <remarks>
        /// For example, (1,0) will return the right neighbor<br/>
        /// and (2,0) will return the right second neighbor.
        /// </remarks>
        /// <param name="cell">Origin cell</param>
        /// <param name="indexDelta">index delta from origin cell to neighbor.
        /// </param>
        /// <returns>Neighbor cell from <paramref name="cell"/> plus <paramref name="indexDelta"/></returns>
        public Cell GetNeighbor(Cell cell, Vector2Int indexDelta)
        {
            return this[cell.index.x + indexDelta.x, cell.index.y + indexDelta.y];
        }

        /// <summary>
        /// Method to get all neighbors surrounding <paramref name="cell"/>
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>
        /// Maximum 8 neighbors and ignore null neighbors
        /// </returns>
        public HashSet<Cell> GetNeighbors(Cell cell)
        {
            HashSet<Cell> neighbors = new HashSet<Cell>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue; // Skip cell itself

                    Cell neighbor = GetNeighbor(cell, new Vector2Int(x, y));

                    if (neighbor != null) // Ignore null neighbors
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }

            return neighbors;
        }

        /// <summary>
        /// Method to get only diagonal neighbors of <paramref name="cell"/>
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>
        /// Maximum 4 neighbors and ignore null neighbors
        /// </returns>
        public HashSet<Cell> GetNeighborsInDiagonal(Cell cell)
        {
            HashSet<Cell> neighbors = new HashSet<Cell>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if ((x == 0 && y == 0) || ((x != 0 && y == 0) || (x == 0 && y != 0))) continue; // Skip cell itself and inline neighbors

                    Cell neighbor = GetNeighbor(cell, new Vector2Int(x, y));

                    if (neighbor != null) // Ignore null neighbors
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }

            return neighbors;
        }

        /// <summary>
        /// Method to get neighbors of <paramref name="cell"/>, ignore diagonally
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>
        /// Maximum 4 neighbors and ignore null neighbors
        /// </returns>
        public HashSet<Cell> GetNeighborsIgnoreDiagonal(Cell cell)
        {
            HashSet<Cell> neighbors = new HashSet<Cell>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if ((x == 0 && y == 0) || (x != 0 && y != 0)) continue; // Skip cell itself and diagonal neighbors

                    Cell neighbor = GetNeighbor(cell, new Vector2Int(x, y));

                    if (neighbor != null) // Ignore null neighbors
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }

            return neighbors;
        }




        private void OnDrawGizmos()
        {
            if (!gameObject.activeInHierarchy) return;
            DrawFrame();
#if UNITY_EDITOR
            foreach (var cell in _cellMap)
            {
                if (cell == null) continue;
                Handles.Label(cell.transform.position, $"{cell.index.x}, {cell.index.y}");
            }
#endif
        }

        
        private void DrawFrame()
        {
            // Row line
            for (int y = 0; y <= size.y; y++)
            {
                Vector3 step = space switch
                {
                    Space.Horizontal => transform.forward,
                    Space.Vertical => transform.up,
                    _ => Vector3.zero
                };

                Vector3 alignStep = space switch
                {
                    Space.Horizontal => -transform.forward,
                    Space.Vertical => -transform.up,
                    _ => Vector3.zero
                };

                Vector3 offset = alignment switch
                {
                    Alignment.BottomLeft => Vector3.zero,
                    Alignment.BottomRight => -transform.right * size.x,
                    Alignment.TopLeft => alignStep * size.y,
                    Alignment.TopRight => -transform.right * size.x + alignStep * size.y,
                    Alignment.Center => (-transform.right * size.x + alignStep * size.y) / 2f,
                    _ => Vector3.zero
                };

                Vector3 start = transform.position + (y * step + offset) * cellSize;
                Vector3 end = start + transform.right * size.x * cellSize;

                Gizmos.DrawLine(start, end);
            }


            // Column line
            for (int x = 0; x <= size.x; x++)
            {
                Vector3 step = space switch
                {
                    Space.Horizontal => transform.forward,
                    Space.Vertical => transform.up,
                    _ => Vector3.zero
                };

                Vector3 offsetY = space switch
                {
                    Space.Horizontal => -transform.forward * size.y,
                    Space.Vertical => -transform.up * size.y,
                    _ => Vector3.zero
                };

                Vector3 offset = alignment switch
                {
                    Alignment.BottomLeft => Vector3.zero,
                    Alignment.BottomRight => -transform.right * size.x,
                    Alignment.TopLeft => offsetY,
                    Alignment.TopRight => -transform.right * size.x + offsetY,
                    Alignment.Center => (-transform.right * size.x + offsetY) / 2f,
                    _ => Vector3.zero
                };

                Vector3 start = transform.position + (transform.right * x + offset) * cellSize;
                Vector3 end = start + step * size.y * cellSize;

                Gizmos.DrawLine(start, end);
            }
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            for (int i = 0; i < _cellMap.Length; i++)
            {
                yield return _cellMap[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

