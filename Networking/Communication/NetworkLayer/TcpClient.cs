using Assets.Code.Networking.Communication.NetworkLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Networking.Communication.NetworkLayer
{
    public class TcpClient
    {

        public async Task<INetworkInterface> Start(IPAddress address, int port)
        {
            Socket client = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(address, port);
            Console.WriteLine($"Connected to {address}:{port}");
            return new SocketInterface(client, true, address, port);
        }

    }
}
