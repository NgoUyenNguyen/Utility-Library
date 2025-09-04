# [Grid](GridSystem.md##GRID-INCLUDES).LocalToIndex
---
## Declaration
```csharp
public Vector2Int LocalToIndex(Vector3 localPos)
```

### Parameters
|Parameter|Description|
|---|---|
|**localPos**|**LocalSpace** position|

### Returns
Nearest index from input position

## Description
Method to get index from **LocalSpace** position. 
If input position is out of bounds, return the nearest index.
