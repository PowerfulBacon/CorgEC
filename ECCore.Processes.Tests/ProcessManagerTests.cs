using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECCore.Processes.Tests
{
    [TestClass]
    public class ProcessManagerTests
    {

        [TestMethod]
        public void TestInitialised()
        {
            TestResults results = new TestResults();
            ProcessManager manager = ProcessManager.FromBuilder()
                .WithService(new ExampleProcess(results))
                .Build();
            Assert.IsTrue(results.initialised);
        }

        [TestMethod]
        public void TestFired()
        {
            TestResults results = new TestResults();
            ProcessManager manager = ProcessManager.FromBuilder()
                .WithService(new ExampleProcess(results))
                .Build();
            manager.Fire();
            Assert.IsTrue(results.fired);
        }

    }

    public class TestResults
    {
        public bool initialised = false;
        public bool fired = false;
    }

    public class ExampleProcess : ECProcess
    {
        protected override ECProcessFlags Flags => ECProcessFlags.NONE;

        protected override TimeSpan Delay { get; } = TimeSpan.FromMilliseconds(1);

        private TestResults testResults;

        public ExampleProcess(TestResults testResults)
        {
            this.testResults = testResults;
        }

        protected override void Initialise()
        {
            testResults.initialised = true;
        }

        protected override void Process(TimeSpan timeElapsed)
        {
            testResults.fired = true;
        }
    }

}
