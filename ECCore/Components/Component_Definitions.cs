using ECCore.Entities;


namespace ECCore.Components
{
	public abstract partial class Component<TParent>
		where TParent : ECEntity<TParent>
	{

#if NET6_0_OR_GREATER
		public TParent? Parent { get; internal set; }
#else
		public TParent Parent { get; internal set; }
#endif

		/// <summary>
		/// Initialise this component and register the appropriate signals.
		/// </summary>
		protected internal abstract void Initialise();

		internal void Remove()
		{
			// Remove all signals associated with this component
			componentGroup.ClearSignals();
			// Unregister all signals
			// Call component removed
			ComponentRemoved();
			// Remove the parent
			Parent = null;
		}

		/// <summary>
		/// Called when the component is removed from it's parent.
		/// Clean up any side-effects or references.
		/// </summary>
		protected virtual void ComponentRemoved() { }

	}
}