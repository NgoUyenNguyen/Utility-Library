using System.Collections.Generic;
using NgoUyenNguyen.GridSystem;
using UnityEngine;

/// <summary>
/// Represents a grid structure composed of cells, supporting various alignment and spacing options.
/// </summary>
/// <remarks>The <see cref="Grid"/> class provides functionality for creating, managing, and
/// interacting with a grid of cells. Likely a collection, 
/// <see cref="Grid"/> can access cells through index like a 2D array and iterated by foreach loop</remarks>
public abstract class Grid<T> : BaseGrid where T : Cell
{
    /// <summary>
    /// Access <c>Cell</c> in <c>Grid</c> through (<paramref name="x"/>, <paramref name="y"/>) coordinate
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public new T this[int x, int y]
    {
        get { return base[x, y] as T; }
        set { base[x, y] = value; }
    }

    /// <summary>
    /// Access <c>Cell</c> in <c>Grid</c> through index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public new T this[Vector2Int index]
    {
        get { return base[index] as T; }
        set { base[index] = value; }
    }


    /// <summary>
    /// <c>ONLY USE IN EDITOR</c><br/>
    /// Method to spawn prefab at specific <c>Index</c>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cellPrefab"></param>
    /// <param name="index"></param>
    /// <returns><c>Cell</c> at <c>Index</c></returns>
    public T SpawnCellPrefab(T cell, Vector2Int index)
    {
        return base.SpawnCellPrefab(cell, index) as T;
    }

    /// <summary>
    /// Method to get <c>Cell</c> from <c>LocalSpace</c> position
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// If input position is out of bounds, return the nearest <c>Cell</c>
    /// </remarks>
    /// <param name="localPos"></param>
    /// <returns>Nearest <c>Cell</c></returns>
    public new T LocalToCell(Vector3 localPos)
    {
        return base.LocalToCell(localPos) as T;
    }

    /// <summary>
    /// Method to get <c>Cell</c> from <c>WorldSpace</c> position
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// If input position is out of bounds, return the nearest <c>Cell</c>
    /// </remarks>
    /// <param name="worldPos"></param>
    /// <returns>Nearest <c>Cell</c></returns>
    public new T WorldToCell(Vector3 worldPos)
    {
        return base.WorldToCell(worldPos) as T;
    }

    /// <summary>
    /// Method to get cell from <paramref name="cell"/> and <paramref name="indexDelta"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// For example, (1,0) will return the right neighbor<br/>
    /// and (2,0) will return the right second neighbor.
    /// </remarks>
    /// <param name="cell">Origin cell</param>
    /// <param name="indexDelta">index delta from origin cell to neighbor.
    /// </param>
    /// <returns>Neighbor cell from <paramref name="cell"/> plus <paramref name="indexDelta"/></returns>
    public T GetNeighbor(T cell, Vector2Int indexDelta)
    {
        return base.GetNeighbor(cell, indexDelta) as T;
    }

    /// <summary>
    /// Method to get all neighbors surrounding <paramref name="cell"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cell"></param>
    /// <returns>
    /// Maximum 8 neighbors and ignore null neighbors
    /// </returns>
    public HashSet<T> GetNeighbors(T cell)
    {
        HashSet<T> result = new HashSet<T>();
        foreach (var n in base.GetNeighbors(cell))
        {
            result.Add(n as T);
        }

        return result;
    }

    /// <summary>
    /// Method to get only diagonal neighbors of <paramref name="cell"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cell"></param>
    /// <returns>
    /// Maximum 4 neighbors and ignore null neighbors
    /// </returns>
    public HashSet<T> GetNeighborsInDiagonal(T cell)
    {
        HashSet<T> result = new HashSet<T>();
        foreach (var n in base.GetNeighborsInDiagonal(cell))
        {
            result.Add(n as T);
        }

        return result;
    }

    /// <summary>
    /// Method to get neighbors of <paramref name="cell"/>, ignore diagonally
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cell"></param>
    /// <returns>
    /// Maximum 4 neighbors and ignore null neighbors
    /// </returns>
    public HashSet<T> GetNeighborsIgnoreDiagonal(T cell)
    {
        HashSet<T> result = new HashSet<T>();
        foreach (var n in base.GetNeighborsIgnoreDiagonal(cell))
        {
            result.Add(n as T);
        }

        return result;
    }


}
