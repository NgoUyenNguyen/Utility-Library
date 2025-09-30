# Extension Attributes
---

## Abstract and Interface Serialize Attributes

By default, Unity's serialization system support serializing classes and structs to display in the inspector and store
in disks
if they have the `Serializable` attribute.

```csharp
[Serializable]
public class ExampleClass
{
    public int value;
}
```

However, what if we want to serialize an abstract class or an interface?
Unity's serialization system does not support serializing abstract classes or interfaces. But after Unity 2019.3,
we can use the `SerializeReference` attribute to serialize abstract classes and interfaces, however, it can not be
edited in the inspector.
To solve this, you can use attibute `SubclassSelector` to allow selecting a subclass to serialize from the inspector.

```cSharp
public class TestMonoBehaviour : MonoBehaviour
{
    [Serializable]
    public class ExampleConcreteClass1 : ExampleAbstractClass, ExampleInterface
    {
        public override int value => 10;
        
        public void DoSomething()
        {
            Debug.Log("Do something 1");
        }
    }
    
    [Serializable]
    public class ExampleConcreteClass2 : ExampleAbstractClass, ExampleInterface
    {
        public override int value => 20;
        
        public void DoSomething()
        {
            Debug.Log("Do something 2");
        }   
    }
    
    [SerializeReference, SubclassSelector]
    public ExampleAbstractClass example; // Now in inspector, you can select a subclass to serialize.
    
    [SerializeReference, SubclassSelector]
    public ExampleInterface[] exampleArray; // It also works for arrays and lists. So now each element in the array can be a different subclass.
}

public abstract class ExampleAbstractClass
{
    public virtual int value;
}

public interface ExampleInterface
{
    void DoSomething();
}
```

If you want to cleanerly group the subclasses together, you can use the `AddTypeMenu` attribute in the concrete class to
define its group name.

```cSharp

    [Serializable, AddTypeMenu("Example/ExampleConcreteClass1")]
    public class ExampleConcreteClass1 : ExampleAbstractClass, ExampleInterface
    {
        public override int value => 10;
    }
    
    [Serializable, AddTypeMenu("Example/ExampleConcreteClass2")] 
    public class ExampleConcreteClass2 : ExampleAbstractClass, ExampleInterface
    {
        public override int value => 20;
    }
    
    // Now both ExampleConcreteClass1 and ExampleConcreteClass2 will be inside the "Example" group in the inspector dropdown menu.
```

**Notice: If the type is renamed, the reference is lost.**  
It is a limitation of `SerializeReference` of Unity.  
When serializing a `SerializeReference` reference, the type name, namespace, and assembly name are used, so if any of
these are changed, the reference cannot be resolved during deserialization.  
To solve this problem, `UnityEngine.Scripting.APIUpdating.MovedFromAttribute` can be used.  
Also, this [thread](https://discussions.unity.com/t/serializereference-data-loss-when-class-name-is-changed/756470) will
be helpful.
