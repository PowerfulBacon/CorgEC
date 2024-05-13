using Assets.Code.Networking.Communication.NetworkLayer;
using Assets.Code.Networking.Serialisation;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Networking.Communication.NetworkLayer
{
    public class LocalInterface : INetworkInterface, IDisposable
    {

        public bool IsHost { get; }

        public Action<byte[], int, int> OnRecieveBytes { set; get; }
        public Action OnDisconnect { set; get; }

        private Dictionary<uint, INetworkedSerialised> _knownObjects = new Dictionary<uint, INetworkedSerialised>();

        public IReadOnlyDictionary<uint, INetworkedSerialised> KnownObjects => _knownObjects;

        IDictionary<uint, INetworkedSerialised> INetworkObjectTracker._AccessibleObjects => _knownObjects;

        public int BytesSent { get; private set; }

        public int BytesRecieved { get; private set; }

        private TaskCompletionSource<byte[]> recieveQueue = new TaskCompletionSource<byte[]>();

        private bool sending = true;

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
            _ = StartReadingTask();
        }

        private Task StartReadingTask()
        {
            List<byte> previousBytes = new List<byte>(1024);
            int amountToRead = 0;
            // Start our socket reading task
            return Task.Run(async () =>
            {
                while (sending)
                {
                    var buffer = await recieveQueue.Task;
                    int received = buffer.Length;
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
            recieveQueue.SetResult(message);
            recieveQueue = new TaskCompletionSource<byte[]>();
        }

        public void Dispose()
        {
            sending = false;
        }
    }
}
