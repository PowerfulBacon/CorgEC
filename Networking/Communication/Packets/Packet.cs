using Assets.Code.Networking.Communication.ApplicationLayer;
using Assets.Code.Networking.Communication.NetworkLayer;
using Assets.Code.Networking.Serialisation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Assets.Code.Networking.Communication.Packets
{
    public interface IPacket
    {
        PacketSecurityFlags SecurityFlags { get; }
        void Deserialise(INetworkInterface sender, NetworkManager localNetworkManager, BinaryReader reader, bool ignore);
        void SendToOthers(NetworkManager networkManager);
        void Compile(ushort id);
    }

    public abstract class Packet<TThis> : IPacket
        where TThis : Packet<TThis>
    {

        /// <summary>
        /// How much network delay are we simulating?
        /// </summary>
        public static int? simulatedDelay = 200;

        /// <summary>
        /// Security flags set on this packet.
        /// </summary>
        public virtual PacketSecurityFlags SecurityFlags { get; } = PacketSecurityFlags.DEFAULT;
 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">We need to know who the target is so that we know if we need to serialise the entity.</param>
        /// <returns></returns>
        private byte[] Serialise(INetworkInterface target)
        {
            lock (this)
            {
                using MemoryStream memoryStream = new MemoryStream();
                using BinaryWriter writer = new BinaryWriter(memoryStream);
                // we need to insert our ID
                writer.Write(PacketCache<TThis>.PACKET_ID);
                foreach (var property in PacketCache<TThis>.SerialisedProperties)
                {
                    SerialisationHelper.Serialise(target, property.PropertyType, property.GetValue(this), writer);
                }
                foreach (var field in PacketCache<TThis>.SerialisedFields)
                {
                    SerialisationHelper.Serialise(target, field.FieldType, field.GetValue(this), writer);
                }
                return memoryStream.ToArray();
            }
        }

        public void Deserialise(INetworkInterface sender, NetworkManager localNetworkManager, BinaryReader reader, bool ignore)
        {
            Packet<TThis> packet = (TThis)FormatterServices.GetUninitializedObject(typeof(TThis));
            foreach (var property in PacketCache<TThis>.SerialisedProperties)
            {
                var value = SerialisationHelper.Deserialise(sender, localNetworkManager, property.PropertyType, reader);
                //Console.WriteLine($"{property.Name}: {value}");
                property.SetValue(packet, value);
            }
            foreach (var field in PacketCache<TThis>.SerialisedFields)
            {
                var value = SerialisationHelper.Deserialise(sender, localNetworkManager, field.FieldType, reader);
                //Console.WriteLine($"{field.Name}: {value}");
                field.SetValue(packet, value);
            }
            // Nothing more to do here
            if (ignore)
                return;
            // Alright, let's handle this
            // TODO Handle getting the transmission time from the server and passing it here for prediction
            if (simulatedDelay == null)
            {
                Task.Run(() => packet.Recieve(localNetworkManager, sender, (simulatedDelay ?? 0) / 1000.0));
            }
            else
            {
                Task.Delay(simulatedDelay.Value).ContinueWith(_ => packet.Recieve(localNetworkManager, sender, (simulatedDelay ?? 0) / 1000.0), TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        /// <summary>
        /// Called when the packet is recieved.
        /// </summary>
        /// <param name="sendTime"></param>
        protected abstract void Recieve(NetworkManager localNetworkManager, INetworkInterface sender, double sendTime);

        /// <summary>
        /// Send this packet to a specific client.
        /// </summary>
        /// <param name="networkInterface"></param>
        public void SendTo(NetworkManager networkManager, INetworkInterface networkInterface)
        {
            if ((SecurityFlags & PacketSecurityFlags.ALLOW_CLIENT_RAISE) == 0 && !networkManager.IsHost)
            {
                throw new Exception($"Attempting to raise packet {GetType().Name} as a client which is not allowed per the packet's security flags.");
            }
            if ((SecurityFlags & PacketSecurityFlags.ALLOW_HOST_RAISE) == 0 && networkManager.IsHost)
            {
                throw new Exception($"Attempting to raise packet {GetType().Name} as the host which is not allowed per the packet's security flags.");
            }
            if ((SecurityFlags & PacketSecurityFlags.RAISED_ON_HOST) == 0 && !networkInterface.IsHost)
            {
                throw new Exception($"Attempting to raise packet {GetType().Name} on the host's instance which is not permitted.");
            }
            if ((SecurityFlags & PacketSecurityFlags.RAISED_ON_CLIENT) == 0 && networkInterface.IsHost)
            {
                throw new Exception($"Attempting to raise packet {GetType().Name} on a client's instance which is not permitted.");
            }
            // Cheat and bypass the network interface system to prevent duplicating objects that the server doesn't know it knows about.
            if (networkInterface == networkManager.LoopbackInterface)
            {
                Recieve(networkManager, networkInterface, 0);
            }
            else
            {
                networkInterface.SendBytes(Serialise(networkInterface));
            }
        }

        /// <summary>
        /// Send this packet to everyone, including ourselves.
        /// </summary>
        /// <param name="networkManager"></param>
        public void SendToAll(NetworkManager networkManager)
        {
            if ((SecurityFlags & PacketSecurityFlags.ALLOW_CLIENT_RAISE) == 0 && !networkManager.IsHost)
            {
                throw new Exception($"Attempting to raise packet {GetType().Name} as a client which is not allowed per the packet's security flags.");
            }
            if ((SecurityFlags & PacketSecurityFlags.ALLOW_HOST_RAISE) == 0 && networkManager.IsHost)
            {
                throw new Exception($"Attempting to raise packet {GetType().Name} as the host which is not allowed per the packet's security flags.");
            }
            if ((SecurityFlags & PacketSecurityFlags.RAISED_ON_HOST) == 0)
            {
                throw new Exception($"Attempting to raise packet {GetType().Name} on the host's instance which is not permitted.");
            }
            if ((SecurityFlags & PacketSecurityFlags.RAISED_ON_CLIENT) == 0)
            {
                throw new Exception($"Attempting to raise packet {GetType().Name} on a client's instance which is not permitted.");
            }
            foreach (var connection in networkManager.Connections)
            {
                connection.SendBytes(Serialise(connection));
            }
            Recieve(networkManager, networkManager.LoopbackInterface, 0);
        }

        /// <summary>
        /// Send this packet to everyone except ourselves.
        /// </summary>
        public void SendToOthers(NetworkManager networkManager)
        {
            if ((SecurityFlags & PacketSecurityFlags.ALLOW_CLIENT_RAISE) == 0 && !networkManager.IsHost)
            {
                throw new Exception($"Attempting to raise packet {GetType().Name} as a client which is not allowed per the packet's security flags.");
            }
            if ((SecurityFlags & PacketSecurityFlags.ALLOW_HOST_RAISE) == 0 && networkManager.IsHost)
            {
                throw new Exception($"Attempting to raise packet {GetType().Name} as the host which is not allowed per the packet's security flags.");
            }
            if ((SecurityFlags & PacketSecurityFlags.RAISED_ON_CLIENT) == 0)
            {
                throw new Exception($"Attempting to raise packet {GetType().Name} on the client's instance which is not permitted.");
            }
            foreach (var connection in networkManager.Connections)
            {
                connection.SendBytes(Serialise(connection));
            }
        }

        /// <summary>
        /// Send this packet to the host, or handle it locally if we are the host.
        /// </summary>
        /// <param name="networkManager"></param>
        public void SendToHost(NetworkManager networkManager)
        {
            if ((SecurityFlags & PacketSecurityFlags.ALLOW_CLIENT_RAISE) == 0 && !networkManager.IsHost)
            {
                throw new Exception($"Attempting to raise packet {GetType().Name} as a client which is not allowed per the packet's security flags.");
            }
            if ((SecurityFlags & PacketSecurityFlags.ALLOW_HOST_RAISE) == 0 && networkManager.IsHost)
            {
                throw new Exception($"Attempting to raise packet {GetType().Name} as the host which is not allowed per the packet's security flags.");
            }
            if ((SecurityFlags & PacketSecurityFlags.RAISED_ON_HOST) == 0)
            {
                throw new Exception($"Attempting to raise packet {GetType().Name} on the host's instance which is not permitted.");
            }
            // We are the host
            if (networkManager.IsHost)
            {
                Recieve(networkManager, networkManager.LoopbackInterface, 0);
            }
            else
            {
                SendToOthers(networkManager);
            }
        }

        /// <summary>
        /// Compile the packet and throw errors if there are things that we cannot currently handle.
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="AggregateException"></exception>
        public void Compile(ushort id)
        {
            PacketCache<TThis>.PACKET_ID = id;
            List<Exception> exceptionList = new List<Exception>();
            // Ensure that our variables are types that we can serialise
            foreach (var property in PacketCache<TThis>.SerialisedProperties)
            {
                if (!SerialisationHelper.CanSerialise(property.PropertyType))
                {
                    exceptionList.Add(new Exception($"Type {GetType()} has a property of type {property.PropertyType} which cannot be serialised. Please change the type to a serialisable entity or remove the variable."));
                }
            }
            foreach (var field in PacketCache<TThis>.SerialisedFields)
            {
                if (!SerialisationHelper.CanSerialise(field.FieldType))
                {
                    exceptionList.Add(new Exception($"Type {GetType()} has a field of type {field.FieldType} which cannot be serialised. Please change the type to a serialisable entity or remove the variable."));
                }
            }
            if (exceptionList.Count == 1)
            {
                throw exceptionList[0];
            }
            if (exceptionList.Count > 0)
            {
                throw new AggregateException(exceptionList);
            }
        }
    }
}
