namespace ECCore.Signals
{

	internal class LinkedSignalContextList<TSignal>
	{

		internal SignalContext<TSignal> head;
		internal SignalContext<TSignal> tail;

	}
}
