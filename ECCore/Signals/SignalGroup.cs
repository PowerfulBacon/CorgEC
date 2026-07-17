using System;
using System.Collections.Generic;

namespace ECCore.Signals
{
	public sealed class SignalGroup : IDisposable
	{

		internal List<ISignalInternalContext> signals = new List<ISignalInternalContext>();

		private bool _disposed = false;

		public void ClearSignals()
		{
			if (_disposed)
				throw new ObjectDisposedException($"Attempting to call {nameof(Dispose)} a disposed object.");
			foreach (var signal in signals)
			{
				signal.InternalUnregister();
			}
			signals.Clear();
		}

		public void Dispose()
		{
			if (_disposed)
				throw new ObjectDisposedException($"Attempting to call {nameof(Dispose)} a disposed object.");
			_disposed = true;
			signals = null;
			ClearSignals();
		}

	}
}
