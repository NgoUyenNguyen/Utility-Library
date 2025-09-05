# [Grid System](GridSystem.md) Usage
---

By default, Cell is an emty game object. For purpose of customize, create your own cell prefab, 
which has [Cell](GridSystem.md##CELL-INCLUDES) component or its child class.  
Create new script which inherits from [Grid](GridSystem.md##GRID-INCLUDES)<> (not the Grid without generic, its Unity's 
[Grid](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Grid.html) component), 
the generic type is type of [Cell](GridSystem.md##CELL-INCLUDES) component. 
Get an example of case using child of Cell component:

```csharp
public class ExampleCell : Cell
{	
}
```
Add this script to a [GameObject](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/GameObject.html)
and set this [GameObject](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/GameObject.html) to be a prefab.  
Now move to [Grid](GridSystem.md##GRID-INCLUDES):
```csharp
public class ExampleGrid : Grid<ExampleCell>
{
}
```
Add this script to a [GameObject](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/GameObject.html),
set up the [Grid](GridSystem.md##GRID-INCLUDES), and drag the cell prefab we set before to Cell Prefab of 
the [Grid](GridSystem.md##GRID-INCLUDES).  
Now the ExampleGrid will work with ExampleCell defined before.
