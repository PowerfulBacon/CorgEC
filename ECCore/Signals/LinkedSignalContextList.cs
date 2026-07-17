namespace ECCore.Signals
{

	internal class LinkedSignalContextList<TContext, TSignal>
	{

		internal TContext contextReference;

		internal SignalContext<TContext, TSignal> head;
		internal SignalContext<TContext, TSignal> tail;

		public LinkedSignalContextList(TContext contextReference)
		{
			this.contextReference = contextReference;
		}
	}
}
