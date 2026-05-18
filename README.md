# CorgEC

A lightweight EC library for C#.

# Components

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
        Register<ExampleSignal>(signal =>
        {
            // Do something
        });
    }
}
```

# Signals

## Registering a signal

Anything that inherits from `SignalHolder` can have signals registered and raised against it. Entities, for example, inherit from SignalHolder.

```cs
target.GetSignalContext<ExampleSignal>().Register(signal =>
{
    // Do something
});
```

## Raising a signal

If you are going to be raising a signal a lot, caching the signal context will provide the best performance.

```cs
// Raise a signal
target.GetSignalContext<ExampleSignal>().Raise(new ExampleSignal())
```
