using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECCore.Signals
{

	/// <summary>
	/// Connects a specific signal type to a specific entity.
	/// Per-entity singleton
	/// </summary>
	/// <typeparam name="TContext">The type that this context exists against</typeparam>
	/// <typeparam name="TSignal">The signal that we will be accepting</typeparam>
	public class SignalContext<TContext, TSignal> : ISignalContext, ISignalInternalContext
	{

		/// <summary>
		/// The list that we need to belong to
		/// </summary>
		private LinkedSignalContextList<TContext, TSignal> parentList;

		/// <summary>
		/// The group that we belong to
		/// </summary>
		internal SignalGroup group;

		/// <summary>
		/// The next signal context in the linked list
		/// </summary>
		internal SignalContext<TContext, TSignal> next;

		/// <summary>
		/// Have we been registered?
		/// </summary>
		private bool registered = false;

		private List<Action<TContext, TSignal>> baseActions = null;

		private List<Func<TContext, TSignal, Task>> asyncActions = null;

		/// <summary>
		/// Returns true if we have any asynchronous actions that may require awaiting on the
		/// Raise call.
		/// </summary>
		public bool IsAsynchronous => asyncActions != null;

		/// <summary>
		/// Add/Remove signals associated with the synchronous raised event.
		/// </summary>
		public event Action<TContext, TSignal> onRaised
		{
			add => Register(value);
			remove => Unregister(value);
		}

		/// <summary>
		/// Add/Remove signals associated with the asynchronous raised event.
		/// </summary>
		public event Action<TContext, TSignal> onRaisedAsync
		{
			add => Register(value);
			remove => Unregister(value);
		}

		/// <summary>
		/// Create a signal context
		/// </summary>
		/// <param name="parentList"></param>
		/// <param name="group"></param>
		internal SignalContext(LinkedSignalContextList<TContext, TSignal> parentList, SignalGroup group)
		{
			this.parentList = parentList;
			this.group = group;
		}

		public Task Raise(TSignal signal)
		{
			if (baseActions != null)
			{
				for (int i = baseActions.Count - 1; i >= 0; i--)
				{
					if (baseActions.Count >= i)
					{
						baseActions[i].Invoke(parentList.contextReference, signal);
					}
				}
			}
			// Execute asynchronous actions
			if (asyncActions != null)
			{
				return Task.WhenAll(asyncActions.Select(x => x(parentList.contextReference, signal)).ToArray());
			}
			// Execute synchronousely
			return Task.CompletedTask;
		}

		void ISignalInternalContext.InternalUnregister()
		{
			asyncActions = null;
			baseActions = null;
			if (registered)
			{
				if (parentList.head == this)
				{
					parentList.head = next;
				}
				if (parentList.tail == this)
				{
					parentList.tail = parentList.head;
					// Recurse until we reach the new final element
					while (parentList.tail?.next != null)
					{
						parentList.tail = parentList.tail.next;
					}
				}
				registered = false;
			}
		}

		/// <summary>
		/// Clears all signal registrations
		/// </summary>
		public void UnregisterAll()
		{
			((ISignalInternalContext)this).InternalUnregister();
			// Publicly exposed unregisters must also manage the group
			group.signals.Remove(this);
		}

		public void Unregister(Func<TContext, TSignal, Task> action)
		{
			if (asyncActions == null)
			{
				return;
			}
			asyncActions.Remove(action);
			// Clear unnecessary allocations
			if (asyncActions.Count == 0)
			{
				if (baseActions.Count == 0)
				{
					UnregisterAll();
				}
				else
				{
					asyncActions = null;
				}
			}
		}

		public void Unregister(Action<TContext, TSignal> action)
		{
			if (baseActions == null)
			{
				return;
			}
			baseActions.Remove(action);
			// Clear unnecessary allocations
			if (baseActions.Count == 0)
			{
				if (asyncActions.Count == 0)
				{
					UnregisterAll();
				}
				else
				{
					baseActions = null;
				}
			}
		}

		public void Register(Func<TContext, TSignal, Task> action)
		{
			JoinListIfNecessary();
			if (asyncActions == null)
			{
				asyncActions = new List<Func<TContext, TSignal, Task>>();
			}
			asyncActions.Add(action);
		}

		public void Register(Action<TContext, TSignal> action)
		{
			JoinListIfNecessary();
			if (baseActions == null)
			{
				baseActions = new List<Action<TContext, TSignal>>();
			}
			baseActions.Add(action);
		}

		private void JoinListIfNecessary()
		{
			if (!registered)
			{
				// Join linked raising list
				if (parentList.tail != null)
				{
					parentList.tail.next = this;
				}
				parentList.tail = this;
				if (parentList.head == null)
				{
					parentList.head = this;
				}

				// Join group
				group.signals.Add(this);

				registered = true;
			}
		}

	}
}