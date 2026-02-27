using UnityEngine;
using ZLinq;

namespace NgoUyenNguyen.Grid
{
    public partial class BaseGrid
    {
        /// <summary>
        /// Method to get <c>LocalSpace</c> position from <c>Cell</c>
        /// </summary>
        /// <param name="cell"></param>
        /// <returns><c>LocalSpace</c> position</returns>
        public Vector3? CellToLocal(Cell cell)
        {
            if (!CellMap.AsValueEnumerable().Contains(cell)) return null;
            
            return IndexToLocal(cell.index);
        }

        /// <summary>
        /// Method to get <c>WolrdSpace</c> position from <c>Cell</c>
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>WorldSpace position</returns>
        public Vector3? CellToWorld(Cell cell)
        {
            var localPos = CellToLocal(cell);
            if (!localPos.HasValue) return null;
            
            return transform.TransformPoint(localPos.Value);
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
            var xIndex = Mathf.Clamp(index.x, 0, Size.x - 1);
            var yIndex = Mathf.Clamp(index.y, 0, Size.y - 1);

            if (_cellMap[xIndex + yIndex * Size.x] != null) return _cellMap[xIndex + yIndex * Size.x];

            var maxRadius = Mathf.Max(Size.x, Size.y);
            for (var radius = 1; radius <= maxRadius; radius++)
            {
                var ring = GetRing<Cell>(IndexToAxial(index), radius);

                var minDist = float.MaxValue;
                Cell nearestCell = null;
                foreach (var cell in ring)
                {
                    var dist = Vector3.Distance(localPos, cell.transform.localPosition);

                    if (!(dist < minDist)) continue;
                    minDist = dist;
                    nearestCell = cell;
                }

                if (nearestCell != null) return nearestCell;
            }

            return null;
        }

        private Cell Square_LocalToCell(Vector3 localPos)
        {
            var index = LocalToIndex(localPos);
            var xIndex = Mathf.Clamp(index.x, 0, Size.x - 1);
            var yIndex = Mathf.Clamp(index.y, 0, Size.y - 1);

            if (_cellMap[xIndex + yIndex * Size.x] != null) return _cellMap[xIndex + yIndex * Size.x];

            var maxRadius = Mathf.Max(Size.x, Size.y);
            for (var radius = 1; radius <= maxRadius; radius++)
            {
                var ring = GetRing<Cell>(index, radius);

                var minDist = float.MaxValue;
                Cell nearestCell = null;
                foreach (var cell in ring)
                {
                    var dist = Vector3.Distance(localPos, cell.transform.localPosition);

                    if (!(dist < minDist)) continue;
                    minDist = dist;
                    nearestCell = cell;
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
            var xPos = localPos.x;
            var yPos = Space switch
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
            switch (Alignment)
            {
                case GridAlignment.BottomLeft:
                    xIndex = Mathf.FloorToInt(xPos / CellSize);
                    yIndex = Mathf.FloorToInt(yPos / CellSize);
                    break;

                case GridAlignment.BottomRight:
                    xIndex = (Size.x - 1) - Mathf.FloorToInt(xPos / CellSize);
                    yIndex = Mathf.FloorToInt(yPos / CellSize);
                    break;

                case GridAlignment.TopLeft:
                    xIndex = Mathf.FloorToInt(xPos / CellSize);
                    yIndex = (Size.y - 1) - Mathf.FloorToInt(yPos / CellSize);
                    break;

                case GridAlignment.TopRight:
                    xIndex = (Size.x - 1) - Mathf.FloorToInt(xPos / CellSize);
                    yIndex = (Size.y - 1) - Mathf.FloorToInt(yPos / CellSize);
                    break;

                case GridAlignment.Center:
                    xIndex = Mathf.FloorToInt((xPos + (Size.x * CellSize) / 2f) / CellSize);
                    yIndex = Mathf.FloorToInt((yPos + (Size.y * CellSize) / 2f) / CellSize);
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
            var local2D = _layout switch
            {
                CellLayout.Square => Square_IndexToLocal(index),
                CellLayout.Hexagon => Hexagon_AxialToLocal(index),
                _ => Vector2.zero
            };

            return Space switch
            {
                GridSpace.Horizontal => new Vector3(local2D.x, 0, local2D.y),
                GridSpace.Vertical => new Vector3(local2D.x, local2D.y, 0),
                _ => Vector3.zero
            };
        }

        private Vector2 Hexagon_AxialToLocal(Vector2Int axialIndex)
        {
            var horizStep = Mathf.Sqrt(3) * _cellSize / 2;
            
            var xPos = horizStep / 2 +
                       (Mathf.Sqrt(3) * axialIndex.x + (Mathf.Sqrt(3) / 2) * axialIndex.y) * _cellSize / 2;
            var yPos = _cellSize / 2 + 1.5f * axialIndex.y * _cellSize / 2;

            var xDelta = (_size.x + .5f) * horizStep;
            var yDelta = Mathf.Ceil(_size.y / 2f) * CellSize + Mathf.Floor(_size.y / 2f) * _cellSize / 2;

            return Alignment switch
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
            return Alignment switch
            {
                GridAlignment.TopLeft => new Vector2(
                    index.x * CellSize + CellSize / 2,
                    -Size.y * CellSize + index.y * CellSize + CellSize / 2),

                GridAlignment.TopRight => new Vector2(
                    -Size.x * CellSize + index.x * CellSize + CellSize / 2,
                    -Size.y * CellSize + index.y * CellSize + CellSize / 2),

                GridAlignment.BottomLeft => new Vector2(
                    index.x * CellSize + CellSize / 2,
                    index.y * CellSize + CellSize / 2),

                GridAlignment.BottomRight => new Vector2(
                    -Size.x * CellSize + index.x * CellSize + CellSize / 2,
                    index.y * CellSize + CellSize / 2),

                GridAlignment.Center => new Vector2(
                    (index.x - (Size.x - 1) / 2f) * CellSize,
                    (index.y - (Size.y - 1) / 2f) * CellSize),

                _ => Vector2.zero
            };
        }

        private Vector2Int Hexagon_LocalToIndex(float xPos, float yPos)
        {
            var x = xPos - Mathf.Sqrt(3) * _cellSize / 4;
            var y = yPos - _cellSize / 2;
            var xDelta = (_size.x + .5f) * Mathf.Sqrt(3) * _cellSize / 2;
            var yDelta = Mathf.Ceil(_size.y / 2f) * CellSize + Mathf.Floor(_size.y / 2f) * _cellSize / 2;

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

            var q = (Mathf.Sqrt(3f) / 3f * x - 1f / 3f * y) / (_cellSize / 2f);
            var r = (2f / 3f * y) / (_cellSize / 2f);

            return RoundAxial(new Vector2(q, r));
        }

        private static Vector2Int RoundAxial(Vector2 axialPos)
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
    }
}