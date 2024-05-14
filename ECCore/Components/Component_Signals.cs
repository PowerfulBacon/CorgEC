using Assets.Code.Networking.Communication.ApplicationLayer;
using Assets.Code.Networking.Communication.NetworkLayer;
using Assets.Code.Networking.Communication.Packets;
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
			// Check if we need to run the signal
			if (((runOn & RunOn.Owner) != 0 && Parent.IsLocalOwner())
				|| ((runOn & RunOn.Server) != 0 && Instance.IsHostInstance())
				|| (runOn & RunOn.Everyone) != 0
				|| ((runOn & RunOn.Client) != 0 && Instance.IsClientInstance())
				|| (runOn & RunOn.Self) != 0)
			{
				var signalContext = Parent.GetSignalContext<TSignal>();
				signalContext.Register(onSignalRaised);
			}
			// We are the host, and we need to relay this msesage onto the client that should be handling it.
			// Since host messages aren't automatically sent to clients, we need to mark it as needing to be
			// sent.
			// Multiple components registering the same signal will automatically handle this.
			if ((runOn & RunOn.Everyone | RunOn.Client) != 0 && Instance.IsHostInstance())
			{
                var signalContext = Parent.GetSignalContext<TSignal>();
				signalContext.dispatchTo |= runOn;
            }
			// We are a client and we want to run it on other clients, or the server
			if ((runOn & RunOn.Everyone | RunOn.Client | RunOn.Server) != 0 && !Instance.IsHostInstance())
            {
                var signalContext = Parent.GetSignalContext<TSignal>();
                signalContext.dispatchTo |= runOn;
            }
			// We want to run it on the owner, but we don't own it
			if ((runOn & RunOn.Owner) != 0 && !Parent.IsLocalOwner())
            {
                var signalContext = Parent.GetSignalContext<TSignal>();
                signalContext.dispatchTo |= runOn;
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

    public class NetworkSignal : Packet<NetworkSignal>
    {
        protected override void Recieve(NetworkManager localNetworkManager, INetworkInterface sender, double sendTime)
        {
            throw new NotImplementedException();
        }
    }
}