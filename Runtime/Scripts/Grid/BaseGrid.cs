using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NgoUyenNguyen.Grid
{
    /// <summary>
    /// Represents a grid structure composed of cells, supporting various alignment and spacing options.
    /// </summary>
    /// <remarks>The <see cref="BaseGrid"/> class provides functionality for creating, managing, and
    /// interacting with a grid of cells. Likely a collection, 
    /// <see cref="BaseGrid"/> can access cells through index like a 2D array and iterated by foreach loop</remarks>
    public abstract class BaseGrid : MonoBehaviour
    {
        #region Fields

#if UNITY_EDITOR
        [HideInInspector] public bool prefabInitialized;
#endif


        [SerializeField, HideInInspector] 
        private Cell[] _cellMap = Array.Empty<Cell>();

        [SerializeField, HideInInspector, Tooltip("Prefab to spawn in Grid")]
        private Cell _cellPrefab;

        [SerializeField, HideInInspector, Tooltip("Size of the Grid")]
        private Vector2Int _size;

        [SerializeField, HideInInspector, Tooltip("Size of each Cell")]
        private float _cellSize = 1;

        [SerializeField, HideInInspector, Tooltip("Define relative position of each Cell to the Grid")]
        private GridAlignment _alignment;

        [SerializeField, HideInInspector, Tooltip("Space to which the Grid belongs")]
        private GridSpace _space;

        [SerializeField, HideInInspector, Tooltip("Layout of each Cell")]
        private CellLayout _layout;

        #endregion

        #region Properties

        /// <summary>
        /// Prefab of <c>Cell</c> spawned in <c>Grid</c>
        /// </summary>
        public Cell cellPrefab
        {
            get => _cellPrefab;
            private set => _cellPrefab = value;
        }

        /// <summary>
        /// Size of <c>Grid</c>
        /// </summary>
        public Vector2Int size
        {
            get => _size;
            set
            {
                _size = value;
                CalculateCellsPosition();
            }
        }

        /// <summary>
        /// Size of each <c>Cell</c> in <c>Grid</c>
        /// </summary>
        public float cellSize
        {
            get => _cellSize;
            set
            {
                _cellSize = value;
                CalculateCellsPosition();
            }
        }

        /// <summary>
        /// Alignment of <c>Grid</c>
        /// </summary>
        /// <remarks>
        /// Define where <c>Cells</c> are spawned from <c>Grid</c> position
        /// </remarks>
        public GridAlignment alignment
        {
            get => _alignment;
            set
            {
                _alignment = value;
                CalculateCellsPosition();
            }
        }

        /// <summary>
        /// Coordinate space in which the operation is performed.
        /// </summary>
        public GridSpace space
        {
            get => _space;
            set
            {
                _space = value;
                CalculateCellsPosition();
            }
        }

        /// <summary>
        /// Layout of <c>Grid</c>
        /// </summary>
        public CellLayout layout
        {
            get => _layout;
            set
            {
                RecalculateCellsIndex(value);
                _layout = value;
                CalculateCellsPosition();
            }
        }

        /// <summary>
        /// Represents the collection of <c>Cells</c> in the <c>Grid</c>
        /// </summary>
        /// <remarks>This array holds all references to <c>Cells</c> in <c>Grid</c>.</remarks>
        public Cell[] cellMap => _cellMap;
        #endregion

        #region Indexer

        /// <summary>
        /// Access <c>Cell</c> in <c>Grid</c> through (<paramref name="x"/>, <paramref name="y"/>) coordinate
        /// </summary>
        /// <param name="x">X index position</param>
        /// <param name="y">Y index position</param>
        /// <returns><see cref="Cell"/> at <paramref name="x"/>, <paramref name="y"/></returns>
        public Cell this[int x, int y]
        {
            get
            {
                switch (_layout)
                {
                    case CellLayout.Square:
                        if (x < 0 ||
                            x >= size.x ||
                            y < 0 ||
                            y >= size.y)
                        {
                            throw new IndexOutOfRangeException();
                        }
                        
                        return _cellMap[size.x * y + x];
                    case CellLayout.Hexagon:
                        var index = AxialToIndex(new Vector2Int(x, y));
                        if (index.x < 0 ||
                            index.x >= size.x ||
                            index.y < 0 ||
                            index.y >= size.y)
                        {
                            throw new IndexOutOfRangeException();
                        }

                        return _cellMap[index.x + index.y * size.x];
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Access <c>Cell</c> in <c>Grid</c> through index
        /// </summary>
        /// <param name="index">Index position of <c>Cell</c></param>
        /// <returns>Cell at <paramref name="index"/></returns>
        public Cell this[Vector2Int index] => this[index.x, index.y];

        #endregion

        #region Creator

        /// <summary>
        /// Method to create <c>Grid</c>
        /// </summary>
        /// <param name="width">Width of grid</param>
        /// <param name="height">Height of grid</param>
        /// <param name="cell">Cell Prefab to be spawned</param>
        public void Create(int width, int height, GameObject cell = null)
        {
            if (cell != null && !cell.TryGetComponent<Cell>(out _))
            {
                throw new ArgumentException("Prefab does not have Cell component!", nameof(cell));
            }

            DestroyOldCells();

            // Set Size
            _size = new Vector2Int(width, height);
            _cellMap = new Cell[size.x * size.y];

            // Set Cell Prefab
            if (cell != null)
            {
                cellPrefab = cell.GetComponent<Cell>();
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
        /// <param name="cell">Cell Prefab to be spawned</param>
        public void Create(bool[,] map, GameObject cell = null)
        {
            if (cell != null && !cell.TryGetComponent<Cell>(out _))
            {
                throw new ArgumentException("Prefab does not have Cell component!", nameof(cell));
            }

            DestroyOldCells();

            //Set Size
            _size = new Vector2Int(map.GetLength(0), map.GetLength(1));
            _cellMap = new Cell[size.x * size.y];

            // Set Cell Prefab
            if (cell != null)
            {
                cellPrefab = cell.GetComponent<Cell>();
            }

            CellSpawn(cellPrefab, map);
        }

        /// <summary>
        /// Method to create <c>Grid</c>
        /// </summary>
        /// <param name="size">Size of grid</param>
        /// <param name="cell"><c>Cell</c> to be spawned</param>
        public void Create(Vector2Int size, GameObject cell = null)
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
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    if (map != null && !map[x, y]) continue;
                    _cellMap[x + y * size.x] = Instantiate(cellPrefab.gameObject, transform).GetComponent<Cell>();
                    _cellMap[x + y * size.x].grid = this;
                    AsignCellIndex(this[x, y], new Vector2Int(x, y));
                    _cellMap[x + y * size.x].transform.localPosition = CellToLocal(this[x, y]);
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
                cell.transform.localPosition = CellToLocal(cell);
            }
        }

        private void AsignCellIndex(Cell cell, Vector2Int index)
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
            if (layout == newLayout) return;
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
                throw new InvalidOperationException("Can Not Create Prefab In Runtime!");
            }
            else if (!cellPrefab.TryGetComponent<Cell>(out _))
            {
                throw new ArgumentException("Prefab does not have Cell component!", nameof(cellPrefab));
            }
            else
            {
                DestroyOldCellPrefabs();

                // Set Size
                this._size = size;
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
                throw new InvalidOperationException("Can Not Create Prefab In Runtime!");
            }

            if (!cellPrefab.TryGetComponent<Cell>(out _))
            {
                throw new ArgumentException("Prefab does not have Cell component!", nameof(cellPrefab));
            }

            DestroyOldCellPrefabs();

            // Set Size
            _size = new Vector2Int(map.GetLength(0), map.GetLength(1));
            _cellMap = new Cell[size.x * size.y];

            CellPrefabSpawn(cellPrefab, map);
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
            this.cellPrefab = cellPrefab.GetComponent<Cell>();

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    if (map != null && !map[x, y]) continue;
                    _cellMap[x + y * size.x] = (PrefabUtility.InstantiatePrefab(cellPrefab, transform) as GameObject)
                        .GetComponent<Cell>();
                    _cellMap[x + y * size.x].grid = this;
                    AsignCellIndex(_cellMap[x + y * size.x], new Vector2Int(x, y));
                    _cellMap[x + y * size.x].transform.localPosition = CellToLocal(_cellMap[x + y * size.x]);
                }
            }
        }

        public Cell SpawnCellPrefab(Cell cellPrefab, Vector2Int index)
        {
            if (cellPrefab == null) return null;
            if (_cellMap[index.x + index.y * size.x] != null)
                DestroyImmediate(_cellMap[index.x + index.y * size.x].gameObject);

            Cell cell = (PrefabUtility.InstantiatePrefab(cellPrefab.gameObject, transform) as GameObject)
                .GetComponent<Cell>();
            cell.index = _layout switch
            {
                CellLayout.Hexagon => IndexToAxial(index),
                _ => index
            };
            cell.grid = this;
            _cellMap[index.x + index.y * size.x] = cell;

            return cell;
        }
#endif

        #endregion

        #region Convert API

        /// <summary>
        /// Method to get <c>LocalSpace</c> position from <c>Cell</c>
        /// </summary>
        /// <param name="cell"></param>
        /// <returns><c>LocalSpace</c> position</returns>
        public Vector3 CellToLocal(Cell cell)
        {
            if (!cellMap.Contains(cell))
            {
                throw new ArgumentException(
                    $"{name} does not contain {cell.name}", nameof(cell));
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
        /// <param name="localPos"><c>Local Space</c> position</param>
        /// <returns>Nearest <c>Cell</c></returns>
        public Cell LocalToCell(Vector3 localPos)
        {
            return _layout switch
            {
                CellLayout.Square => Square_LocalToCell(localPos),
                CellLayout.Hexagon => Hexagon_LocalToCell(localPos),
                _ => null
            };
        }

        private Cell Hexagon_LocalToCell(Vector3 localPos)
        {
            var index = AxialToIndex(LocalToIndex(localPos));
            var xIndex = Mathf.Clamp(index.x, 0, size.x - 1);
            var yIndex = Mathf.Clamp(index.y, 0, size.y - 1);

            if (_cellMap[xIndex + yIndex * size.x] != null) return _cellMap[xIndex + yIndex * size.x];

            int maxRadius = Mathf.Max(size.x, size.y);
            for (int radius = 1; radius <= maxRadius; radius++)
            {
                var ring = GetRing<Cell>(IndexToAxial(index), radius);
                if (ring.Count == 0) continue;

                float minDist = float.MaxValue;
                Cell nearestCell = null;
                foreach (var cell in ring)
                {
                    float dist = Vector3.Distance(localPos, cell.transform.localPosition);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearestCell = cell;
                    }
                }

                if (nearestCell != null) return nearestCell;
            }

            return null;
        }

        private Cell Square_LocalToCell(Vector3 localPos)
        {
            var index = LocalToIndex(localPos);
            var xIndex = Mathf.Clamp(index.x, 0, size.x - 1);
            var yIndex = Mathf.Clamp(index.y, 0, size.y - 1);

            if (_cellMap[xIndex + yIndex * size.x] != null) return _cellMap[xIndex + yIndex * size.x];

            int maxRadius = Mathf.Max(size.x, size.y);
            for (int radius = 1; radius <= maxRadius; radius++)
            {
                var ring = GetRing<Cell>(index, radius);
                if (ring.Count == 0) continue;

                float minDist = float.MaxValue;
                Cell nearestCell = null;
                foreach (var cell in ring)
                {
                    float dist = Vector3.Distance(localPos, cell.transform.localPosition);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearestCell = cell;
                    }
                }

                if (nearestCell != null) return nearestCell;
            }

            return null;
        }

        /// <summary>
        /// Method to get <c>Cell</c> from <c>WorldSpace</c> position
        /// </summary>
        /// <remarks>
        /// If input position is out of bounds, return the nearest <c>Cell</c>
        /// </remarks>
        /// <param name="worldPos"><c>World Space</c> position</param>
        /// <returns>Nearest <c>Cell</c></returns>
        public Cell WorldToCell(Vector3 worldPos)
        {
            return LocalToCell(transform.InverseTransformPoint(worldPos));
        }

        /// <summary>
        /// Method to get <c>Index</c> position from <c>WorldSpace</c> position
        /// </summary>
        /// <param name="worldPos"><c>World Space</c> position</param>
        /// <returns><c>Index</c> position</returns>
        public Vector2Int WorldToIndex(Vector3 worldPos)
        {
            return LocalToIndex(transform.InverseTransformPoint(worldPos));
        }

        /// <summary>
        /// Method to get <c>Index</c> position from <c>LocalSpace</c> position
        /// </summary>
        /// <param name="localPos"><c>Local Space</c> position</param>
        /// <returns><c>Index</c> position</returns>
        public Vector2Int LocalToIndex(Vector3 localPos)
        {
            float xPos = localPos.x;
            float yPos = space switch
            {
                GridSpace.Horizontal => localPos.z,
                GridSpace.Vertical => localPos.y,
                _ => 0
            };

            return _layout switch
            {
                CellLayout.Square => Square_LocalToIndex(xPos, yPos),
                CellLayout.Hexagon => Hexagon_LocalToIndex(xPos, yPos),
                _ => default
            };
        }

        private Vector2Int Square_LocalToIndex(float xPos, float yPos)
        {
            int xIndex = 0, yIndex = 0;
            switch (alignment)
            {
                case GridAlignment.BottomLeft:
                    xIndex = Mathf.FloorToInt(xPos / cellSize);
                    yIndex = Mathf.FloorToInt(yPos / cellSize);
                    break;

                case GridAlignment.BottomRight:
                    xIndex = (size.x - 1) - Mathf.FloorToInt(xPos / cellSize);
                    yIndex = Mathf.FloorToInt(yPos / cellSize);
                    break;

                case GridAlignment.TopLeft:
                    xIndex = Mathf.FloorToInt(xPos / cellSize);
                    yIndex = (size.y - 1) - Mathf.FloorToInt(yPos / cellSize);
                    break;

                case GridAlignment.TopRight:
                    xIndex = (size.x - 1) - Mathf.FloorToInt(xPos / cellSize);
                    yIndex = (size.y - 1) - Mathf.FloorToInt(yPos / cellSize);
                    break;

                case GridAlignment.Center:
                    xIndex = Mathf.FloorToInt((xPos + (size.x * cellSize) / 2f) / cellSize);
                    yIndex = Mathf.FloorToInt((yPos + (size.y * cellSize) / 2f) / cellSize);
                    break;
            }

            return new Vector2Int(xIndex, yIndex);
        }

        /// <summary>
        /// Method to get <c>WorldSpace</c> position from <c>Index</c>
        /// </summary>
        /// <param name="index"><c>Index</c> position</param>
        /// <returns><c>WorldSpace</c> position</returns>
        public Vector3 IndexToWorld(Vector2Int index)
        {
            return transform.TransformPoint(IndexToLocal(index));
        }

        /// <summary>
        /// Method to get <c>LocalSpace</c> position from <c>Index</c>
        /// </summary>
        /// <param name="index"><c>Index</c> position</param>
        /// <returns><c>LocalSpace</c> position</returns>
        public Vector3 IndexToLocal(Vector2Int index)
        {
            Vector2 local2D = _layout switch
            {
                CellLayout.Square => Square_IndexToLocal(index),
                CellLayout.Hexagon => Hexagon_AxialToLocal(index),
                _ => Vector2.zero
            };

            return space switch
            {
                GridSpace.Horizontal => new Vector3(local2D.x, 0, local2D.y),
                GridSpace.Vertical => new Vector3(local2D.x, local2D.y, 0),
                _ => Vector3.zero
            };
        }

        private Vector2 Hexagon_AxialToLocal(Vector2Int axialIndex)
        {
            float horizStep = Mathf.Sqrt(3) * _cellSize / 2;
            
            float xPos = horizStep / 2 +
                         (Mathf.Sqrt(3) * axialIndex.x + (Mathf.Sqrt(3) / 2) * axialIndex.y) * _cellSize / 2;
            float yPos = _cellSize / 2 + 1.5f * axialIndex.y * _cellSize / 2;

            float xDelta = (_size.x + .5f) * horizStep;
            float yDelta = Mathf.Ceil(_size.y / 2f) * cellSize + Mathf.Floor(_size.y / 2f) * _cellSize / 2;

            return alignment switch
            {
                GridAlignment.BottomRight => new Vector2(xPos - xDelta, yPos),
                GridAlignment.TopLeft => new Vector2(xPos, yPos - yDelta),
                GridAlignment.TopRight => new Vector2(xPos - xDelta, yPos - yDelta),
                GridAlignment.Center => new Vector2(xPos - xDelta / 2, yPos - yDelta / 2),
                _ => new Vector2(xPos, yPos)
            };
        }

        private Vector2 Square_IndexToLocal(Vector2Int index)
        {
            return alignment switch
            {
                GridAlignment.TopLeft => new Vector2(
                    index.x * cellSize + cellSize / 2,
                    -size.y * cellSize + index.y * cellSize + cellSize / 2),

                GridAlignment.TopRight => new Vector2(
                    -size.x * cellSize + index.x * cellSize + cellSize / 2,
                    -size.y * cellSize + index.y * cellSize + cellSize / 2),

                GridAlignment.BottomLeft => new Vector2(
                    index.x * cellSize + cellSize / 2,
                    index.y * cellSize + cellSize / 2),

                GridAlignment.BottomRight => new Vector2(
                    -size.x * cellSize + index.x * cellSize + cellSize / 2,
                    index.y * cellSize + cellSize / 2),

                GridAlignment.Center => new Vector2(
                    (index.x - (size.x - 1) / 2f) * cellSize,
                    (index.y - (size.y - 1) / 2f) * cellSize),

                _ => Vector2.zero
            };
        }

        private Vector2Int Hexagon_LocalToIndex(float xPos, float yPos)
        {
            float x = xPos - Mathf.Sqrt(3) * _cellSize / 4;
            float y = yPos - _cellSize / 2;
            float xDelta = (_size.x + .5f) * Mathf.Sqrt(3) * _cellSize / 2;
            float yDelta = Mathf.Ceil(_size.y / 2f) * cellSize + Mathf.Floor(_size.y / 2f) * _cellSize / 2;

            switch (_alignment)
            {
                case GridAlignment.BottomRight:
                    x += xDelta;
                    break;
                case GridAlignment.TopLeft:
                    y += yDelta;
                    break;
                case GridAlignment.TopRight:
                    x += xDelta;
                    y += yDelta;
                    break;
                case GridAlignment.Center:
                    x += xDelta / 2;
                    y += yDelta / 2;
                    break;
            }

            float q = (Mathf.Sqrt(3f) / 3f * x - 1f / 3f * y) / (_cellSize / 2f);
            float r = (2f / 3f * y) / (_cellSize / 2f);

            return RoundAxial(new Vector2(q, r));
        }

        private Vector2Int RoundAxial(Vector2 axialPos)
        {
            var q = Mathf.Round(axialPos.x);
            var r = Mathf.Round(axialPos.y);
            var s = Mathf.Round(-axialPos.x - axialPos.y);

            var qDiff = Mathf.Abs(q - axialPos.x);
            var rDiff = Mathf.Abs(r - axialPos.y);
            var sDiff = Mathf.Abs(s + axialPos.x + axialPos.y);

            if (qDiff > rDiff && qDiff > sDiff) q = -r - s;
            else if (rDiff > sDiff) r = -q - s;

            return new Vector2Int((int)q, (int)r);
        }

        public Vector2Int IndexToAxial(Vector2Int index)
        {
            var parity = index.y & 1;
            var q = index.x - (index.y - parity) / 2;
            return new Vector2Int(q, index.y);
        }

        public Vector2Int AxialToIndex(Vector2Int axial)
        {
            var parity = axial.y & 1;
            var col = axial.x + (axial.y - parity) / 2;
            return new Vector2Int(col, axial.y);
        }

        #endregion

        #region Neighbor API

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
        public T GetNeighbor<T>(T cell, Vector2Int indexDelta) where T : Cell
        {
            if (!cellMap.Contains(cell))
            {
                throw new ArgumentException(
                    $"{name} does not contain {cell.name}", nameof(cell));
            }

            var neighborIndex = cell.index + indexDelta;
            if (_layout == CellLayout.Hexagon) neighborIndex = AxialToIndex(neighborIndex);
            if (neighborIndex.x < 0 ||
                neighborIndex.x >= size.x ||
                neighborIndex.y < 0 ||
                neighborIndex.y >= size.y)
            {
                return null;
            }

            return _cellMap[neighborIndex.x + neighborIndex.y * size.x] as T;
        }


        /// <summary>
        /// Method to get neighbors surrounding <paramref name="cell"/>
        /// </summary>
        /// <param name="cell">Origin <c>Cell</c></param>
        /// <param name="filter">
        /// Filter mode:
        /// <list type="bullet">
        /// <item><see cref="NeighborFilter.None"/> = All neighbors</item>
        /// <item><see cref="NeighborFilter.DiagonalOnly"/> = Only choose <c>Diagonal</c> neighbors</item>
        /// <item><see cref="NeighborFilter.OrthogonalOnly"/> = Only choose <c>Orthogonal</c> neighbors</item>
        /// </list>
        /// </param>
        /// <remarks>
        /// <c>NOTICE: filter only works with Square Grid. Otherwise, it always returns all neighbors</c>
        /// </remarks>
        /// <returns><see cref="HashSet{T}"/> of neighbors</returns>
        public HashSet<T> GetNeighbors<T>(T cell, NeighborFilter filter) where T : Cell
        {
            if (!cellMap.Contains(cell))
            {
                throw new ArgumentException(
                    $"{name} does not contain {cell.name}", nameof(cell));
            }

            switch (_layout)
            {
                case CellLayout.Square:
                    return filter switch
                    {
                        NeighborFilter.OrthogonalOnly => Square_GetNeighborsOrthogonal(cell),
                        NeighborFilter.DiagonalOnly => Square_GetNeighborsDiagonal(cell),
                        _ => Square_GetAllNeighbors(cell)
                    };
                case CellLayout.Hexagon:
                    return Hexagon_GetNeighbors(cell);
            }

            return null;
        }

        /// <summary>
        /// Method to get neighbors surrounding <paramref name="cell"/>
        /// </summary>
        /// <param name="cell">Origin <c>Cell</c></param>
        /// <returns><see cref="HashSet{T}"/> of neighbors</returns>
        public HashSet<T> GetNeighbors<T>(T cell) where T : Cell
        {
            return GetNeighbors(cell, NeighborFilter.None);
        }

        /// <summary>
        /// Method to get all cells in a ring around one cell
        /// </summary>
        /// <param name="cell">Center <c>Cell</c> of the ring </param>
        /// <param name="radius">Radius of the ring</param>
        /// <returns><see cref="HashSet{T}"/> Ring</returns>
        public HashSet<T> GetRing<T>(T cell, int radius) where T : Cell
        {
            if (!cellMap.Contains(cell))
            {
                throw new ArgumentException(
                    $"{name} does not contain {cell.name}", nameof(cell));
            }

            return GetRing<T>(cell.index, radius);
        }

        /// <summary>
        /// Method to get all cells in a ring around one index
        /// </summary>
        /// <param name="center">Center index of the ring</param>
        /// <param name="radius">Radius of the ring</param>
        /// <returns><see cref="HashSet{T}"/> Ring</returns>
        public HashSet<T> GetRing<T>(Vector2Int center, int radius) where T : Cell
        {
            return _layout switch
            {
                CellLayout.Square => Square_GetRing<T>(center, radius),
                CellLayout.Hexagon => Hexagon_GetRing<T>(center, radius),
                _ => null
            };
        }

        #region Square

        private HashSet<T> Square_GetRing<T>(Vector2Int center, int radius) where T : Cell
        {
            var ring = new HashSet<T>();
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius) continue;

                    var neighborIndex = center + new Vector2Int(x, y);
                    var neighbor = default(T);
                    if (neighborIndex.x >= 0 &&
                        neighborIndex.x < _size.x &&
                        neighborIndex.y >= 0 &&
                        neighborIndex.y < _size.y)
                    {
                        neighbor = _cellMap[neighborIndex.x + neighborIndex.y * _size.x] as T;
                    }

                    if (neighbor != null)
                    {
                        ring.Add(neighbor);
                    }
                }
            }

            return ring;
        }

        private HashSet<T> Square_GetAllNeighbors<T>(T cell) where T : Cell
        {
            HashSet<T> neighbors = new();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue; // Skip cell itself

                    Cell neighbor = GetNeighbor(cell, new Vector2Int(x, y));

                    if (neighbor != null) // Ignore null neighbors
                    {
                        neighbors.Add((T)neighbor);
                    }
                }
            }

            return neighbors;
        }

        private HashSet<T> Square_GetNeighborsDiagonal<T>(T cell) where T : Cell
        {
            HashSet<T> neighbors = new();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if ((x == 0 && y == 0) || (x != 0 && y == 0) || (x == 0 && y != 0))
                        continue; // Skip cell itself and inline neighbors

                    Cell neighbor = GetNeighbor(cell, new Vector2Int(x, y));

                    if (neighbor != null) // Ignore null neighbors
                    {
                        neighbors.Add((T)neighbor);
                    }
                }
            }

            return neighbors;
        }

        private HashSet<T> Square_GetNeighborsOrthogonal<T>(T cell) where T : Cell
        {
            HashSet<T> neighbors = new();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if ((x == 0 && y == 0) || (x != 0 && y != 0)) continue; // Skip cell itself and diagonal neighbors

                    Cell neighbor = GetNeighbor(cell, new Vector2Int(x, y));

                    if (neighbor != null) // Ignore null neighbors
                    {
                        neighbors.Add((T)neighbor);
                    }
                }
            }

            return neighbors;
        }

        #endregion

        #region Hexagon

        public HashSet<T> Hexagon_GetNeighbors<T>(T cell) where T : Cell
        {
            HashSet<T> neighbors = new();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == y) continue;
                    Cell neighbor = GetNeighbor(cell, new Vector2Int(x, y));

                    if (neighbor != null) // Ignore null neighbors
                    {
                        neighbors.Add((T)neighbor);
                    }
                }
            }

            return neighbors;
        }

        private HashSet<T> Hexagon_GetRing<T>(Vector2Int center, int radius) where T : Cell
        {
            var ring = new HashSet<T>();
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x + y) > radius ||
                        (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius && Mathf.Abs(x + y) != radius))
                        continue;
                    var neighborIndex = AxialToIndex(center + new Vector2Int(x, y));
                    var neighbor = default(T);
                    if (neighborIndex.x >= 0 &&
                        neighborIndex.x < _size.x &&
                        neighborIndex.y >= 0 &&
                        neighborIndex.y < _size.y)
                    {
                        neighbor = _cellMap[neighborIndex.x + neighborIndex.y * _size.x] as T;
                    }

                    if (neighbor != null)
                    {
                        ring.Add(neighbor);
                    }
                }
            }

            return ring;
        }

        #endregion

        #endregion

        #region Other API

        /// <summary>
        /// Combines the meshes of child objects into a single mesh for optimization.
        /// </summary>
        /// <remarks>
        /// This method assumes that child objects do not have submesh.
        /// If child objects have submesh, this method will not work properly.
        /// </remarks>
        /// <param name="material">Optional material to apply to the combined mesh.
        /// If not provided, the material of the first child mesh will be used.
        /// </param>
        public void CombineMesh(Material material = null)
        {
            var meshFilterArray = GetComponentsInChildren<MeshFilter>();
            if (meshFilterArray[0].GetComponent<MeshRenderer>().sharedMaterial == null) return;

            var combineInstances = new CombineInstance[meshFilterArray.Length];
            for (int i = 0; i < meshFilterArray.Length; i++)
            {
                combineInstances[i].mesh = meshFilterArray[i].sharedMesh;
                combineInstances[i].transform =
                    transform.worldToLocalMatrix * meshFilterArray[i].transform.localToWorldMatrix;
                meshFilterArray[i].GetComponent<MeshRenderer>().enabled = false; // Disable cell's meshRenderer
            }

            var combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combineInstances);

            if (!TryGetComponent(out MeshFilter meshFilter))
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            meshFilter.sharedMesh = combinedMesh;

            if (!TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            if (material == null)
                material = meshFilterArray[0].GetComponent<MeshRenderer>().sharedMaterial;
            meshRenderer.sharedMaterial = material;
        }
        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (!gameObject.activeInHierarchy) return;
            DrawFrame();
