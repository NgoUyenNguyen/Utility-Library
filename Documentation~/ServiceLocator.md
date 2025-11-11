# Service Locator

A scoped register/resolve mechanism for Unity services.
The system supports three lookup layers in priority order: Local (a ServiceLocator attached on a parent in the hierarchy), Scene scope, and Global scope.
You can register services at any layer and resolve them from anywhere through a unified API.

---

## Goals & Benefits
- Decouple service usage from service creation (dependency inversion).
- Multiple scopes: Global (cross-scene), Scene (within a single scene), and Local along the Transform hierarchy.
- Lookup order from nearest to farthest, allowing narrower scopes to override wider ones.
- Simple API surface: Register, Get, TryGet, Has, Unregister.

---

## Scopes and Lookup Order
1) Local within the caller's hierarchy: the nearest ServiceLocator in parent chain (GetComponentInParent).
2) Scene scope: a ServiceLocator dedicated to the scene that contains the caller.
3) Global scope: a global ServiceLocator, which can persist across scene loads (DontDestroyOnLoad).

The ServiceLocator system implements hierarchical scope inheritance, allowing services registered in broader scopes to
be accessible from narrower ones. For example, a service registered in the Global scope can be resolved from both Scene
and Local scopes, while a service registered in Scene scope is available to all Local scopes within that scene. This
inheritance follows the lookup order (Local → Scene → Global). Additionally, narrower scopes can override services from
broader scopes by registering their own implementation of the same service type. When resolving a service, the system
will return the implementation from the nearest scope, effectively allowing Local implementations to override Scene
ones, and Scene implementations to override Global ones.

Helper accessors:
- ServiceLocator.For(MonoBehaviour mb): returns the closest locator relative to mb (Local → Scene → Global).
- ServiceLocator.ForSceneOf(MonoBehaviour mb): returns the locator for mb's scene (falls back to Global if none).
- ServiceLocator.Global: returns (or creates on demand) the global locator.

---

## Bootstrapping
There are three common approaches:

1) On-demand
- Simply access ServiceLocator.Global. A GameObject named "ServiceLocator [Global]" will be created and configured for the Global scope.
- For Scene scope, if your scene contains a GameObject with SceneServiceLocatorBootstrapper, then calling ServiceLocator.ForSceneOf(mb) will lazily bootstrap the scene container.

2) Editor menu
- GameObject → ServiceLocator → Add Global Scope: creates a Global GameObject with GlobalServiceLocatorBootstrapper.
- GameObject → ServiceLocator → Add Scene Scope: creates a Scene GameObject with SceneServiceLocatorBootstrapper.

3) Manual
- Create a GameObject and add ServiceLocator plus the appropriate Bootstrapper. The bootstrapper will call ConfigureAsGlobal or ConfigureForScene internally to set the scope.

Note: Global can be configured with DontDestroyOnLoad so it survives scene changes.

---

## Register Services
Two API styles are available:

1) Generic, infers the type from the instance
```csharp
ServiceLocator.Global
    .Register<IMyService>(new MyService());
```

2) Non-generic, map multiple types to the same instance
```csharp
var svc = new MyService();
ServiceLocator.Global
    .Register(svc, typeof(IMyService), typeof(object)); // e.g., map additional interfaces
```

You can also register into specific scopes:
```csharp
// Global scope
ServiceLocator.Global.Register<ILogger>(new UnityLogger());

// Scene scope
ServiceLocator.ForSceneOf(this).Register<IEnemySpawner>(new EnemySpawner());

// Local (attached somewhere up the parent hierarchy)
ServiceLocator.For(this).Register<IDamageCalculator>(new DamageCalculator());
```

---

## Resolve (Get/TryGet/Has)
- Get<T>(): returns a service of type T; throws if not found.
- TryGet<T>(out T service): returns true/false without throwing.
- Has<T>(): checks if a service of type T exists.
- Has(Type): non-generic variant.

Example:
```csharp
// Prefer the locator closest to the usage context
var locator = ServiceLocator.For(this);

// Get: throws if missing
var audio = locator.Get<IAudioService>();

// TryGet: safe lookup
if (locator.TryGet<IAnalytics>(out var analytics))
{
    analytics.Log("LevelStart");
}

// Has: quick check
if (!locator.Has<IConfig>())
{
    locator.Register<IConfig>(new DefaultConfig());
}
```

Lookup order: Local → Scene → Global. If not present locally, the system searches the scene locator, then falls back to Global.

---

## Unregister
```csharp
// Unregister by type
a = ServiceLocator.Global.Unregister<IAudioService>();

// Unregister multiple types at once (non-generic)
ServiceLocator.Global.Unregister(typeof(IMyService), typeof(IAnotherService));

// Remove all services registered in the current locator
ServiceLocator.Global.UnregisterAll();
```

Note: Unregistering in one scope does not affect other scopes.

---

## End-to-end Example
Assume you have three services: IAudioService, IAnalytics, and IConfig.

1) Register global services at the start of the game (e.g., in an initializer):
```csharp
void Awake()
{
    ServiceLocator.Global
        .Register<IAudioService>(new AudioService())
        .Register<IAnalytics>(new AnalyticsService());
}
```

2) Register scene services at level load (e.g., in a scene bootstrap):
```csharp
void Start()
{
    var sceneLocator = ServiceLocator.ForSceneOf(this);
    sceneLocator.Register<IConfig>(new LevelConfig());
}
```

3) Use in any MonoBehaviour:
```csharp
public class Player : MonoBehaviour
{
    private IAudioService _audio;
    private IConfig _config;

    void Awake()
    {
        var locator = ServiceLocator.For(this); // Local → Scene → Global
        _audio = locator.Get<IAudioService>();  // from Global if not provided by Local/Scene
        _config = locator.Get<IConfig>();       // from Scene as registered in step 2
    }

    void OnDamage()
    {
        _audio.Play("hit");
    }
}
```

---

## Best Practices
- Favor narrower scopes for easier lifecycle management: use Local/Scene when a service is tied to a GameObject/scene.
- Global is ideal for platform-wide services: logging, analytics, save system, global config.
- When overriding a service in a narrower scope, ensure it is intentional (the same T may exist with different instances per scope).
- For async or disposable resources, unregister/cleanup at the appropriate scene/object lifecycle hooks.
