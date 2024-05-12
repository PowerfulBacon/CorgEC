using ECCore.Attributes;
using System;
using System.Threading.Tasks;

namespace ECCore.Components
{
	public abstract partial class Component<TSelf> : IComponent
		where TSelf : Component<TSelf>
	{

		/// <summary>
		/// Find the signal attributes and register them
		/// </summary>
		private void SetupSignals()
		{
			foreach (var signal in RegisteredSignals)
			{
				signal.registrationAction.Invoke(this);
			}
		}

		protected void Register<TSignal>(Action<TSignal> onSignalRaised, AcceptFrom acceptFrom, RunOn runOn)
			where TSignal : Signal<TSignal>
		{
			if (Parent == null)
				throw new InvalidOperationException("Cannot register signals when the parent of a component has not been assigned.");
			// TODO: Should we be doing this at the signal registrar level instead of the entity level?
			//		 That would imply that signals registrars all need to have the concept of ownership.
			//		 oh yea btw, entities that are fully local should be owned by the local concept and not communicated
			// Cases:
			// Server Component, Server Signal:
			//		Register normally
			// Client Component, Client Signal:
			//		Register normally
			// Client Component, Server Signal:
			//		Server sends the signal to the client that should handle this signal.
			//		As the server, we need to ensure that we are still listening to this signal so that we can relay
			//		it to the client.
			//		As a client, we we handle this signal normally.
			// Server Component, Client Signal:
			//		All client signals are sent to the server when they are raised. This is handled by SignalContext.
			//		So we need to do nothing.
			//		As the server, we simply need to handle this as we would expect.
			// We need to operate directly on the signals
			//
			// If this component requires ownership and we are the owner, operate.
			// Otherwise, operate if we are the host.
			// This means that we will handle all signals that we are meant to handle, however to handle
			// a signal, we need to know about it. This is why we need to register special cases.
			if (((runOn & RunOn.Owner) != 0 && Parent.IsLocalOwner())
				|| ((runOn & RunOn.Server) != 0 && Instance.IsHostInstance())
				|| (runOn & RunOn.Everyone) != 0
				|| ((runOn & RunOn.Client) != 0 && !Instance.IsHostInstance()))
			{
				var signalContext = Parent.GetSignalContext<TSignal>();
				signalContext.Register(onSignalRaised);
			}
			// We are the host, and we need to relay this msesage onto the client that should be handling it.
			// Since host messages aren't automatically sent to clients, we need to mark it as needing to be
			// sent.
			// Multiple components registering the same signal will automatically handle this.
			else if ((runOn & RunOn.Everyone | RunOn.Client | RunOn.Owner) != 0 && !Parent.IsLocalOwner() && Instance.IsHostInstance())
			{
				// TODO: Mark the signal as needing dispatch to the client that owns our parent
			}
		}

		/// <summary>
		/// Get the signal raise context for our parent entity for the signal that we are trying to raise.
		/// You are only allowed to raise on this context, for registration use the proper Register() functions
		/// otherwise networking will break miserably.
		/// </summary>
		public ISignalRaiseContext<TSignal> GetSignalRaiseContext<TSignal>()
			where TSignal : Signal<TSignal>
        {
			if (Parent == null)
				throw new InvalidOperationException("Cannot register signals when the parent of a component has not been assigned.");
			return Parent.GetSignalContext<TSignal>();
		}

	}
}