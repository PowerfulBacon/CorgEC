using Assets.Code.Networking.Communication.NetworkLayer;
using Assets.Code.Networking.Communication.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Networking.Serialisation
{
    public static class SerialisationHelper
    {

        private const uint MOST_SIGNIFICANT_BIT = 0b10000000000000000000000000000000;

        //TODO Handle deserialisation of important types like lists/arrays/dictionaries.
        public static object Deserialise(INetworkInterface sender, INetworkObjectTracker localObjects, Type type, BinaryReader reader)
        {
			// Optimisation: Check the name, so that we can skip straight through primitive types
#if NET6_0_OR_GREATER
            object? result = type.Name switch
            {
                "Boolean" => type == typeof(bool) ? reader.ReadBoolean() : null,
                "Byte" => type == typeof(byte) ? reader.ReadByte() : null,
                "SByte" => type == typeof(sbyte) ? reader.ReadSByte() : null,
                "Int16" => type == typeof(short) ? reader.ReadInt16() : null,
                "UInt16" => type == typeof(ushort) ? reader.ReadUInt16() : null,
                "Int32" => type == typeof(int) ? reader.ReadInt32() : null,
                "UInt32" => type == typeof(uint) ? reader.ReadUInt32() : null,
                "Int64" => type == typeof(long) ? reader.ReadInt64() : null,
                "UInt64" => type == typeof(ulong) ? reader.ReadUInt64() : null,
                "Char" => type == typeof(char) ? reader.ReadChar() : null,
                "Single" => type == typeof(float) ? reader.ReadSingle() : null,
                "Double" => type == typeof(double) ? reader.ReadDouble() : null,
                "String" => type == typeof(string) ? reader.ReadString() : null,
                _ => null
        };
#else
			object result = null;
            switch (type.Name)
            {
                case "Boolean":
                    result = type == typeof(bool) ? (bool?)reader.ReadBoolean() : null;
					break;
				case "Byte":
                    result = type == typeof(byte) ? (byte?)reader.ReadByte() : null;
					break;
				case "SByte":
					result = type == typeof(sbyte) ? (sbyte?)reader.ReadSByte() : null;
					break;
				case "Int16":
					result = type == typeof(short) ? (short?)reader.ReadInt16() : null;
					break;
				case "UInt16":
					result = type == typeof(ushort) ? (ushort?)reader.ReadUInt16() : null;
					break;
				case "Int32":
					result = type == typeof(int) ? (int?)reader.ReadInt32() : null;
					break;
				case "UInt32":
					result = type == typeof(uint) ? (uint?)reader.ReadUInt32() : null;
					break;
				case "Int64":
					result = type == typeof(long) ? (long?)reader.ReadInt64() : null;
					break;
				case "UInt64":
					result = type == typeof(ulong) ? (ulong?)reader.ReadUInt64() : null;
					break;
				case "Char":
					result = type == typeof(char) ? (char?)reader.ReadChar() : null;
					break;
				case "Single":
					result = type == typeof(float) ? (float?)reader.ReadSingle() : null;
					break;
				case "Double":
					result = type == typeof(double) ? (double?)reader.ReadDouble() : null;
					break;
				case "String":
					result = type == typeof(string) ? reader.ReadString() : null;
					break;
			}
#endif
            if (typeof(INetworkedSerialised).IsAssignableFrom(type))
            {
                // Read the uint ID.
                // 0 = null
                // Most significant bit indicates if we have more data to read
                uint identifier = reader.ReadUInt32();
                if (identifier == 0)
                {
                    // We have been sent null
                    return null;
                }
                else if ((identifier & MOST_SIGNIFICANT_BIT) != 0)
                {
                    // We have been given the entire entity to read
                    // Read the type to work out how we can process it
                    uint typeIdentifier = reader.ReadUInt32();
                    Type deserialisedType = PacketTypeList.SerialisableTypes[typeIdentifier];
                    // Create an uninitialised instance of that type
                    INetworkedSerialised createdEntity = (INetworkedSerialised)FormatterServices.GetUninitializedObject(deserialisedType);
                    createdEntity.NetworkID = identifier & ~MOST_SIGNIFICANT_BIT;
                    // Deserialise
                    createdEntity.Deserialise(sender, localObjects, reader);
                    if (!localObjects._AccessibleObjects.ContainsKey(identifier & ~MOST_SIGNIFICANT_BIT))
                        localObjects._AccessibleObjects.Add(identifier & ~MOST_SIGNIFICANT_BIT, createdEntity);
                    //  Return the object
                    return createdEntity;
                }
                else
                {
                    // We have no been given the entire entity to read
                    // Find the entity from our cache and return that object
                    return localObjects.KnownObjects[identifier];
                }
            }
            if (result != null)
                return result;
            // Handle any more complex types
            throw new NotImplementedException($"Could not deserialise the type {type.Name}.");
        }

        public static void Serialise(INetworkInterface target, Type type, object value, BinaryWriter writer)
        {
            // Optimisation: Check the name, so that we can skip straight through primitive types
            switch (type.Name)
            {
                case "Boolean":
                    if (type != typeof(bool) || value == null)
                        throw new NotImplementedException($"Could not serialise the type {type.Name}.");
                    writer.Write((bool)value);
                    return;
                case "Byte":
                    if (type != typeof(byte) || value == null)
                        throw new NotImplementedException($"Could not serialise the type {type.Name}.");
                    writer.Write((byte)value);
                    return;
                case "SByte":
                    if (type != typeof(sbyte) || value == null)
                        throw new NotImplementedException($"Could not serialise the type {type.Name}.");
                    writer.Write((sbyte)value);
                    return;
                case "Int16":
                    if (type != typeof(short) || value == null)
                        throw new NotImplementedException($"Could not serialise the type {type.Name}.");
                    writer.Write((short)value);
                    return;
                case "UInt16":
                    if (type != typeof(ushort) || value == null)
                        throw new NotImplementedException($"Could not serialise the type {type.Name}.");
                    writer.Write((ushort)value);
                    return;
                case "Int32":
                    if (type != typeof(int) || value == null)
                        throw new NotImplementedException($"Could not serialise the type {type.Name}.");
                    writer.Write((int)value);
                    return;
                case "UInt32":
                    if (type != typeof(uint) || value == null)
                        throw new NotImplementedException($"Could not serialise the type {type.Name}.");
                    writer.Write((uint)value);
                    return;
                case "Int64":
                    if (type != typeof(long) || value == null)
                        throw new NotImplementedException($"Could not serialise the type {type.Name}.");
                    writer.Write((long)value);
                    return;
                case "UInt64":
                    if (type != typeof(ulong) || value == null)
                        throw new NotImplementedException($"Could not serialise the type {type.Name}.");
                    writer.Write((ulong)value);
                    return;
                case "Char":
                    if (type != typeof(char) || value == null)
                        throw new NotImplementedException($"Could not serialise the type {type.Name}.");
                    writer.Write((char)value);
                    return;
                case "Single":
                    if (type != typeof(float) || value == null)
                        throw new NotImplementedException($"Could not serialise the type {type.Name}.");
                    writer.Write((float)value);
                    return;
                case "Double":
                    if (type != typeof(double) || value == null)
                        throw new NotImplementedException($"Could not serialise the type {type.Name}.");
                    writer.Write((double)value);
                    return;
                case "String":
                    if (type != typeof(string) || value == null)
                        throw new NotImplementedException($"Could not serialise the type {type.Name}.");
                    writer.Write((string)value);
                    return;
            };
            if (typeof(INetworkedSerialised).IsAssignableFrom(type))
            {
                // If the object is null, just send 0
                if (value == null)
                {
                    writer.Write((uint)0);
                    return;
                }
                INetworkedSerialised networkedSerialised = (INetworkedSerialised)value;
                // If the object has no network ID, assign one
                if (networkedSerialised.NetworkID == 0)
                {
					networkedSerialised.NetworkID = NetworkSerialisedID.CurrentNetworkID++;
                }
                // If we need to serialise the object, send the whole thing
                if (!target.KnownObjects.ContainsKey(networkedSerialised.NetworkID))
                {
                    // Indicate that we need to send the entire object
                    writer.Write(networkedSerialised.NetworkID | MOST_SIGNIFICANT_BIT);
                    // Send the type as an identifier (we need to know how to deserialise the object on the other end)
                    writer.Write(PacketTypeList.SerialisableTyeIDs[networkedSerialised.GetType()]);
                    // Send the entire object
                    networkedSerialised.Serialise(target, writer);
                    // Now that they know about the object, mark that in
                    target._AccessibleObjects.Add(networkedSerialised.NetworkID, networkedSerialised);
                    return;
                }
                else
                {
                    // Just send the ID, we know about this object
                    writer.Write(networkedSerialised.NetworkID & ~MOST_SIGNIFICANT_BIT);
                    return;
                }
            }
            // Handle any more complex types
            throw new NotImplementedException($"Could not deserialise the type {type.Name}.");
        }

		public static bool CanSerialise(Type type)
		{
            switch (type.Name)
            {
                case "Boolean":
                    return type == typeof(bool) ? true : typeof(INetworkedSerialised).IsAssignableFrom(type);
				case "Byte":
					return type == typeof(byte) ? true : typeof(INetworkedSerialised).IsAssignableFrom(type);
				case "SByte":
					return type == typeof(sbyte) ? true : typeof(INetworkedSerialised).IsAssignableFrom(type);
				case "Int16":
					return type == typeof(short) ? true : typeof(INetworkedSerialised).IsAssignableFrom(type);
				case "UInt16":
					return type == typeof(ushort) ? true : typeof(INetworkedSerialised).IsAssignableFrom(type);
				case "Int32":
					return type == typeof(int) ? true : typeof(INetworkedSerialised).IsAssignableFrom(type);
				case "UInt32":
					return type == typeof(uint) ? true : typeof(INetworkedSerialised).IsAssignableFrom(type);
				case "Int64":
					return type == typeof(long) ? true : typeof(INetworkedSerialised).IsAssignableFrom(type);
				case "UInt64":
					return type == typeof(ulong) ? true : typeof(INetworkedSerialised).IsAssignableFrom(type);
				case "Char":
					return type == typeof(char) ? true : typeof(INetworkedSerialised).IsAssignableFrom(type);
				case "Single":
					return type == typeof(float) ? true : typeof(INetworkedSerialised).IsAssignableFrom(type);
				case "Double":
					return type == typeof(double) ? true : typeof(INetworkedSerialised).IsAssignableFrom(type);
				case "String":
					return type == typeof(string) ? true : typeof(INetworkedSerialised).IsAssignableFrom(type);
			}
            return typeof(INetworkedSerialised).IsAssignableFrom(type);
		}
	}
}
