using System;
using System.Collections.Generic;
using System.Text;

namespace ECCore.Components
{
	public abstract partial class Component<TSelf> : IComponent
		where TSelf : Component<TSelf>
	{
	}
}
