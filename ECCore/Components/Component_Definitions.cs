using ECCore.Attributes;
using ECCore.Instances;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ECCore.Components
{
	public abstract partial class Component<TSelf> : IComponent
		where TSelf : Component<TSelf>
	{

#if NET6_0_OR_GREATER
		Entity? IComponent.Parent { get; set; }
		public Entity? Parent { get => ((IComponent)this).Parent; internal set => ((IComponent)this).Parent = value; }
#else
		Entity IComponent.Parent { get; set; }
		public Entity Parent { get => ((IComponent)this).Parent; internal set => ((IComponent)this).Parent = value; }
#endif

		public Instance Instance => Parent?.Instance ?? throw new NullReferenceException("Attempting to access the instance of a component that has been disconnected from the world.");

		void IComponent._Initialise()
		{
			SetupSignals();
            Initialise();
		}

		/// <summary>
		/// Initialise this component and register the appropriate signals.
		/// </summary>
		protected internal virtual void Initialise()
		{ }

		void IComponent._Remove()
		{
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