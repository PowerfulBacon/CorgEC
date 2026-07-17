using ECCore.Signals;
using System;
using System.Threading.Tasks;

namespace ECCore.Components
{

	public abstract partial class Component<TParent>
	{

		private SignalGroup componentGroup = new SignalGroup();

		protected void Register<TSignal>(Action<TParent, TSignal> onSignalRaised)
		{
			if (Parent == null)
			{
				throw new InvalidOperationException("Cannot register signals when the parent of a component has not been assigned.");
			}

			SignalContext<TParent, TSignal> signalContext = Parent.GetSignalContext<TSignal>(componentGroup);
			signalContext.Register(onSignalRaised);
		}

		protected void Register<TSignal>(Func<TParent, TSignal, Task> onSignalRaised)
		{
			if (Parent == null)
			{
				throw new InvalidOperationException("Cannot register signals when the parent of a component has not been assigned.");
			}

			SignalContext<TParent, TSignal> signalContext = Parent.GetSignalContext<TSignal>(componentGroup);
			signalContext.Register(onSignalRaised);
		}

		protected void Unregister<TSignal>(Action<TParent, TSignal> onSignalRaised)
		{
			if (Parent == null)
			{
				throw new InvalidOperationException("Cannot register signals when the parent of a component has not been assigned.");
			}

			SignalContext<TParent, TSignal> signalContext = Parent.GetSignalContext<TSignal>(componentGroup);
			signalContext.Unregister(onSignalRaised);
		}

		protected void Unregister<TSignal>(Func<TParent, TSignal, Task> onSignalRaised)
		{
			if (Parent == null)
			{
				throw new InvalidOperationException("Cannot register signals when the parent of a component has not been assigned.");
			}

			SignalContext<TParent, TSignal> signalContext = Parent.GetSignalContext<TSignal>(componentGroup);
			signalContext.Unregister(onSignalRaised);
		}

		/// <summary>
		/// Get the signal raise context for our parent entity for the signal that we are trying to raise
		/// </summary>
		public SignalContext<TParent, TSignal> GetSignalRaiseContext<TSignal>()
		{
			if (Parent == null)
			{
				throw new InvalidOperationException("Cannot register signals when the parent of a component has not been assigned.");
			}

			return Parent.GetSignalContext<TSignal>(componentGroup);
		}

	}
}