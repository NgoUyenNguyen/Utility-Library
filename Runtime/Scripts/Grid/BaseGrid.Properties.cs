using UnityEngine;

namespace NgoUyenNguyen.Grid
{
    public partial class BaseGrid
    {
        /// <summary>
        /// Prefab of <c>Cell</c> spawned in <c>Grid</c>
        /// </summary>
        public Cell CellPrefab
        {
            get => _cellPrefab;
            set
            {
                if (value == null) return;
                _cellPrefab = value;
            }
        }

        /// <summary>
        /// Size of <c>Grid</c>
        /// </summary>
        public Vector2Int Size
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
        public float CellSize
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
        public GridAlignment Alignment
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
        public GridSpace Space
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
        public CellLayout Layout
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
        public Cell[] CellMap
        {
            get => _cellMap;
            set
            {
                if (value == null) return;
                _cellMap = value;
                CalculateCellsPosition();
            }
        }
    }
}