# [Activity](../HierarchyStateMachine.md##ACTIVITY).ActivateAsync

---
## Declaration

```csharp
public virtual Task ActivateAsync(CancellationToken ct)
```

## Parameters

| Parameter | Description        |
|-----------|--------------------|
| ct        | Cancellation token |

## Returns

Task represents the asynchronous operation.

## Description

An asynchronous method that called when entering associated [State](../HierarchyStateMachine.md##STATE),
and activates the Activity.

## Remarks

The best practice of using this method is as below

```csharp
public virtual async Task ActivateAsync(CancellationToken ct)
{
    if (Mode != ActivityMode.Inactive) return; // Exit if the mode is not Inactive

    Mode = ActivityMode.Activating; // Change the mode to Activating
    
    // Insert your asyncronous code here
    
    Mode = ActivityMode.Active; // Change the mode to Active
}
```



