using Assets.Code.Networking.Communication.NetworkLayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECCore.Instances
{
    public class Instance
    {

        public static Instance DefaultInstance { get; } = new Instance();

#if NET6_0_OR_GREATER
        public INetworkInterface? NetworkInterface { get; }
#else
		public INetworkInterface NetworkInterface { get; }
#endif

        public Instance()
        {
            
        }

        public Instance(INetworkInterface networkInterface)
        {
            NetworkInterface = networkInterface;
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
            return true;
        }

    }
}
