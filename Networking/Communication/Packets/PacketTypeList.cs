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

        private static HashSet<string> loadedAssemblies = new HashSet<string>();

        private static uint identifier = 0;

        private static ushort packetId = 0;

        public static void Generate(params Assembly[] assemblies)
        {
            if (loadedAssemblies.Count > 0)
            {
                Console.WriteLine($"{loadedAssemblies.Count} assemblise are already loaded: {string.Join(", ", loadedAssemblies)}");
            }
            foreach (var assembly in assemblies)
            {
                Console.WriteLine($"Loading assembly {assembly.GetName().Name}");
            }
            // Load the types and assign them IDs
            foreach (var serialisableType in assemblies
                .Where(asm => !loadedAssemblies.Contains(asm.Location))
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
            var tempArray = assemblies
                .Where(asm => !loadedAssemblies.Contains(asm.Location))
                .SelectMany(x => x.GetTypes())
                .Where(x => x.BaseType != null && x.BaseType.IsConstructedGenericType && x.BaseType.GetGenericTypeDefinition() == typeof(Packet<>) && !x.IsAbstract)
                .Select(x => (IPacket)FormatterServices.GetUninitializedObject(x))
                .Select(packet =>
                {
                    packet.Compile(packetId);
                    return packet;
                });
            if (PacketArray != null)
            {
                tempArray = tempArray.Union(PacketArray);
            }
            PacketArray = tempArray.ToArray();
            foreach (var loadedAssembly in assemblies)
            {
                loadedAssemblies.Add(loadedAssembly.Location);
			}
			Console.WriteLine($"Compiled {PacketArray.Length} packets.");
        }

    }
}
