
using Assets.Code.Networking.Communication.ApplicationLayer;
using Assets.Code.Networking.Communication.NetworkLayer;
using Assets.Code.Networking.Communication.Packets;
using Assets.Code.Networking.Serialisation;
using ECCore.Attributes;
using ECCore.Instances;
using ECSCore.Signals;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

/// <summary>
/// Connects a specific signal type to a specific entity.
/// Per-entity singleton.
/// </summary>
/// <typeparam name="TSignal"></typeparam>
public class SignalContext<TSignal> : ISignalRaiseContext<TSignal>
    where TSignal : Signal<TSignal>
{

    public Action<TSignal> onRaisedLocal;

    private Instance instance;

    /// <summary>
    /// Who this signal gets relayed to
    /// </summary>
    internal RunOn dispatchTo;

    /// <summary>
    /// Who will we accept this signal from, if someone tells us to raise a signal but they aren't in this list then we are rejecting them.
    /// </summary>
    internal AcceptFrom acceptFrom;

    /// <summary>
    /// If we are a signal context from an entity, we need to know about that entity in order to network it
    /// </summary>
    private Entity underlyingEntity;

	public SignalContext(Instance instance)
    {
        this.instance = instance;
    }

    public SignalContext(Instance instance, Entity underlyingEntity) : this(instance)
    {
        this.underlyingEntity = underlyingEntity;
	}

    public void Raise(TSignal signal)
    {
		onRaisedLocal?.Invoke(signal);
        // We need to dispatch the signal, to who is a good question
        if (dispatchTo != 0 && instance.NetworkManager != null && underlyingEntity != null)
        {
            // If this runs on the server or the clients, we know that we have to send this to everyone
            // To send to the server, we have to send to the server
            // To send the clients, we have to send to the server or to everyone
            if ((dispatchTo & RunOn.Server) != 0 && instance.IsClientInstance())
            {
                signal.SendToHost(instance.NetworkManager);
			}
            else if ((dispatchTo & (RunOn.Client | RunOn.Everyone)) != 0)
			{
				signal.SendToOthers(instance.NetworkManager);
			}
            else if ((dispatchTo & RunOn.Owner) != 0)
            {

            }
        }
    }

    internal void Register(Action<TSignal> action)
    {
		onRaisedLocal += action;
    }

}
