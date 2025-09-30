# [Grid](GridSystem.md##GRID-INCLUDES).CombineMesh
---
## Declaration
```csharp
public void CombineMesh(Material material = null)
```

### Parameters
| Parameter  | Description                                                                                                           |
|------------|-----------------------------------------------------------------------------------------------------------------------|
| **material** | Optional material to apply to the combined mesh<br/>If not provided, the material of the first child mesh will be used. |

### Returns

## Description
If your grid is static and cells do not change overtime.  
You should use this method to combines the meshes of child objects into a single mesh for optimization.  
<br/>
Note: This method assumes that child objects do not have submesh.  
If child objects have submesh, this method will not work properly and you have to combine the meshes manually.