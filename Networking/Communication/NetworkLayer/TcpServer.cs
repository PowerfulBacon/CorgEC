using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Networking.Communication.NetworkLayer
{
    public class TcpServer : INetworkHost
    {

        public Action<INetworkInterface> OnConnectionRecieved { set; private get; }
        
        public Action OnClosed { set; private get; }

        public event Action<INetworkInterface> connectionRecievedHooks;

        public async Task Start(int port)
        {
            var endPoint = new IPEndPoint(IPAddress.Any, port);
            TcpListener listener = new TcpListener(endPoint);
            try
            {
                listener.Start();
                Console.WriteLine($"Listener started on port {port}");
                while (true)
                {
                    try
                    {
                        // Accept a socket
                        Socket client = await listener.AcceptSocketAsync();
                        Console.WriteLine($"Connection accepted from {(client.RemoteEndPoint as IPEndPoint)?.Address}:{(client.RemoteEndPoint as IPEndPoint)?.Port}");
                        // Create a socket interface
                        var clientInterface = new SocketInterface(
                            client,
                            false,
                            (client.RemoteEndPoint as IPEndPoint)?.Address ?? throw new NullReferenceException("Server exception: Client remote end point was null or not an IPEndPoint"),
                            (client.RemoteEndPoint as IPEndPoint)?.Port ?? throw new NullReferenceException("Server exception: Client remote end point was null or not an IPEndPoint")
                        );
                        // Handle the connection
                        OnConnectionRecieved?.Invoke(clientInterface);
                        connectionRecievedHooks?.Invoke(clientInterface);
                    }
                    // Accept null references, explicitly don't accept socket exceptions as they mean we actually failed
                    catch (NullReferenceException ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            finally
            {
                listener.Stop();
                OnClosed?.Invoke();
            }
        }

    }
}
