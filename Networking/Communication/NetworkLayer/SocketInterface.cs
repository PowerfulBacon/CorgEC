//#define DEBUG_PACKETS

using Assets.Code.Networking.Serialisation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Networking.Communication.NetworkLayer
{
    internal class SocketInterface : INetworkInterface, IDisposable
    {

        public const int DEFAULT_TICKRATE = 20;

        public bool IsHost { get; }

        /// <summary>
        /// The base tick rate of the server, for continuous packet updates that can't be sent the moment
        /// they are processed.
        /// This will handle things like movement which is interpolated, while single events such as shooting
        /// will by handled immediately.
        /// </summary>
        public int TickRate { get; set; }

        public Action<byte[], int, int> OnRecieveBytes { set; get; }
        public Action OnDisconnect { set; get; }

        private Dictionary<uint, INetworkedSerialised> _knownObjects = new Dictionary<uint, INetworkedSerialised>();

        public IReadOnlyDictionary<uint, INetworkedSerialised> KnownObjects => _knownObjects;

        IDictionary<uint, INetworkedSerialised> INetworkObjectTracker.AccessibleObjects => _knownObjects;

        public int BytesSent { get; private set; }

        public int BytesRecieved { get; private set; }

        private Socket _socket;

        /// <summary>
        /// Constructor will automatically start the task that handles reading the socket
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="isHost"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public SocketInterface(Socket socket, bool isHost, IPAddress address, int port, int tickRate = DEFAULT_TICKRATE)
        {
            IsHost = isHost;
            _socket = socket;
            _ = StartReadingTask();
            TickRate = tickRate;
        }

        private Task StartReadingTask()
        {
            List<byte> previousBytes = new List<byte>(1024);
            int amountToRead = 0;
            // Start our socket reading task
            return Task.Run(async () =>
            {
                while (_socket.Connected)
                {
                    var buffer = new byte[1024];
#if NET6_0_OR_GREATER
                    var received = await _socket.ReceiveAsync(buffer, SocketFlags.None);
#else
					var received = _socket.Receive(buffer, SocketFlags.None);
#endif
					BytesRecieved += received;
                    // Pointer is the element that we should read first
                    int pointer = 0;
#if DEBUG_PACKETS
                    Console.WriteLine($"Processing {received} bytes...");
#endif
                    while (pointer < received)
                    {
#if DEBUG_PACKETS
                        Console.WriteLine($"Reading from pointer {pointer}/{received-1}");
#endif
                        // Starting a read.
                        if (amountToRead <= 0)
                        {
                            // The first set of bytes is the amount we need to read
                            amountToRead = BitConverter.ToInt32(buffer, pointer);
                            pointer += sizeof(int);
#if DEBUG_PACKETS
                            Console.WriteLine($"Looking for {amountToRead} bytes (bytes {pointer} to {pointer + amountToRead - 1}).");
#endif
                            // If we can read this all in 1 go, then great
                            if (amountToRead <= received - pointer)
                            {
                                // No need to deal with buffer overflow
#if DEBUG_PACKETS
                                Console.WriteLine($"(A) Read message of size {amountToRead}");
#endif
                                OnRecieveBytes?.Invoke(buffer, pointer, amountToRead);
                                pointer += amountToRead;
                                // Mark that we finished reading
                                amountToRead = 0;
                            }
                            // If we cannot read all of this in 1 go, add it to the buffer
                            else
                            {
                                // Calculate how much left we have to read so that we can read all of that
                                int readAmount = received - pointer;
#if DEBUG_PACKETS
                                Console.WriteLine($"(B) Recieved a message of size {readAmount} but we want {amountToRead} bytes, waiting for next packet...");
#endif
                                // Ignore the length message
                                previousBytes.AddRange(buffer.Skip(pointer).Take(readAmount));
                                amountToRead -= readAmount;
                                // This will take our pointer to the end
                                pointer += readAmount;
                            }
                        }
                        // Continuing a read
                        else
                        {
                            // If we can finish the read
                            if (amountToRead < received - pointer)
                            {
                                // Read what we need to
                                previousBytes.AddRange(buffer.Skip(pointer).Take(amountToRead));
                                // Increment the reading pointer
                                pointer += amountToRead;
                                // Read that stuff
#if DEBUG_PACKETS
                                Console.WriteLine($"(C) Read message of size {previousBytes.Count}");
#endif
                                OnRecieveBytes?.Invoke(previousBytes.ToArray(), 0, previousBytes.Count);
                                previousBytes.Clear();
                                // Mark that we finished reading.
                                amountToRead = 0;
                            }
                            // We can't read all of it in 1 go
                            else
                            {
                                // Calculate how much left we have to read so that we can read all of that
                                int readAmount = received - pointer;
#if DEBUG_PACKETS
                                Console.WriteLine($"(D) Recieved a message of size {readAmount} but we want {amountToRead} bytes, waiting for next packet...");
#endif
                                // Ignore the length message
                                previousBytes.AddRange(buffer.Skip(pointer).Take(readAmount));
                                amountToRead -= readAmount;
                                // This will take our pointer to the end
                                pointer += readAmount;
                            }
                        }
                    }
                }
            });
        }

        public void SendBytes(byte[] bytes)
        {
            byte[] message = new byte[bytes.Length + sizeof(int)];
            BytesSent += message.Length;
            bytes.CopyTo(message, sizeof(int));
            BitConverter.GetBytes((int)bytes.Length).CopyTo(message, 0);
            _socket.Send(message);
        }

        public void Dispose()
        {
            _socket?.Dispose();
        }
    }
}
       