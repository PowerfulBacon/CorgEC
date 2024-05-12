
using ECCore.Attributes;
using System.Reflection;

public abstract class Signal<TSignal>
    where TSignal : Signal<TSignal>
{

    /// <summary>
    /// Has the signal been dispatched over the networking yet?
    /// </summary>
    internal bool isDispatched = false;

}
