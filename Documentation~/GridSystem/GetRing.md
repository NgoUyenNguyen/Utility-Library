# [Grid](GridSystem.md##GRID-INCLUDES).GetRing
---
## Declaration
```csharp
public HashSet<T> GetRing<T>(T cell, int radius) where T : Cell
```

### Parameters
| Parameter  | Description                                                                                                           |
|------------|-----------------------------------------------------------------------------------------------------------------------|
| **cell**   | Origin [Cell](GridSystem.md##CELL-INCLUDES)                                                                           |
| **radius** | Radius of the ring                                                                                             |

### Returns
Ring of [Cells](GridSystem.md##CELL-INCLUDES) of type T.

## Declaration
```csharp
public HashSet<T> GetRing<T>(Vector2Int center, int radius) where T : Cell
```

### Parameters
| Parameter  | Description               |
|------------|---------------------------|
| **center** | Center index position     |
| **radius** | Radius of the ring |

### Returns
Ring of [Cells](GridSystem.md##CELL-INCLUDES) of type T.

## Description
Method to get a ring of [Cells](GridSystem.md##CELL-INCLUDES) of type T.