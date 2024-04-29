using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Networking.Communication.NetworkLayer
{
    /// <summary>
    /// Not using events because we should only join them once and need easy unregistering.
    /// </summary>
    public interface INetworkHost
    {
        Action<INetworkInterface> OnConnectionRecieved { set; }
        Action OnClosed { set; }
    }
}
