# [StateMachine](../StackStateMachine.md##STATEMACHINE).AlwaysEnterExit

---
## Declaration
```csharp
public bool AlwaysEnterExit;
```

## Description
Controls how enter/exit callbacks are applied to underlying states when pushing or popping.

- When false (default):
  - PushState: the previous top state remains paused (no Exit called); only the new state receives Enter.
  - PopState: the popped state receives Exit; the newly exposed state resumes without Enter.
- When true:
  - PushState: the previous top state receives Exit before pushing; the new state receives Enter.
  - PopState: the popped state receives Exit; the newly exposed top receives Enter again.
