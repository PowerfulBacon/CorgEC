using Assets.Code.Networking.Communication.NetworkLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Networking.Serialisation
{
	internal static class NetworkSerialisedID
	{
		internal static uint CurrentNetworkID = 1;
	}

    public interface INetworkedSerialised
    {

        /// <summary>
        /// We need to be able to set this internally
        /// </summary>
        uint NetworkID { get; set; }

        /// <summary>
        /// Serialise this object into the binary writer.
        /// </summary>
        /// <param name="writer"></param>
        void Serialise(INetworkInterface target, BinaryWriter writer);

        /// <summary>
        /// Read this object from the provided binary reader.
        /// </summary>
        /// <param name="reader"></param>
        void Deserialise(INetworkInterface sender, INetworkObjectTracker localObjects, BinaryReader reader);

    }
}
