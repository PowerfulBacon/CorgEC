using ECCore.Attributes;
using ECCore.Components;
using ECCore.Instances;
using JetBrains.Annotations;

namespace ECCore.Tests
{
    [TestClass]
    public class ComponentTests
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