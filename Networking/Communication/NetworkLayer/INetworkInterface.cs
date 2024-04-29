using Assets.Code.Networking.Serialisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Networking.Communication.NetworkLayer
{
    public interface INetworkInterface : INetworkObjectTracker
    {

        /// <summary>
        /// Is this network interface the host?
        /// </summary>
        bool IsHost { get; }

        /// <summary>
        /// Recieve bytes from the network
        /// </summary>
        Action<byte[], int, int> OnRecieveBytes { set; }

        /// <summary>
        /// Called when the network interface is disconnected
        /// </summary>
        Action OnDisconnect { set; }

        /// <summary>
        /// Send bytes over the network interface.
        /// </summary>
        /// <param name="bytes"></param>
        void SendBytes(byte[] bytes);

        int BytesSent { get; }

        int BytesRecieved { get; }

    }
}
