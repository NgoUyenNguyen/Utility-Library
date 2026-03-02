using System;
using System.Collections.Generic;
using UnityEngine;
using ZLinq;

namespace NgoUyenNguyen.Grid
{
    public class GridAnalyser<TCell> where TCell : Cell
    {
        private readonly Grid<TCell> grid;

        public Vector3 GridUp
        {
            get
            {
                return grid.Space switch
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
                return grid.Space switch
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
                return grid.Layout switch
                {
                    CellLayout.Square => new Vector2(grid.CellSize * grid.Size.x, grid.CellSize * grid.Size.y),
                    CellLayout.Hexagon => default,
                    _ => throw new InvalidOperationException("Invalid Layout!")
                };
            }
        }

        public Vector3 GridCenter
        {
            get
            {
                switch (grid.Layout)
                {
                    case CellLayout.Square:
                        return grid.Alignment switch
                        {
                            GridAlignment.Center =>
                                grid.transform.position,
                            GridAlignment.BottomLeft =>
                                grid.transform.position +
                                (GridRight * grid.Size.x / 2 + GridForward * grid.Size.y / 2) * grid.CellSize,
                            GridAlignment.BottomRight =>
                                grid.transform.position +
                                (GridLeft * grid.Size.x / 2 + GridForward * grid.Size.y / 2) * grid.CellSize,
                            GridAlignment.TopLeft =>
                                grid.transform.position +
                                (GridRight * grid.Size.x / 2 + GridBackward * grid.Size.y / 2) *
                                grid.CellSize,
                            GridAlignment.TopRight =>
                                grid.transform.position +
                                (GridLeft * grid.Size.x / 2 + GridBackward * grid.Size.y / 2) * grid.CellSize,
                            _ => throw new InvalidOperationException("Invalid Alignment!")
                        };
                    case CellLayout.Hexagon:
                        return grid.Alignment switch
                        {
                            GridAlignment.Center =>
                                grid.transform.position,
                            GridAlignment.BottomLeft =>
                                grid.transform.position +
                                (GridRight * ((grid.Size.x * 2 + 1) * Mathf.Sqrt(3) / 4) + GridForward * grid.Size.y / 2) * grid.CellSize / 2,
                            GridAlignment.BottomRight =>
                                grid.transform.position +
                                (GridLeft * grid.Size.x / 2 + GridForward * grid.Size.y / 2) * grid.CellSize,
                            GridAlignment.TopLeft =>
                                grid.transform.position +
                                (GridRight * grid.Size.x / 2 + GridBackward * grid.Size.y / 2) *
                                grid.CellSize,
                            GridAlignment.TopRight =>
                                grid.transform.position +
                                (GridLeft * grid.Size.x / 2 + GridBackward * grid.Size.y / 2) * grid.CellSize,
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

        public IEnumerable<TCell> GetBorderCells() => grid
            .AsValueEnumerable()
            .Where(cell => grid.GetNeighbors(cell, NeighborFilter.OrthogonalOnly)
                .AsValueEnumerable()
                .Count() < 4)
            .ToList();
    }
}