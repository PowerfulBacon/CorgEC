using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Networking.Communication.Packets
{
    public enum PacketSecurityFlags
    {
        DEFAULT = ALLOW_HOST_RAISE | RAISED_ON_HOST | RAISED_ON_CLIENT,
        // Default and required
        ALLOW_HOST_RAISE = 1 << 0,
        // Allow clients to raise this packet
        ALLOW_CLIENT_RAISE = 1 << 1,
        // This packet can be raised on the host
        RAISED_ON_HOST = 1 << 2,
        // This packet can be raised on clients
        RAISED_ON_CLIENT = 1 << 3,
    }
}
