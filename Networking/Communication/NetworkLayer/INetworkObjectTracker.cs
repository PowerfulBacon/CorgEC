using Assets.Code.Networking.Serialisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Networking.Communication.NetworkLayer
{
    public interface INetworkObjectTracker
    {

        /// <summary>
        /// The set of objects that this client knows about, when we send them an object
        /// it will be added to this list so that we know we don't have to serialise the entire
        /// object again.
        /// </summary>
        IReadOnlyDictionary<uint, INetworkedSerialised> KnownObjects { get; }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Internally accessed only: Allows us to add to the accessible objects list
        /// </summary>
        internal IDictionary<uint, INetworkedSerialised> _AccessibleObjects { get; }
#else
        IDictionary<uint, INetworkedSerialised> _AccessibleObjects { get; }
#endif

    }
}
