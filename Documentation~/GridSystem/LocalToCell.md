# [Grid](GridSystem.md##GRID-INCLUDES).LocalToCell
---
## Declaration
```csharp
public Cell LocalToCell(Vector3 localPos)
```
### Parameters
|Parameter|Description|
|---|---|
|**localPos**|**LocalSpace** position of [Cell](GridSystem.md##CELL-INCLUDES)|

### Returns
Nearest [Cell](GridSystem.md##CELL-INCLUDES) from input position

## Description
Method to get Cell from **LocalSpace** position. 
If input position is out of bounds, return the nearest [Cell](GridSystem.md##CELL-INCLUDES)
