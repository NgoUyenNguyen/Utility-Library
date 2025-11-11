# [State Machine](StateMachine.md) Usage
---

For using, create a enum to define states of the object in the 
[MonoBehaviour](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/MonoBehaviour.html):
```csharp
public class ExampleClass : MonoBehaviour
{
	public enum ExampleState
	{
		// Define states
		Idle,
		Stand,
		Jump,
	}
}
```
Then instead inheritting from 
[MonoBehaviour](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/MonoBehaviour.html),
 inheritting from StateManager with the generic type is your enum of states:
```csharp
public class ExampleClass : StateManager<ExampleState>
{
	public enum ExampleState
	{
		// Define states
		Idle,
		Stand,
		Jump,
	}

	protected override void InitializeStates()
	{
	}

	protected override ExampleState InitializeEntryState()
	{
	}
}
```
Now you need to define each state in a class which inheritting from BaseState. 
Get an example with the Idle state: 
```csharp
public class Idle : BaseState<ExampleClass.ExampleState>
{
	private ExampleClass _context

	// You only just need to define the enum equivalent to this class in constructor,
	// But highly recommend using a field to hold reference to its StateManager,
	// which will be the context for the state to interact with other GameObject
	public Idle (ExampleClass.ExampleState.Idle, ExampleState context) : base(ExampleClass.ExampleState.Idle)
	{
		_context = context;
	}

	public override void EnterState()
	{
	}

	public override void UpdateState()
	{
	}

	public override void ExitState()
	{
	}

	public override ExampleClass.ExampleState GenerateNextState()
	{
	}
}
```
Finally, add these state classes to StateManager and define the entry state
```csharp
	protected override void InitializeStates()
	{
		// Add state to StateManager through AddStates()
		AddStates(
		new Idle(ExampleState.Idle, this),
		new Stand(ExampleState.Stand, this),
		new Jump(ExampleState.Jump, this)
		);
	}

	protected override ExampleState InitializeEntryState()
	{
		// In this example, Idle will be the entry state
		return ExampleState.Idle;
	}
```
Now you can control game object in each state class.

