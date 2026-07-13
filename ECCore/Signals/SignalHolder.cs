using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECCore.Signals
{
	public class SignalHolder
	{

		#region Signal Contexts

		private Dictionary<Type, object> signalContexts = new Dictionary<Type, object>();

		public SignalContext<TSignal> GetSignalContext<TSignal>()
			where TSignal : Signal
		{
			if (signalContexts.TryGetValue(typeof(TSignal), out var result))
			{
				return (SignalContext<TSignal>)result;
			}

			var signalContext = new SignalContext<TSignal>();
			signalContexts.Add(typeof(TSignal), signalContext);
			return signalContext;
		}

		/// <summary>
		/// Raise a signal against the entity, going through the signal context
		/// implicitly.
		/// </summary>
		/// <typeparam name="TSignal">
		/// The type of the signal being raised against the target.
		/// </typeparam>
		/// <param name="signal">
		/// The signal being raised against the target.
		/// </param>
		/// <returns>
		/// Returns a task that represents the execution of the signal, as some
		/// handlers may take time to execute.
		/// </returns>
		public Task RaiseSignal<TSignal>(TSignal signal)
			where TSignal : Signal
		{
			if (signalContexts.TryGetValue(typeof(TSignal), out var value))
			{
				return ((SignalContext<TSignal>)value).Raise(signal);
			}
			return Task.CompletedTask;
		}

		#endregion

	}
}
