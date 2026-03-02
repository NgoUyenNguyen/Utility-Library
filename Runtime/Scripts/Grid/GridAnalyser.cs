using System;
using System.Collections.Generic;
using UnityEngine;
using ZLinq;

namespace NgoUyenNguyen.Grid
{
    public static class GridAnalyser<TCell> where TCell : Cell
    {
        public static Vector3 GridUp(Grid<TCell> grid) => grid.Space switch
        {
            GridSpace.Horizontal => grid.transform.up,
            GridSpace.Vertical => -grid.transform.forward,
            _ => throw new InvalidOperationException("Invalid Space!")
        };

        public static Vector3 GridForward(Grid<TCell> grid) => grid.Space switch
        {
            GridSpace.Horizontal => grid.transform.forward,
            GridSpace.Vertical => grid.transform.up,
            _ => throw new InvalidOperationException("Invalid Space!")
        };

        public static Vector3 GridRight(Grid<TCell> grid) => grid.transform.right;

        public static Vector3 GridDown(Grid<TCell> grid) => -GridUp(grid);

        public static Vector3 GridLeft(Grid<TCell> grid) => -GridRight(grid);

        public static Vector3 GridBackward(Grid<TCell> grid) => -GridForward(grid);

        public static Vector3 GridCenter(Grid<TCell> grid)
        {
            return grid.Layout switch
            {
                CellLayout.Square => grid.Alignment switch
                {
                    GridAlignment.Center => grid.transform.position,
                    GridAlignment.BottomLeft => grid.transform.position +
                                                (GridRight(grid) * grid.Size.x / 2 + GridForward(grid) * grid.Size.y / 2) *
                                                grid.CellSize,
                    GridAlignment.BottomRight => grid.transform.position +
                                                 (GridLeft(grid) * grid.Size.x / 2 + GridForward(grid) * grid.Size.y / 2) *
                                                 grid.CellSize,
                    GridAlignment.TopLeft => grid.transform.position +
                                             (GridRight(grid) * grid.Size.x / 2 + GridBackward(grid) * grid.Size.y / 2) *
                                             grid.CellSize,
                    GridAlignment.TopRight => grid.transform.position +
                                              (GridLeft(grid) * grid.Size.x / 2 + GridBackward(grid) * grid.Size.y / 2) *
                                              grid.CellSize,
                    _ => throw new InvalidOperationException("Invalid Alignment!")
                },
                CellLayout.Hexagon => grid.Alignment switch
                {
                    GridAlignment.Center => grid.transform.position,
                    GridAlignment.BottomLeft => grid.transform.position +
                                                (GridRight(grid) * ((grid.Size.x * 2 + 1) * Mathf.Sqrt(3) / 4) +
                                                 GridForward(grid) * grid.Size.y / 2) * grid.CellSize / 2,
                    GridAlignment.BottomRight => grid.transform.position +
                                                 (GridLeft(grid) * grid.Size.x / 2 + GridForward(grid) * grid.Size.y / 2) *
                                                 grid.CellSize,
                    GridAlignment.TopLeft => grid.transform.position +
                                             (GridRight(grid) * grid.Size.x / 2 + GridBackward(grid) * grid.Size.y / 2) *
                                             grid.CellSize,
                    GridAlignment.TopRight => grid.transform.position +
                                              (GridLeft(grid) * grid.Size.x / 2 + GridBackward(grid) * grid.Size.y / 2) *
                                              grid.CellSize,
                    _ => throw new InvalidOperationException("Invalid Alignment!")
                },
                _ => throw new InvalidOperationException("Invalid Layout!")
            };
        }
        
        

        public static IEnumerable<TCell> GetBorderCells(Grid<TCell> grid) => grid
            .AsValueEnumerable()
            .Where(cell => grid.GetNeighbors(cell, NeighborFilter.OrthogonalOnly)
                .AsValueEnumerable()
                .Count() < 4)
            .ToList();
    }
}