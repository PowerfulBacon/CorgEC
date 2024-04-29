
using ECCore.Attributes;
using System.Reflection;

public abstract class Signal<TSignal>
    where TSignal : Signal<TSignal>
{

    /// <summary>
    /// Can the signal that we are referencing be raised from a client?
    /// </summary>
    public static bool IsAllowedFromClient { get; } = ((typeof(TSignal).GetCustomAttribute<AcceptAttribute>()?.SignalFlags ?? SignalFlags.SERVER_ONLY) & SignalFlags.FROM_CLIENT) != 0;

    /// <summary>
    /// Has the signal been dispatched over the networking yet?
    /// </summary>
    internal bool isDispatched = false;

}
