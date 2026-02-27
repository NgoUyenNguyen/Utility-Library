using UnityEngine;

namespace NgoUyenNguyen.Grid
{
    public partial class BaseGrid
    {
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
                            x >= Size.x ||
                            y < 0 ||
                            y >= Size.y)
                        {
                            return null;
                        }
                        
                        return _cellMap[Size.x * y + x];
                    case CellLayout.Hexagon:
                        var index = AxialToIndex(new Vector2Int(x, y));
                        if (index.x < 0 ||
                            index.x >= Size.x ||
                            index.y < 0 ||
                            index.y >= Size.y)
                        {
                            return null;
                        }

                        return _cellMap[index.x + index.y * Size.x];
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

    }
}