# Grid System
***Provides pathfinding functionality using customizable parameters for determining neighbors, distances, and heuristics.***
---
## Usage
Create a script in Editor folder, inherits from BaseLevelEditor class
```csharp
public class ExampleLevelEditor : BaseLevelEditor
{
	// Override level template prefab
	protected override GameObject levelTemplate => ...

	// Override OnDrawGUI, this method is called each frame
	// when Level Editor window is open
	protected ovveride void OnDrawGUI()
	{
		// Custom your Editor window here
	}
}
```