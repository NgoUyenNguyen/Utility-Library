using System;
using UnityEngine;

namespace NgoUyenNguyen.Grid
{
    public class GridAnalyser<TCell> where TCell : Cell
    {
        private readonly Grid<TCell> grid;

        public Vector3 GridUp
        {
            get
            {
                return grid.space switch
                {
                    GridSpace.Horizontal => grid.transform.up,
                    GridSpace.Vertical => -grid.transform.forward,
                    _ => throw new InvalidOperationException("Invalid Space!")
                };
            }
        }

        public Vector3 GridForward
        {
            get
            {
                return grid.space switch
                {
                    GridSpace.Horizontal => grid.transform.forward,
                    GridSpace.Vertical => grid.transform.up,
                    _ => throw new InvalidOperationException("Invalid Space!")
                };
            }
        }

        public Vector3 GridRight => grid.transform.right;

        public Vector3 GridDown => -GridUp;

        public Vector3 GridLeft => -GridRight;

        public Vector3 GridBackward => -GridForward;

        public Vector2 GridSize
        {
            get
            {
                return grid.layout switch
                {
                    CellLayout.Square => new Vector2(grid.cellSize * grid.size.x, grid.cellSize * grid.size.y),
                    CellLayout.Hexagon => default,
                    _ => throw new InvalidOperationException("Invalid Layout!")
                };
            }
        }

        public Vector3 GridCenter
        {
            get
            {
                switch (grid.layout)
                {
                    case CellLayout.Square:
                        return grid.alignment switch
                        {
                            GridAlignment.Center =>
                                grid.transform.position,
                            GridAlignment.BottomLeft =>
                                grid.transform.position +
                                (GridRight * grid.size.x / 2 + GridForward * grid.size.y / 2) * grid.cellSize,
                            GridAlignment.BottomRight =>
                                grid.transform.position +
                                (GridLeft * grid.size.x / 2 + GridForward * grid.size.y / 2) * grid.cellSize,
                            GridAlignment.TopLeft =>
                                grid.transform.position +
                                (GridRight * grid.size.x / 2 + GridBackward * grid.size.y / 2) *
                                grid.cellSize,
                            GridAlignment.TopRight =>
                                grid.transform.position +
                                (GridLeft * grid.size.x / 2 + GridBackward * grid.size.y / 2) * grid.cellSize,
                            _ => throw new InvalidOperationException("Invalid Alignment!")
                        };
                    case CellLayout.Hexagon:
                        return grid.alignment switch
                        {
                            GridAlignment.Center =>
                                grid.transform.position,
                            GridAlignment.BottomLeft =>
                                grid.transform.position +
                                (GridRight * ((grid.size.x * 2 + 1) * Mathf.Sqrt(3) / 4) + GridForward * grid.size.y / 2) * grid.cellSize / 2,
                            GridAlignment.BottomRight =>
                                grid.transform.position +
                                (GridLeft * grid.size.x / 2 + GridForward * grid.size.y / 2) * grid.cellSize,
                            GridAlignment.TopLeft =>
                                grid.transform.position +
                                (GridRight * grid.size.x / 2 + GridBackward * grid.size.y / 2) *
                                grid.cellSize,
                            GridAlignment.TopRight =>
                                grid.transform.position +
                                (GridLeft * grid.size.x / 2 + GridBackward * grid.size.y / 2) * grid.cellSize,
                            _ => throw new InvalidOperationException("Invalid Alignment!")
                        };
                    default:
                        throw new InvalidOperationException("Invalid Layout!");
                }
            }
        }

        public GridAnalyser(Grid<TCell> grid) => this.grid = grid;


        public TCell GetFurthestCell(Vector3 direction)
        {
            if (direction == Vector3.zero) return null;

            direction.Normalize();
            throw new NotImplementedException();
        }
    }
}