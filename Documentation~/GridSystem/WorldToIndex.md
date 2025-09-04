# [Grid](GridSystem.md##GRID-INCLUDES).WorldToIndex
---
## Declaration
```csharp
public Vector2Int WorldToIndex(Vector3 worldPos)
```
### Parameters
|Parameter|Description|
|---|---|
|**worldPos**|**WolrdSpace** position|

### Returns
Nearest index from input position

## Description
Method to get index from **WolrdSpace** position. 
If input position is out of bounds, return the nearest index.
