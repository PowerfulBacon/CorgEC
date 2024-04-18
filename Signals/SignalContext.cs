
using ECSCore.Signals;
using System;
using System.Threading.Tasks;

/// <summary>
/// Connects a specific signal type to a specific entity.
/// Per-entity singleton
/// </summary>
/// <typeparam name="TSignal"></typeparam>
public class SignalContext<TSignal>
	where TSignal : Signal
{

	public Func<TSignal, Task>? onRaisedAsync;
	public Action<TSignal>? onRaised;

	public Task Raise(TSignal signal)
	{
		onRaised?.Invoke(signal);
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
