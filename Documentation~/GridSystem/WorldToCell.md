# [Grid](GridSystem.md##GRID-INCLUDES).WorldToCell
---
## Declaration
```csharp
public Cell WorldToCell(Vector3 worldPos)
```

### Parameters
|Parameter|Description|
|---|---|
|**worldPos**|**WorldSpace** position of [Cell](GridSystem.md##CELL-INCLUDES)|

### Returns
Nearest [Cell](GridSystem.md##CELL-INCLUDES) from input position

## Description
Method to get Cell from **WolrdSpace** position. 
If input position is out of bounds, return the nearest [Cell](GridSystem.md##CELL-INCLUDES)
