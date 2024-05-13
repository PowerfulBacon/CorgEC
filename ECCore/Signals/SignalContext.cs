
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

    /// <summary>
    /// Who this signal gets relayed to
    /// </summary>
    internal RunOn dispatchTo;

    public SignalContext(Instance instance)
    {
        this.instance = instance;
    }

    public Task Raise(TSignal signal)
    {
        onRaised?.Invoke(signal);
        var result = onRaisedAsync?.Invoke(signal) ?? Task.CompletedTask;
        // Network the signal
        return result;
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
