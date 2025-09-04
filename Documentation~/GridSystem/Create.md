# [Grid](GridSystem.md##GRID-INCLUDES).Create
---
## Declaration
```csharp
public void Create(int width, int height, Cell cell = null)
```

### Parameters
|Parameter|Description|
|---|---|
|**width**|Width of the [Grid](GridSystem.md##GRID-INCLUDES)|
|**height**|Height of the [Grid](GridSystem.md##GRID-INCLUDES)|
|**cell**|Cell to be spawned.<br> (Leave default to use [Grid](GridSystem.md##GRID-INCLUDES).[cellPrefab](cellPrefab.md))|

### Returns



## Declaration
```csharp
public void Create(Vector2Int size, Cell cell = null)
```

### Parameters
|Parameter|Description|
|---|---|
|**size**|Size of the [Grid](GridSystem.md##GRID-INCLUDES)|
|**cell**|Cell to be spawned.<br> (Leave default to use [Grid](GridSystem.md##GRID-INCLUDES).[cellPrefab](cellPrefab.md))|

### Returns

## Declaration
```csharp
public void Create(bool[,] map, Cell cell = null)
```

### Parameters
|Parameter|Description|
|---|---|
|**map**|Map to define where to spawn cell in detail.<br> Leave default to spawn all|
|**cell**|Cell to be spawned.<br> (Leave default to use [Grid](GridSystem.md##GRID-INCLUDES).[cellPrefab](cellPrefab.md))|

### Returns



## Description
Method to create [Grid](GridSystem.md##GRID-INCLUDES)
