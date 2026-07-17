using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECCore.Signals
{
	public class SignalHolder
	{

		private struct SignalTypeAndGroup
		{
			private SignalGroup group;
			private Type type;

			public SignalTypeAndGroup(SignalGroup group, Type type)
			{
				this.group = group;
				this.type = type;
			}

			public override bool Equals(object obj)
			{
				return obj is SignalTypeAndGroup group &&
					   EqualityComparer<SignalGroup>.Default.Equals(this.group, group.group) &&
					   EqualityComparer<Type>.Default.Equals(type, group.type);
			}

			public override int GetHashCode()
			{
				int hashCode = 2104381881;
				hashCode = hashCode * -1521134295 + EqualityComparer<SignalGroup>.Default.GetHashCode(group);
				hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(type);
				return hashCode;
			}
		}

		#region Signal Contexts

		private Dictionary<Type, object> signalContextLists = new Dictionary<Type, object>();

		private Dictionary<SignalTypeAndGroup, WeakReference> signalContextLookup = new Dictionary<SignalTypeAndGroup, WeakReference>();

		private LinkedSignalContextList<TSignal> GetSignalContextList<TSignal>(SignalGroup group)
		{
			if (signalContextLists.TryGetValue(typeof(TSignal), out var result))
			{
				return (LinkedSignalContextList<TSignal>)result;
			}

			var signalContext = new LinkedSignalContextList<TSignal>();
			signalContextLists.Add(typeof(TSignal), signalContext);
			return signalContext;
		}

		public SignalContext<TSignal> GetSignalContext<TSignal>(SignalGroup group)
		{
			var cacheKey = new SignalTypeAndGroup(group, typeof(TSignal));
			// Fast cache access
			if (signalContextLookup.TryGetValue(cacheKey, out var cachedValue))
			{
				// Object can no longer be garbage collected once we access it
				var cachedTarget = cachedValue.Target;
				if (cachedTarget != null)
				{
					return (SignalContext<TSignal>)cachedTarget;
				}
			}
			// Create new signal
			var linkedList = GetSignalContextList<TSignal>(group);
			// Create the new signal context
			var createdSignalContext = new SignalContext<TSignal>(linkedList, group);
			signalContextLookup[cacheKey] = new WeakReference(createdSignalContext);
			return createdSignalContext;
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
		{
			if (signalContextLists.TryGetValue(typeof(TSignal), out var value))
			{
				var linkedList = ((LinkedSignalContextList<TSignal>)value);
				var current = linkedList.head;
				while (current != null)
				{
					current.Raise(signal);
					current = current.next;
				}
			}
			return Task.CompletedTask;
		}

		#endregion

	}
}
