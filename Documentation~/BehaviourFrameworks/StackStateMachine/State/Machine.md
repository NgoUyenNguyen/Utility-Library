# [State](../StackStateMachine.md##STATE).Machine

---
## Declaration
```csharp
public readonly StateMachine Machine;
```

## Description
Reference to the owning [StateMachine](../StackStateMachine.md##STATEMACHINE) that manages the stack and updates for this state. The machine is injected automatically when the state is added to the stack (via reflection in StateMachine).
