using System;
using System.Threading.Tasks;

namespace ECCore.Signals
{

	/// <summary>
	/// Connects a specific signal type to a specific entity.
	/// Per-entity singleton
	/// </summary>
	/// <typeparam name="TSignal"></typeparam>
	public class SignalContext<TSignal>
		where TSignal : Signal
	{

#if NET6_0_OR_GREATER
		public Func<TSignal, Task>? onRaisedAsync;
		public Action<TSignal>? onRaised;
#else
		public Func<TSignal, Task> onRaisedAsync;
		public Action<TSignal> onRaised;
#endif

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
}