# [StateManager](StateMachine.md##STATEMANAGER-INCLUDES).OnStateChanged
---
## Declaration
```csharp
public event (enum fromState, enum toState) OnStateChanged;
```
### Parameters
|Parameter|Description|
|---|---|
|**fromState**|The previous state before the transition|
|**toState**|The new state after the transition|

## Description
Callback fired whenever state changes

