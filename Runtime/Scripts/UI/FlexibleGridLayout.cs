using UnityEngine;
using UnityEngine.UI;

namespace NgoUyenNguyen
{
    [AddComponentMenu("Layout/Flexible Grid Layout")]
    public class FlexibleGridLayout : LayoutGroup
    {
        public enum FitType
        {
            Uniform,
            Width,
            Height,
            FixedRows,
            FixedColumns
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDirty();
        }
#endif

        [SerializeField] private FitType fitType;
        [SerializeField] private int rows;
        [SerializeField] private int columns;
        [SerializeField] private Vector2 cellSize;
        [SerializeField] private Vector2 spacing;
        [SerializeField] private bool fitX;
        [SerializeField] private bool fitY;

        public int Rows
        {
            get => rows;
            set => rows = value;
        }

        public int Columns
        {
            get => columns;
            set => columns = value;
        }

        public Vector2 CellSize
        {
            get => cellSize;
            set => cellSize = value;
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalculateGrid();
        }


        public override void CalculateLayoutInputVertical()
        {
            float minWidth = padding.left + padding.right +
                             (cellSize.x * columns) +
                             (spacing.x * (columns - 1));

            float minHeight = padding.top + padding.bottom +
                              (cellSize.y * rows) +
                              (spacing.y * (rows - 1));

            SetLayoutInputForAxis(minWidth, minWidth, -1, 0);
            SetLayoutInputForAxis(minHeight, minHeight, -1, 1);
        }


        public override void SetLayoutHorizontal()
        {
            for (int i = 0; i < rectChildren.Count; i++)
            {
                int column = i % columns;
                float xPos = padding.left + (cellSize.x + spacing.x) * column;
                SetChildAlongAxis(rectChildren[i], 0, xPos, cellSize.x);
            }
        }


        public override void SetLayoutVertical()
        {
            for (int i = 0; i < rectChildren.Count; i++)
            {
                int row = i / columns;
                float yPos = padding.top + (cellSize.y + spacing.y) * row;
                SetChildAlongAxis(rectChildren[i], 1, yPos, cellSize.y);
            }
        }

        
        private void CalculateGrid()
        {
            
            if (fitType == FitType.Uniform || fitType == FitType.Width || fitType == FitType.Height)
            {
                float sqrt = Mathf.Sqrt(rectChildren.Count);
                rows = columns = Mathf.CeilToInt(sqrt);
            }

            if (fitType == FitType.Width || fitType == FitType.FixedColumns)
                rows = Mathf.CeilToInt(rectChildren.Count / (float)columns);

            if (fitType == FitType.Height || fitType == FitType.FixedRows)
                columns = Mathf.CeilToInt(rectChildren.Count / (float)rows);

            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);

            float parentWidth = rectTransform.rect.width;
            float parentHeight = rectTransform.rect.height;

            float cellWidth = (parentWidth - padding.left - padding.right - spacing.x * (columns - 1)) / columns;
            float cellHeight = (parentHeight - padding.top - padding.bottom - spacing.y * (rows - 1)) / rows;

            if (fitX) cellSize.x = cellWidth;
            if (fitY) cellSize.y = cellHeight;
        }

    }
}