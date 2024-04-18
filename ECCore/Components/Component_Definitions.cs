using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public abstract partial class Component
{

#if NET6_0_OR_GREATER
	public Entity? Parent { get; internal set; }
#else
    public Entity Parent { get; internal set; }
#endif

    /// <summary>
    /// Initialise this component and register the appropriate signals.
    /// </summary>
    protected internal abstract void Initialise();

	internal void Remove()
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
