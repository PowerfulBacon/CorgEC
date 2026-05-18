using System;

namespace ECCore.Attributes
{

	/// <summary>
	/// Indicates that we require another component to be attached to
	/// our parent entity for direct communication.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class DependencyAttribute : Attribute
	{

		public bool Optional { get; } = false;

		public DependencyAttribute(bool optional = false)
		{
			Optional = optional;
		}
	}
}