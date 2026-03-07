using System;
using System.Collections.Generic;
using UnityEngine;
using ZLinq;

namespace NgoUyenNguyen.Grid
{
    public class GridAnalyser<TCell> where TCell : Cell
    {
        public Grid<TCell> Grid { get; }
        
        public GridAnalyser(Grid<TCell> grid) => Grid = grid;
        
        public Vector3 GridUp => GetGridUp(Grid);
        
        public Vector3 GridForward => GetGridForward(Grid);
        
        public Vector3 GridRight => GetGridRight(Grid);
        public Vector3 GridDown => GetGridDown(Grid);
        
        public Vector3 GridLeft => GetGridLeft(Grid);
        
        public Vector3 GridBackward => GetGridBackward(Grid);
        
        public Vector3 GridCenter => GetGridCenter(Grid);
        
        public IEnumerable<TCell> BorderCells => GetBorderCells(Grid);
        
        
        
        public static Vector3 GetGridUp(Grid<TCell> grid) => grid.Space switch
        {
            GridSpace.Horizontal => grid.transform.up,
            GridSpace.Vertical => -grid.transform.forward,
            _ => throw new InvalidOperationException("Invalid Space!")
        };

        public static Vector3 GetGridForward(Grid<TCell> grid) => grid.Space switch
        {
            GridSpace.Horizontal => grid.transform.forward,
            GridSpace.Vertical => grid.transform.up,
            _ => throw new InvalidOperationException("Invalid Space!")
        };

        public static Vector3 GetGridRight(Grid<TCell> grid) => grid.transform.right;

        public static Vector3 GetGridDown(Grid<TCell> grid) => -GetGridUp(grid);

        public static Vector3 GetGridLeft(Grid<TCell> grid) => -GetGridRight(grid);

        public static Vector3 GetGridBackward(Grid<TCell> grid) => -GetGridForward(grid);

        public static Vector3 GetGridCenter(Grid<TCell> grid)
        {
            return grid.Layout switch
            {
                CellLayout.Square => grid.Alignment switch
                {
                    GridAlignment.Center => grid.transform.position,
                    GridAlignment.BottomLeft => grid.transform.position +
                                                (GetGridRight(grid) * grid.Size.x / 2 + GetGridForward(grid) * grid.Size.y / 2) *
                                                grid.CellSize,
                    GridAlignment.BottomRight => grid.transform.position +
                                                 (GetGridLeft(grid) * grid.Size.x / 2 + GetGridForward(grid) * grid.Size.y / 2) *
                                                 grid.CellSize,
                    GridAlignment.TopLeft => grid.transform.position +
                                             (GetGridRight(grid) * grid.Size.x / 2 + GetGridBackward(grid) * grid.Size.y / 2) *
                                             grid.CellSize,
                    GridAlignment.TopRight => grid.transform.position +
                                              (GetGridLeft(grid) * grid.Size.x / 2 + GetGridBackward(grid) * grid.Size.y / 2) *
                                              grid.CellSize,
                    _ => throw new InvalidOperationException("Invalid Alignment!")
                },
                CellLayout.Hexagon => grid.Alignment switch
                {
                    GridAlignment.Center => grid.transform.position,
                    GridAlignment.BottomLeft => grid.transform.position +
                                                (GetGridRight(grid) * ((grid.Size.x * 2 + 1) * Mathf.Sqrt(3) / 4) +
                                                 GetGridForward(grid) * grid.Size.y / 2) * grid.CellSize / 2,
                    GridAlignment.BottomRight => grid.transform.position +
                                                 (GetGridLeft(grid) * grid.Size.x / 2 + GetGridForward(grid) * grid.Size.y / 2) *
                                                 grid.CellSize,
                    GridAlignment.TopLeft => grid.transform.position +
                                             (GetGridRight(grid) * grid.Size.x / 2 + GetGridBackward(grid) * grid.Size.y / 2) *
                                             grid.CellSize,
                    GridAlignment.TopRight => grid.transform.position +
                                              (GetGridLeft(grid) * grid.Size.x / 2 + GetGridBackward(grid) * grid.Size.y / 2) *
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