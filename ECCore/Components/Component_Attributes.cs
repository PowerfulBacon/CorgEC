using ECCore.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ECCore.Components
{
	public abstract partial class Component<TSelf> : IComponent
		where TSelf : Component<TSelf>
	{
		/// <summary>
		/// Does this component require the entity to be owned in order to run?
		/// </summary>
		public static bool RequiresOwnership = typeof(TSelf).GetCustomAttribute(typeof(RequiresOwnershipAttribute)) != null;
	}
}
