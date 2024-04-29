using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Networking.Communication.Packets
{
    public static class PacketCache<TPacketType>
            where TPacketType : Packet<TPacketType>
    {

        public static IEnumerable<PropertyInfo> SerialisedProperties = typeof(TPacketType)
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => (!x.DeclaringType.IsConstructedGenericType || x.DeclaringType.GetGenericTypeDefinition() != typeof(Packet<>)) && x.Name != "SecurityFlags");

        public static IEnumerable<FieldInfo> SerialisedFields = typeof(TPacketType)
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => !x.DeclaringType.IsConstructedGenericType || x.DeclaringType.GetGenericTypeDefinition() != typeof(Packet<>));
        
        private static ushort? pACKET_ID = null;

        public static ushort PACKET_ID { get {
                if (pACKET_ID == null)
                {
                    Console.Error.WriteLine($"Packet of type {typeof(TPacketType).Name} is not present in the packet cache, add its assembly.");
                    throw new Exception($"Packet of type {typeof(TPacketType).Name} is not present in the packet cache, add its assembly.");
                }
                return pACKET_ID.Value;
            } set => pACKET_ID = value; }

    }
}
