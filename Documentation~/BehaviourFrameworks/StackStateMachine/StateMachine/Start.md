# [StateMachine](../StackStateMachine.md##STATEMACHINE).Start

---
## Declaration
```csharp
public void Start()
```

## Parameter

## Return

## Description
Starts the state machine if it has not already been started. When called the first time and the stack is not empty, this will call [Enter](../State/Enter.md) on the current [CurrentState](CurrentState.md).

- Subsequent calls do nothing once the machine has started.
- If the stack is empty, nothing happens.
