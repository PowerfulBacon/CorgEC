using Assets.Code.Networking.Communication.NetworkLayer;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Networking.Communication.NetworkLayer
{
    public class LocalServer : INetworkHost
    {

        public Action<INetworkInterface> OnConnectionRecieved { set; private get; }

        public Action OnClosed { set; private get; }

        public event Action<INetworkInterface> connectionRecievedHooks;

        public LocalInterface Connect()
        {
            // Create a socket interface that lets us talk to the client
            LocalInterface clientInterface = new LocalInterface(false);
            // Handle the connection
            OnConnectionRecieved?.Invoke(clientInterface);
            connectionRecievedHooks?.Invoke(clientInterface);
            // Return the thing that the client uses to talk to the server
            return clientInterface.Reverse;
        }

    }
}
