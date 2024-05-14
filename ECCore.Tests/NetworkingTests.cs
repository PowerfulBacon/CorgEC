using Assets.Code.Networking.Communication.ApplicationLayer;
using Assets.Code.Networking.Communication.Packets;
using ECCore.Components;
using ECCore.Instances;
using Networking.Communication.NetworkLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECCore.Tests
{
    [TestClass]
    public class NetworkingTests
    {

        [TestInitialize]
        public void Setup()
        {
            PacketTypeList.Generate(typeof(InitialisingComponent).Assembly, typeof(Entity).Assembly, typeof(NetworkManager).Assembly);
            NetworkManager.simulatedDelay = null;
        }

        [TestMethod]
        public void TestComponentSerialisation()
        {
            // Setup
            LocalServer localHost = new LocalServer();
            NetworkManager server = new NetworkManager(localHost);
            NetworkManager client = new NetworkManager(localHost.Connect());
            Instance serverInstance = new Instance(server);
            Instance clientInstance = new Instance(client);
            // Perform Functions
            Entity entity = serverInstance.Create(entity =>
            {
                entity.TryAddComponent(new DataStoreComponent("hello", 55, true));
            });
            // Test Results
            var clientEntity = client.KnownObjects[entity.NetworkID] as Entity;
            Assert.IsNotNull(clientEntity, "Client entity should not be null and should have been communicated.");
            Assert.IsTrue(clientEntity.HasComponent<DataStoreComponent>());
            Assert.AreEqual("hello", clientEntity.GetComponent<DataStoreComponent>().message, "The data store component should have the message set.");
            Assert.AreEqual(55, clientEntity.GetComponent<DataStoreComponent>().number, "The data store component should have the number set.");
            Assert.IsTrue(clientEntity.GetComponent<DataStoreComponent>().boolean, "The data store component should have the boolean set.");
        }

        [TestMethod]
        public void TestNetworkComponentInitialisation()
        {
            // Setup
            LocalServer localHost = new LocalServer();
            NetworkManager server = new NetworkManager(localHost);
            NetworkManager client = new NetworkManager(localHost.Connect());
            Instance serverInstance = new Instance(server);
            Instance clientInstance = new Instance(client);
            InitialisingComponent.addCount = 0;
            // Perform Functions
            serverInstance.Create(entity => {
                entity.TryAddComponent(new InitialisingComponent());
            });
            // Test Results
            Assert.AreEqual(2, InitialisingComponent.addCount, "Should have 2 component added, one for server and 1 for client");
        }

    }

    public class InitialisingComponent : Component<InitialisingComponent>
    {

        public static int addCount = 0;

        protected override void Initialise()
        {
            addCount++;
        }

    }

    public class DataStoreComponent : Component<DataStoreComponent>
    {

        public string message;
        public int number;
        public bool boolean;

		public DataStoreComponent(string message, int number, bool boolean)
		{
			this.message = message;
			this.number = number;
			this.boolean = boolean;
		}
	}
}
