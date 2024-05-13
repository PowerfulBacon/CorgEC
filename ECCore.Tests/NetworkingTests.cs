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
        }

        [TestMethod]
        public void TestEntityCreation()
        {
            // Setup
            LocalServer localHost = new LocalServer();
            NetworkManager server = new NetworkManager(localHost);
            NetworkManager client = new NetworkManager(localHost.Connect());
            server.onException += e =>
            {
                Assert.Fail(e.ToString());
            };
            client.onException += e =>
            {
                Assert.Fail(e.ToString());
            };
            Instance serverInstance = new Instance(server);
            Instance clientInstance = new Instance(client);
            // Perform Functions

            // Test Results
            Assert.Fail("This test hasn't been written yet");
        }

        [TestMethod]
        public void TestComponentAdded()
        {
            // Setup
            LocalServer localHost = new LocalServer();
            NetworkManager server = new NetworkManager(localHost);
            NetworkManager client = new NetworkManager(localHost.Connect());
            server.onException += e =>
            {
                Assert.Fail(e.ToString());
            };
            client.onException += e =>
            {
                Assert.Fail(e.ToString());
            };
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
}
