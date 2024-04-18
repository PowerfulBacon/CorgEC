# CorgEC

A lightweight EC library for C#.

# Components

## Creating a component

Create a class that inherits from component, and implement the initialise method.
You may also implement an override for `ComponentRemoved` if you want to perform some behaviour when the component is removed from an entity.

```cs
internal class ExampleComponent : Component
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
internal class ExampleComponent : Component
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
internal class ExampleComponent : Component
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

# Entities

## Creating an entity

Creating an entity is done through a wrapper to ensure that all components are initialised at the same time, allowing for components to depend on each other.

```cs
Entity.Create(entity =>
{
    // Add components here
    entity.AddComponent(new ExampleComponent());
});
```
