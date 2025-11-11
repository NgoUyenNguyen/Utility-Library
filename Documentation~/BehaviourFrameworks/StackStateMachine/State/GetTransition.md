# [State](../StackStateMachine.md##STATE).GetTransition

---
## Declaration
```csharp
protected virtual void GetTransition()
```

## Parameter

## Return

## Description
Called once per update cycle, before [OnUpdate](OnUpdate.md) is executed. 
Override this to perform stack changes by invoking [Machine](Machine.md).[PushState](../StackStateMachine.md#public-methods) or [PopState](../StackStateMachine.md#public-methods) 
when your transition conditions are met. Because this method returns void in the stack-based variant, transitions are enacted by directly modifying the machine's stack rather than returning a target state.
