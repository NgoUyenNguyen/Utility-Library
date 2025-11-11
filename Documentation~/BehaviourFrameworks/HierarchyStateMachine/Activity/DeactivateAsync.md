# [Activity](../HierarchyStateMachine.md##ACTIVITY).DeactivateAsync

---
## Declaration

```csharp
public virtual Task DectivateAsync(CancellationToken ct)
```

## Parameters

| Parameter | Description        |
|-----------|--------------------|
| ct        | Cancellation token |

## Returns

Task represents the asynchronous operation.

## Description

An asynchronous method that called when exiting associated [State](../HierarchyStateMachine.md##STATE),
and deactivates the Activity.

## Remarks

The best practice of using this method is as below

```csharp
public virtual async Task DectivateAsync(CancellationToken ct)
{
    if (Mode != ActivityMode.Active) return; // Exit if the mode is not Active

    Mode = ActivityMode.Deactivating; // Change the mode to Deactivating
    
    // Insert your asyncronous code here
    
    Mode = ActivityMode.Inactive; // Change the mode to Inactive
}
```

