using System;
using System.Threading.Tasks;

public abstract partial class Component
{

	protected void Register<TSignal>(Action<TSignal> onSignalRaised)
		where TSignal : Signal
	{
		var signalContext = Parent.GetSignalContext<TSignal>();
		signalContext.Register(onSignalRaised);
	}

	protected void Register<TSignal>(Func<TSignal, Task> onSignalRaised)
		where TSignal : Signal
	{
		var signalContext = Parent.GetSignalContext<TSignal>();
		signalContext.Register(onSignalRaised);
	}

	/// <summary>
	/// Get the signal raise context for our parent entity for the signal that we are trying to raise
	/// </summary>
	public SignalContext<TSignal> GetSignalRaiseContext<TSignal>()
		where TSignal : Signal
	{
		return Parent.GetSignalContext<TSignal>();
	}

}