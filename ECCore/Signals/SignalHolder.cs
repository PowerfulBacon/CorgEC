using System;
using System.Collections.Generic;

namespace ECCore.Signals
{
	public class SignalHolder
	{

		#region Signal Contexts

		private Dictionary<Type, object> signalContexts = new Dictionary<Type, object>();

		public SignalContext<TSignal> GetSignalContext<TSignal>()
			where TSignal : Signal
		{
			if (signalContexts.TryGetValue(typeof(TSignal), out object result))
			{
				return (SignalContext<TSignal>)result;
			}

			var signalContext = new SignalContext<TSignal>();
			signalContexts.Add(typeof(TSignal), signalContext);
			return signalContext;
		}

		#endregion

	}
}
