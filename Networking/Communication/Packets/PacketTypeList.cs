using Assets.Code.Networking.Serialisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Networking.Communication.Packets
{
    public static class PacketTypeList
    {
        private static readonly Dictionary<uint, Type> serialisableTypes = new Dictionary<uint, Type>();
        private static readonly Dictionary<Type, uint> serialisableTypeIDs = new Dictionary<Type, uint>();

        public static IPacket[] PacketArray { get; private set; }

        public static IReadOnlyDictionary<uint, Type> SerialisableTypes => serialisableTypes;
        public static IReadOnlyDictionary<Type, uint> SerialisableTyeIDs => serialisableTypeIDs;

        public static void Generate(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                Console.WriteLine($"Loading assembly {assembly.GetName().Name}");
            }
            // Load the types and assign them IDs
            uint identifier = 0;
            foreach (var serialisableType in assemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => {
                    return typeof(INetworkedSerialised).IsAssignableFrom(x) && !x.IsAbstract;
                }))
            {
                identifier++;
                serialisableTypes.Add(identifier, serialisableType);
                serialisableTypeIDs.Add(serialisableType, identifier);
            }
            Console.WriteLine($"Compiled {identifier} serialisable types.");
            // Load the packets and assign them IDs
            PacketArray = assemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => x.BaseType != null && x.BaseType.IsConstructedGenericType && x.BaseType.GetGenericTypeDefinition() == typeof(Packet<>) && !x.IsAbstract)
                .Select(x => (IPacket)FormatterServices.GetUninitializedObject(x))
                .ToArray();
            // Verification
            for (ushort i = 0; i < PacketArray.Length; i++)
            {
                IPacket packet = PacketArray[i];
                packet.Compile(i);
            }
            Console.WriteLine($"Compiled {PacketArray.Length} packets.");
        }

    }
}
