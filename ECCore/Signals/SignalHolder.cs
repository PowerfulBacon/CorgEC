using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECSCore.Signals
{
	public class SignalHolder
	{

		#region Signal Contexts

		private Dictionary<Type, object> signalContexts = new Dictionary<Type, object>();

		public SignalContext<TSignal> GetSignalContext<TSignal>()
			where TSignal : Signal<TSignal>
        {
			if (signalContexts.TryGetValue(typeof(TSignal), out var result))
				return (SignalContext<TSignal>)result;
			SignalContext<TSignal> signalContext = new SignalContext<TSignal>();
			signalContexts.Add(typeof(TSignal), signalContext);
			return signalContext;
		}

		#endregion

	}
}
