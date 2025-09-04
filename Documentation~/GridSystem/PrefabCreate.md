# [Grid](GridSystem.md##GRID-INCLUDES).PrefabCreate
---
**USE ONLY IN EDITOR**

## Declaration
```csharp
public void PrefabCreate(int width, int height, GameObject cellPrefab, bool[,] map = null)
```

### Parameters
|Parameter|Description|
|---|---|
|**width**|Width of [Grid](GridSystem.md##GRID-INCLUDES)|
|**height**|Height of [Grid](GridSystem.md##GRID-INCLUDES)|
|**cellPrefab**|Prefab to be spawned|
### Returns

## Declaration
```csharp
public void PrefabCreate(Vector2Int size, GameObject cellPrefab, bool[,] map = null)
```

### Parameters
|Parameter|Description|
|---|---|
|**size**|Size of [Grid](GridSystem.md##GRID-INCLUDES)|
|**cellPrefab**|Prefab to be spawned|
### Returns

## Declaration
```csharp
public void PrefabCreate(bool[,] map, GameObject cellPrefab)
```

### Parameters
|Parameter|Description|
|---|---|
|**map**|Map to define where to spawn cell in detail|
|**cellPrefab**|Prefab to be spawned|
### Returns


## Description
Method to create [Grid](GridSystem.md##GRID-INCLUDES) which remaining 
its connecting to original [Cell](GridSystem.md##CELL-INCLUDES) prefab