#if UNITY_EDITOR
            foreach (var cell in _cellMap)
            {
                if (cell == null) continue;
                Handles.Label(cell.transform.position,
                    $"{cell.index.x}, {cell.index.y}",
                    EditorStyles.boldLabel);
            }
#endif
        }


        private void DrawFrame()
        {
            switch (_layout)
            {
                case CellLayout.Square:
                    SquareFrame();
                    break;
                case CellLayout.Hexagon:
                    HexagonFrame();
                    break;
            }
        }

        private void SquareFrame()
        {
            // Row line
            for (int y = 0; y <= size.y; y++)
            {
                Vector3 step = space switch
                {
                    GridSpace.Horizontal => transform.forward,
                    GridSpace.Vertical => transform.up,
                    _ => Vector3.zero
                };

                Vector3 alignStep = space switch
                {
                    GridSpace.Horizontal => -transform.forward,
                    GridSpace.Vertical => -transform.up,
                    _ => Vector3.zero
                };

                Vector3 offset = alignment switch
                {
                    GridAlignment.BottomLeft => Vector3.zero,
                    GridAlignment.BottomRight => -transform.right * size.x,
                    GridAlignment.TopLeft => alignStep * size.y,
                    GridAlignment.TopRight => -transform.right * size.x + alignStep * size.y,
                    GridAlignment.Center => (-transform.right * size.x + alignStep * size.y) / 2f,
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
                    GridSpace.Horizontal => transform.forward,
                    GridSpace.Vertical => transform.up,
                    _ => Vector3.zero
                };

                Vector3 offsetY = space switch
                {
                    GridSpace.Horizontal => -transform.forward * size.y,
                    GridSpace.Vertical => -transform.up * size.y,
                    _ => Vector3.zero
                };

                Vector3 offset = alignment switch
                {
                    GridAlignment.BottomLeft => Vector3.zero,
                    GridAlignment.BottomRight => -transform.right * size.x,
                    GridAlignment.TopLeft => offsetY,
                    GridAlignment.TopRight => -transform.right * size.x + offsetY,
                    GridAlignment.Center => (-transform.right * size.x + offsetY) / 2f,
                    _ => Vector3.zero
                };

                Vector3 start = transform.position + (transform.right * x + offset) * cellSize;
                Vector3 end = start + step * size.y * cellSize;

                Gizmos.DrawLine(start, end);
            }
        }

        private void HexagonFrame()
        {
            Vector3 GetCorner(Vector2 center, int i)
            {
                var angleRad = (60 * i - 30) * Mathf.PI / 180;
                var pos2D = new Vector2(
                    center.x + _cellSize / 2 * Mathf.Cos(angleRad),
                    center.y + _cellSize / 2 * Mathf.Sin(angleRad));
                return _space switch
                {
                    GridSpace.Horizontal => transform.TransformPoint(new Vector3(pos2D.x, 0, pos2D.y)),
                    GridSpace.Vertical => transform.TransformPoint(new Vector3(pos2D.x, pos2D.y, 0)),
                    _ => default
                };
            }

            void DrawHex(Vector2 center)
            {
                for (int i = 0; i < 6; i++)
                {
                    Gizmos.DrawLine(GetCorner(center, i),
                        i < 5 ? GetCorner(center, i + 1) : GetCorner(center, 0));
                }
            }

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    float horizStep = Mathf.Sqrt(3f) * (cellSize / 2f);
                    float vertStep = 1.5f * (cellSize / 2f);

                    float xPos = horizStep * (0.5f + x + 0.5f * (y % 2));
                    float yPos = _cellSize / 2 + vertStep * y;

                    float xDelta = (_size.x + .5f) * Mathf.Sqrt(3) * _cellSize / 2;
                    float yDelta = Mathf.Ceil(_size.y / 2f) * cellSize + Mathf.Floor(_size.y / 2f) * _cellSize / 2;
                    DrawHex(_alignment switch
                    {
                        GridAlignment.BottomRight => new Vector2(xPos - xDelta, yPos),
                        GridAlignment.TopLeft => new Vector2(xPos, yPos - yDelta),
                        GridAlignment.TopRight => new Vector2(xPos - xDelta, yPos - yDelta),
                        GridAlignment.Center => new Vector2(xPos - xDelta / 2, yPos - yDelta / 2),
                        _ => new Vector2(xPos, yPos)
                    });
                }
            }
        }

        #endregion
    }
}