
using Assets.Code.Networking.Communication.ApplicationLayer;
using Assets.Code.Networking.Communication.NetworkLayer;
using Assets.Code.Networking.Communication.Packets;
using ECCore.Attributes;
using ECCore.Instances;
using System;
using System.Reflection;

public abstract class Signal<TSignal> : Packet<TSignal>
    where TSignal : Signal<TSignal>
{

    //TODO
    /// <summary>
    /// The amount of time that it took for this message to get from
    /// the sender to the handler.
    /// </summary>
    [NetIgnore]
    public TimeSpan SendTime { get; private set; } = TimeSpan.Zero;

    /// <summary>
    /// Who are we sending this to?
    /// </summary>
    internal Entity targetEntity;

    public override PacketSecurityFlags SecurityFlags => PacketSecurityFlags.ALLOW_CLIENT_RAISE | PacketSecurityFlags.ALLOW_HOST_RAISE | PacketSecurityFlags.RAISED_ON_HOST | PacketSecurityFlags.RAISED_ON_CLIENT;

    protected override void Recieve(NetworkManager localNetworkManager, INetworkInterface sender, double sendTime)
    {
        // TODO: Verify that this signal was allowed to be raised by the person sending the message
        targetEntity.GetSignalContext<TSignal>().Raise((TSignal)this);
    }

}
