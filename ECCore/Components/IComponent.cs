using Assets.Code.Networking.Serialisation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECCore.Components
{
	public interface IComponent : INetworkedSerialised
    {
#if NET6_0_OR_GREATER
		Entity? Parent { get; internal set; }

		internal bool _SetupDependencies();

		internal void _Initialise();

		internal void _Remove();
#else
		Entity Parent { get; set; }

		bool _SetupDependencies();

		void _Initialise();

		void _Remove();
#endif
	}
}
