# PathHandle

***A handle for accessing pathfinding results, similar to a Task in C#, representing a promise that the path result will
be available in the future.***

## Description

PathHandle is a wrapper class that acts as a promise for pathfinding operations. When you request a path
using [Grid.RequestPath](GridSystem/RequestPath.md), instead of waiting for the pathfinding algorithm to complete on the
main thread, you receive a PathHandle immediately. The actual pathfinding calculation happens in a background thread,
and the result will be available through this handle when the calculation is complete.

## Properties

| Property       | Description                                                                                                                                                                                           |
|----------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **IsComplete** | Indicates whether the pathfinding calculation has completed                                                                                                                                           |
| **Result**     | The calculated path result. Only valid when IsComplete is true.<br/>It includes:<br/> - HasPath: Define whether path between 2 cells requested exists<br/> - Path: The path between 2 requested cells |
|**OnComplete** | A callback event includes Result as parameter that will be invoked when the pathfinding calculation is complete.                                                                                      |

## Example Usage
Using IsComplete and Result to check if the pathfinding calculation is complete and get the result.
```csharp
public class TestGrid : Grid<TestCell>
{
    private PathHandle<TestCell> handle;
    
    private void Start()
    {
        handle = RequestPath(this[-4, 8], this[9, 0]);
    }
    
    private void Update()
    {
        if (handle.IsComplete)
        {
            Debug.Log("Path complete");
            foreach (var node in handle.Result.Path)
            {
                // Do something with the path
            }
        }
}
```

Using OnComplete to get the result when the pathfinding calculation is complete.
```csharp
public class TestGrid : Grid<TestCell>
{
    private PathHandle<TestCell> handle;
    
    private void Start()
    {
        handle = RequestPath(this[-4, 8], this[9, 0]);
        handle.OnComplete += OnPathComplete;
    }
    
    // Method invoked when the pathfinding calculation is complete
    private void OnPathComplete(Result<TestCell> result)
    {
        Debug.Log("Path complete");
        foreach (var node in result.Path)
        {
            // Do something with the path
        }
    }
}
```
