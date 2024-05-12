using ECCore.Attributes;
using ECCore.Components;
using ECCore.Instances;

namespace ECCore.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestComponentAdding()
        {
            Instance instance = new Instance();
            Entity entity = instance.Create(entity => {
                entity.TryAddComponent(new TestComponent());
            });
            TestComponent.accept = false;
            entity.GetSignalContext<TestSignal>().Raise(new TestSignal());
            Assert.IsTrue(TestComponent.accept);
        }

        [TestMethod]
        public void ComponentAddPerformance()
        {
            Instance instance = new Instance();
            for (int i = 0; i < 100000; i++)
            {
                Entity entity = instance.Create(entity =>
                {
                    entity.TryAddComponent(new TestComponent());
                });
                entity.GetSignalContext<TestSignal>().Raise(new TestSignal());
            }
        }
    }

    public class TestComponent : Component<TestComponent>
    {

        public static bool accept = false;

        [OnSignal(AcceptFrom.Anyone, RunOn.Everyone)]
        void HandleSignal(TestSignal signal)
        {
            accept = true;
        }

    }

    public class TestSignal : Signal<TestSignal>
    {

    }
}