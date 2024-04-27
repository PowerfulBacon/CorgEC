
using ECCore.Attributes;
using ECCore.Instances;
using ECSCore.Signals;
using System;
using System.Reflection;
using System.Threading.Tasks;

/// <summary>
/// Connects a specific signal type to a specific entity.
/// Per-entity singleton
/// </summary>
/// <typeparam name="TSignal"></typeparam>
public class SignalContext<TSignal> : ISignalRaiseContext<TSignal>
    where TSignal : Signal<TSignal>
{

#if NET6_0_OR_GREATER
	public Func<TSignal, Task>? onRaisedAsync;
	public Action<TSignal>? onRaised;
#else
    public Func<TSignal, Task> onRaisedAsync;
    public Action<TSignal> onRaised;
#endif

    private Instance instance;

    public SignalContext(Instance instance)
    {
        this.instance = instance;
    }

    public Task Raise(TSignal signal)
    {
        onRaised?.Invoke(signal);
        // Client signals need to be dispatched to the server
        if (Signal<TSignal>.IsAllowedFromClient && !instance.IsHostInstance())
        {
            // TODO: Handle dispatch to the server
        }
        return onRaisedAsync?.Invoke(signal) ?? Task.CompletedTask;
    }

    internal void Register(Func<TSignal, Task> action)
    {
        onRaisedAsync += action;
    }

    internal void Register(Action<TSignal> action)
    {
        onRaised += action;
    }

}
