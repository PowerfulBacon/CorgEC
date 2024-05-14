using Assets.Code.Networking.Communication.NetworkLayer;
using Assets.Code.Networking.Serialisation;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;

namespace Networking.Communication.NetworkLayer
{
    public class LocalInterface : INetworkInterface
    {

        public bool IsHost { get; }

        public Action<byte[], int, int> OnRecieveBytes { set; get; }
        public Action OnDisconnect { set; get; }

        private Dictionary<uint, INetworkedSerialised> _knownObjects = new Dictionary<uint, INetworkedSerialised>();

        public IReadOnlyDictionary<uint, INetworkedSerialised> KnownObjects => _knownObjects;

        IDictionary<uint, INetworkedSerialised> INetworkObjectTracker._AccessibleObjects => _knownObjects;

        public int BytesSent { get; private set; }

        public int BytesRecieved { get; private set; }

        public LocalInterface Reverse { get; }

        /// <summary>
        /// Constructor will automatically start the task that handles reading the socket
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="isHost"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public LocalInterface(bool isHost)
        {
            IsHost = isHost;
            Reverse = new LocalInterface(!isHost, this);
        }

        private LocalInterface(bool isHost, LocalInterface reverse)
        {
            IsHost = isHost;
            Reverse = reverse;
        }

        public void SendBytes(byte[] bytes)
        {
            BytesSent += bytes.Length;
            Reverse.OnRecieveBytes?.Invoke(bytes, 0, bytes.Length);
        }
    }
}
