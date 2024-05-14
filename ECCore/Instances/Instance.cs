using Assets.Code.Networking.Communication.ApplicationLayer;
using Assets.Code.Networking.Communication.NetworkLayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECCore.Instances
{
    public class Instance
    {

        public static Instance InstanceFromNetwork(NetworkManager network) => instances[network];

        public static Instance DefaultInstance { get; } = new Instance();

        private static Dictionary<NetworkManager, Instance> instances = new Dictionary<NetworkManager, Instance>();

        /// <summary>
        /// The network interface that the instance can send messages on
        /// </summary>
        public NetworkManager NetworkManager { get; }

        public Instance()
        {
            NetworkManager = new NetworkManager();
        }

        public Instance(NetworkManager networkManager)
        {
            NetworkManager = networkManager;
            instances.Add(networkManager, this);
        }

        ~Instance()
        {
            if (NetworkManager != null)
                instances.Remove(NetworkManager);
        }

        /// <summary>
        /// Create a new entity, adding any components that we need to add to it in the
        /// passed initialisation function.
        /// </summary>
        /// <param name="entityCreation"></param>
        /// <returns></returns>
        public Entity Create(Action<Entity> entityCreation)
        {
            return Entity.Create(this, entityCreation);
        }

        /// <summary>
        /// Are we the host instance?
        /// </summary>
        /// <returns></returns>
        public bool IsHostInstance()
        {
            return NetworkManager.IsHost;
        }

        /// <summary>
        /// Are we considered a client instance?
        /// </summary>
        /// <returns></returns>
        public bool IsClientInstance()
        {
            return !NetworkManager.IsHost;
        }

    }
}
