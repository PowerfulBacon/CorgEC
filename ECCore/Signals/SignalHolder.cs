using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECCore.Signals
{

	/// <summary>
	/// Simple signal holder that only includes itself as context
	/// </summary>
	public sealed class SignalHolder : SignalHolder<SignalHolder>
	{ }

	public class SignalHolder<TEntityType> : IDisposable
		where TEntityType : SignalHolder<TEntityType>
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

		private LinkedSignalContextList<TEntityType, TSignal> GetSignalContextList<TSignal>(SignalGroup group)
		{
			if (signalContextLists.TryGetValue(typeof(TSignal), out var result))
			{
				return (LinkedSignalContextList<TEntityType, TSignal>)result;
			}

			var signalContext = new LinkedSignalContextList<TEntityType, TSignal>((TEntityType)this);
			signalContextLists.Add(typeof(TSignal), signalContext);
			return signalContext;
		}

		public SignalContext<TEntityType, TSignal> GetSignalContext<TSignal>(SignalGroup group)
		{
			var cacheKey = new SignalTypeAndGroup(group, typeof(TSignal));
			// Fast cache access
			if (signalContextLookup.TryGetValue(cacheKey, out var cachedValue))
			{
				// Object can no longer be garbage collected once we access it
				var cachedTarget = cachedValue.Target;
				if (cachedTarget != null)
				{
					return (SignalContext<TEntityType, TSignal>)cachedTarget;
				}
			}
			// Create new signal
			var linkedList = GetSignalContextList<TSignal>(group);
			// Create the new signal context
			var createdSignalContext = new SignalContext<TEntityType, TSignal>(linkedList, group);
			signalContextLookup[cacheKey] = new WeakReference(createdSignalContext);
			return createdSignalContext;
		}

		/// <summary>
		/// Register a signal against this signal holder, the onRaised delegate will be called
		/// when that specific signal is raised against this signal holder.
		/// </summary>
		/// <typeparam name="TSignal"></typeparam>
		/// <param name="group"></param>
		/// <param name="onRaisedAsync"></param>
		public void RegisterSignal<TSignal>(SignalGroup group, Func<TEntityType, TSignal, Task> onRaisedAsync)
		{
			GetSignalContext<TSignal>(group).Register(onRaisedAsync);
		}

		/// <summary>
		/// Register a signal against this signal holder, the onRaised delegate will be called
		/// when that specific signal is raised against this signal holder.
		/// </summary>
		/// <typeparam name="TSignal"></typeparam>
		/// <param name="group"></param>
		/// <param name="onRaised"></param>
		public void RegisterSignal<TSignal>(SignalGroup group, Action<TEntityType, TSignal> onRaised)
		{
			GetSignalContext<TSignal>(group).Register(onRaised);
		}

		/// <summary>
		/// Removes a registered signal from this holder, the provided delegate will no longer
		/// be called when TSignal is raised against this entity.
		/// </summary>
		/// <typeparam name="TSignal"></typeparam>
		/// <param name="group"></param>
		/// <param name="onRaisedAsync"></param>
		public void UnregisterSignal<TSignal>(SignalGroup group, Func<TEntityType, TSignal, Task> onRaisedAsync)
		{
			GetSignalContext<TSignal>(group).Unregister(onRaisedAsync);
		}

		/// <summary>
		/// Removes a registered signal from this holder, the provided delegate will no longer
		/// be called when TSignal is raised against this entity.
		/// </summary>
		/// <typeparam name="TSignal"></typeparam>
		/// <param name="group"></param>
		/// <param name="onRaised"></param>
		public void UnregisterSignal<TSignal>(SignalGroup group, Action<TEntityType, TSignal> onRaised)
		{
			GetSignalContext<TSignal>(group).Unregister(onRaised);
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
				var linkedList = ((LinkedSignalContextList<TEntityType, TSignal>)value);
				var current = linkedList.head;
				while (current != null)
				{
					current.Raise(signal);
					current = current.next;
				}
			}
			return Task.CompletedTask;
		}

		/// <summary>
		/// Removes all signals attached to this holder, which may have been registered from
		/// another entity which would maintain a reference and keep this holder alive.
		/// </summary>
		public void Dispose()
		{
			foreach (var reference in signalContextLookup.Values)
			{
				var referenceTarget = reference.Target;
				if (referenceTarget != null)
				{
					var context = (ISignalContext)referenceTarget;
					context.UnregisterAll();
				}
			}
		}

		#endregion

	}
}
