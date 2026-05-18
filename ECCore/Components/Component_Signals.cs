using ECCore.Signals;
using System;
using System.Threading.Tasks;

namespace ECCore.Components
{

	public abstract partial class Component<TParent>
	{

		protected void Register<TSignal>(Action<TSignal> onSignalRaised)
			where TSignal : Signal
		{
			if (Parent == null)
			{
				throw new InvalidOperationException("Cannot register signals when the parent of a component has not been assigned.");
			}

			SignalContext<TSignal> signalContext = Parent.GetSignalContext<TSignal>();
			signalContext.Register(onSignalRaised);
		}

		protected void Register<TSignal>(Func<TSignal, Task> onSignalRaised)
			where TSignal : Signal
		{
			if (Parent == null)
			{
				throw new InvalidOperationException("Cannot register signals when the parent of a component has not been assigned.");
			}

			SignalContext<TSignal> signalContext = Parent.GetSignalContext<TSignal>();
			signalContext.Register(onSignalRaised);
		}

		/// <summary>
		/// Get the signal raise context for our parent entity for the signal that we are trying to raise
		/// </summary>
		public SignalContext<TSignal> GetSignalRaiseContext<TSignal>()
			where TSignal : Signal
		{
			if (Parent == null)
			{
				throw new InvalidOperationException("Cannot register signals when the parent of a component has not been assigned.");
			}

			return Parent.GetSignalContext<TSignal>();
		}

	}
}