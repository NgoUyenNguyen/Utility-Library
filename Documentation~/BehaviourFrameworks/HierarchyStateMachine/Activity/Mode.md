# [Activity](../HierarchyStateMachine.md##ACTIVITY).Mode

---

## Declaration
```csharp
public ActivityMode Mode { get; protected set; }
```

Possible values:
- Inactive — The activity is not running.
- Activating — The activity is in the process of activating.
- Active — The activity is active.
- Deactivating — The activity is in the process of deactivating.

## Description
- Mode transitions are handled by ActivateAsync and DeactivateAsync.
- The property is read-only from the public API (protected setter on Activity), intended to be controlled by the Activity base implementation.
- Implementations should not set Mode directly from outside the lifecycle methods.
