using System.Threading.Tasks;

public interface ISignalRaiseContext<TSignal> where TSignal : Signal<TSignal>
{
    Task Raise(TSignal signal);
}