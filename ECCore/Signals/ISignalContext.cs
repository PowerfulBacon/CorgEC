namespace ECCore.Signals
{
	public interface ISignalContext
	{
		void UnregisterAll();
	}

	internal interface ISignalInternalContext
	{
		void InternalUnregister();
	}
}