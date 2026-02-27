using UnityEngine;

namespace NgoUyenNguyen.Grid
{
    public partial class BaseGrid
    {
        private void OnDrawGizmos()
        {
            if (!gameObject.activeInHierarchy) return;
            DrawFrame();
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
            for (int y = 0; y <= Size.y; y++)
            {
                Vector3 step = Space switch
                {
                    GridSpace.Horizontal => transform.forward,
                    GridSpace.Vertical => transform.up,
                    _ => Vector3.zero
                };

                Vector3 alignStep = Space switch
                {
                    GridSpace.Horizontal => -transform.forward,
                    GridSpace.Vertical => -transform.up,
                    _ => Vector3.zero
                };

                Vector3 offset = Alignment switch
                {
                    GridAlignment.BottomLeft => Vector3.zero,
                    GridAlignment.BottomRight => -transform.right * Size.x,
                    GridAlignment.TopLeft => alignStep * Size.y,
                    GridAlignment.TopRight => -transform.right * Size.x + alignStep * Size.y,
                    GridAlignment.Center => (-transform.right * Size.x + alignStep * Size.y) / 2f,
                    _ => Vector3.zero
                };

                Vector3 start = transform.position + (y * step + offset) * CellSize;
                Vector3 end = start + transform.right * Size.x * CellSize;

                Gizmos.DrawLine(start, end);
            }


            // Column line
            for (int x = 0; x <= Size.x; x++)
            {
                Vector3 step = Space switch
                {
                    GridSpace.Horizontal => transform.forward,
                    GridSpace.Vertical => transform.up,
                    _ => Vector3.zero
                };

                Vector3 offsetY = Space switch
                {
                    GridSpace.Horizontal => -transform.forward * Size.y,
                    GridSpace.Vertical => -transform.up * Size.y,
                    _ => Vector3.zero
                };

                Vector3 offset = Alignment switch
                {
                    GridAlignment.BottomLeft => Vector3.zero,
                    GridAlignment.BottomRight => -transform.right * Size.x,
                    GridAlignment.TopLeft => offsetY,
                    GridAlignment.TopRight => -transform.right * Size.x + offsetY,
                    GridAlignment.Center => (-transform.right * Size.x + offsetY) / 2f,
                    _ => Vector3.zero
                };

                Vector3 start = transform.position + (transform.right * x + offset) * CellSize;
                Vector3 end = start + step * Size.y * CellSize;

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

            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    float horizStep = Mathf.Sqrt(3f) * (CellSize / 2f);
                    float vertStep = 1.5f * (CellSize / 2f);

                    float xPos = horizStep * (0.5f + x + 0.5f * (y % 2));
                    float yPos = _cellSize / 2 + vertStep * y;

                    float xDelta = (_size.x + .5f) * Mathf.Sqrt(3) * _cellSize / 2;
                    float yDelta = Mathf.Ceil(_size.y / 2f) * CellSize + Mathf.Floor(_size.y / 2f) * _cellSize / 2;
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
    }
}