using ECCore.Attributes;
using System.Threading.Tasks;

public interface ISignalRaiseContext<TSignal> where TSignal : Signal<TSignal>
{
    void Raise(TSignal signal);
}