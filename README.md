# CorgEC

CorgEC is a framework for registering and raising generic signals against entities, and provides an optional implementation of the Entity/Component pattern which you can use.

Importantly, this framework manages groups which allow for quickly deregistering signals, such as when a component is destroyed but its entity still exists or when events need to be registered against a different entity while the registering entity is alive.

# Getting Started: Signalling Base

If you don't want to use the included Entity/Component framework, then you can use the signals as a standalone system for your own purposes.

## Signal Holder

The `SignalHolder` class contains the logic for allowing signals to be registered against it. To use it, either make a class which derives from it, or use it standalone as a seperate variable.

```cs
public SignalHolder signalManager = new SignalHolder();
```

Or:

```cs
public class CustomEntity : SignalHolder<CustomEntity>
{
	// ...
}
```

`SignalHolder` optionally requires a generic parameter when being used in its simple variable form. The generic parameter is the type of the entity that events are being raised against. In the non-generic form, when an event is raised, the source of the event is simply a `SignalHolder`. If you create a class that implements `SignalHolder<>`, then instead of being passed a `SignalHolder`, you will get a reference to your class implementing `SignalHolder<>` instead.

## Registering Signals

To register a signal, you need a signal group. A signal group represents the thing which is registering a signal, which is usually some form of a component or an independant system. You can create a signal group with `new`.

```cs

// Create a signal group
public SignalGroup group = new SignalGroup();

public void RemoveAllSignals()
{
	// Clears all signals, but allows us to register more
	group.ClearSignals();
}

// Optionally dispose the group, which clears all the signals registered to the group
// You can also call ClearSignals if you want to keep using the group.
void Dispose()
{
	group.Dispose();
}

```

Now that we have a group, we can register signals against any `SignalHolder`. You will need to create a class/struct to represent your signal, this has no requirements and doesn't need to implement any interfaces.

```cs
public class FooSignal
{
	public int bar;
}
```

```cs
public void WatchTarget(SignalHolder target)
{
	target.RegisterSignal<FooSignal>(group, OnFoo);
}

private void OnFoo(SignalHolder source, FooSignal signal)
{
	// Do something when we get a foo signal
}
```

## Raising Signals

To raise a signal against a signal holder, simply call `RaiseSignal`.

```cs
public void RaiseSignal(SignalHolder target)
{
	// Create a signal and populate it with some data
	FooSignal signal = new FooSignal();
	signal.bar = 6;

	// Raise the signal against the target
	target.RaiseSignal(signal);
}
```

## Destroying a signal holder

When a signal holder is 'destroyed' (depends on the context of your usage, but is usually when a game entity is deleted from the world), you should call `Dispose`.

If you do not call dispose, then if other entities register signals against the destroyed entity, the destroyed entity will not be able to be garbage collected until the other entities are also garbage collectable.

# Getting Started: Entity/Component Framework

Registering and raising signals against an entity is exactly the same as the previous section, `ECEntity` derives from `SignalContext<>`. This section will cover creating and managing components.

## Creating an entity

To get started, you will need to define an entity type. If you are creating a new project, then you will probably want to create your own core entity class which derives from ECEntity, as followed:

```cs
internal class GameEntity : ECEntity<GameEntity>
{

	public GameEntity() {
		// Note: You have to call initialize.
		// You may want to move this call to after when you add components to an entity, because
		// if two components depend on each other they must be added before initialise is called
		// otherwise the component add will fail (because the dependency does not exist).
		Initialise();
	}

	// Include any fields that you want to use in your game entities.
	// These are directly accessible by all components.

}
```

## Creating a component

Create a class that inherits from component, and implement the initialise method.
You may also implement an override for `ComponentRemoved` if you want to perform some behaviour when the component is removed from an entity.

The component base class must derive from the type of the entity that you are using.

```cs
internal class ExampleComponent : Component<GameEntity>
{
    protected override void Initialise()
    {
        
    }
}
```

## Getting a dependency

Some components may depend on other components for their function. You can use the dependency attribute to get these dependency components.
Dependencies are validated by entities to ensure that they exist.

```cs
internal class ExampleComponent : Component<GameEntity>
{

    [Dependency]
    private OtherComponent _other;

    protected override void Initialise()
    {
        
    }
}
```

## Registering signals to the parent

You can quickly register a signal from the parent entity. When that signal is sent to the parent, it will be triggered on this component.

```cs
internal class ExampleComponent : Component<GameEntity>
{

    [Dependency]
    private OtherComponent _other;

    protected override void Initialise()
    {
		// In this case, source is always the parent
		// but we could also register signals against a different
		// entity if we wanted.
        Register<ExampleSignal>((source, signal) =>
        {
            // Do something
        });
    }
}
```
