using Assets.Code.Networking.Communication.NetworkLayer;
using Assets.Code.Networking.Communication.Packets;
using Assets.Code.Networking.Serialisation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Networking.Communication.ApplicationLayer
{
    public class NetworkManager : INetworkObjectTracker
    {

        public bool IsHost { get; private set; } = false;

        private List<INetworkInterface> _connections = new List<INetworkInterface>();
        public IReadOnlyList<INetworkInterface> Connections => _connections;

        private Dictionary<uint, INetworkedSerialised> _knownObjects = new Dictionary<uint, INetworkedSerialised>();

        public IReadOnlyDictionary<uint, INetworkedSerialised> KnownObjects => _knownObjects;

        IDictionary<uint, INetworkedSerialised> INetworkObjectTracker.AccessibleObjects => _knownObjects;

        internal IDictionary<uint, INetworkedSerialised> internalObjectAccess => _knownObjects;

        private TaskCompletionSource<bool> message = new TaskCompletionSource<bool>();

        public LocalNetworkInterface LoopbackInterface { get; }

        public NetworkManager(INetworkHost networkHost)
        {
            IsHost = true;
            LoopbackInterface = new LocalNetworkInterface(this);
            ConnectClient(LoopbackInterface, true);
            networkHost.OnConnectionRecieved = (client) => ConnectClient(client, false);
            networkHost.OnClosed = () =>
            {
                networkHost.OnConnectionRecieved = null;
                networkHost.OnClosed = null;
            };
        }

        public NetworkManager(INetworkInterface serverInterface)
        {
            LoopbackInterface = new LocalNetworkInterface(this);
            ConnectClient(LoopbackInterface, true);
            ConnectClient(serverInterface, false);
        }

        private void ConnectClient(INetworkInterface client, bool localOnly)
        {
            if (!localOnly)
                _connections.Add(client);
            client.OnRecieveBytes = (bytes, start, length) => {
                using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes, start, length)))
                {
                    IPacket lastPacketProcessed;
                    IPacket currentPacket = null;
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        try
                        {
                            currentPacket = null;
                            // Read the packet type
                            ushort packetType = reader.ReadUInt16();
                            if (packetType < 0 || packetType >= PacketTypeList.PacketArray.Length)
                            {
                                Console.Error.WriteLine($"Could not locate packet with identifier {packetType}. Dropping {length - reader.BaseStream.Position} bytes.");
                                return;
                            }
                            IPacket packet = PacketTypeList.PacketArray[packetType];
                            currentPacket = packet;
                            // Security Check: Ensure that host ownership is enforced.
                            if (!client.IsHost && (packet.SecurityFlags & PacketSecurityFlags.ALLOW_CLIENT_RAISE) != PacketSecurityFlags.ALLOW_CLIENT_RAISE)
                            {
                                // Deserialise the packet
                                packet.Deserialise(client, this, reader, true);
                            }
                            else
                            {
                                // Deserialise the packet
                                packet.Deserialise(client, this, reader, false);
                            }
                            lastPacketProcessed = packet;
                        }
                        catch (Exception e)
                        {
                            // Critical exception
                            Console.Error.WriteLine($"Failed to parse packet type {currentPacket?.GetType().Name ?? "N/A"}\n{string.Join(",", bytes.Skip(start).Take(length))}\n{e}");
                            return;
                        }
                    }
                }
                lock (message)
                {
                    message.SetResult(true);
                    message = new TaskCompletionSource<bool>();
                }
            };
            // Unregister the handlers
            client.OnDisconnect = () => {
                if (!localOnly)
                    _connections.Remove(client);
                client.OnRecieveBytes = null;
                client.OnDisconnect = null;
            };
        }

        public async Task WaitForNextMessage()
        {
            Task target;
            lock (message)
            {
                target = message.Task;
            }
            await target;
        }

    }

    public class LocalNetworkInterface : INetworkInterface
    {
        public bool IsHost => _manager.IsHost;

        public Action<byte[], int, int> OnRecieveBytes { get; set; }
        public Action OnDisconnect { get; set; }

        public IReadOnlyDictionary<uint, INetworkedSerialised> KnownObjects => _manager.KnownObjects;

        IDictionary<uint, INetworkedSerialised> INetworkObjectTracker.AccessibleObjects => _manager.internalObjectAccess;

        public int BytesSent { get; private set; }

        public int BytesRecieved { get; private set; }

        private NetworkManager _manager;

        public LocalNetworkInterface(NetworkManager manager)
        {
            this._manager = manager;
        }

        public void SendBytes(byte[] bytes)
        {
            BytesSent += bytes.Length;
            OnRecieveBytes?.Invoke(bytes, 0, bytes.Length);
        }
    }
}
