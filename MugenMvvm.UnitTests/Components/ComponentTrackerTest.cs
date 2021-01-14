using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Components.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Components
{
    public class ComponentTrackerTest : UnitTestBase
    {
        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 10)]
        [InlineData(10, 1)]
        [InlineData(10, 10)]
        public void AttachDetachShouldUpdateComponents(int listenersCount, int componentCount)
        {
            var componentTracker = new ComponentTracker();
            var componentCollection = new ComponentCollection(this);
            var executed = 0;
            ItemOrArray<IComponent> expectedComponents = default;

            for (var i = 0; i < listenersCount; i++)
                componentTracker.AddListener<IComponent, ComponentTrackerTest>((components, s, arg3) =>
                {
                    ++executed;
                    s.ShouldEqual(this);
                    arg3.ShouldEqual(DefaultMetadata);
                    expectedComponents.ShouldEqual(components);
                }, this);

            for (var i = 0; i < componentCount; i++)
                componentCollection.TryAdd(new TestAttachableComponent<object>());

            expectedComponents = componentCollection.Get<IComponent>();
            componentTracker.Attach(componentCollection, DefaultMetadata);
            executed.ShouldEqual(listenersCount);

            executed = 0;
            expectedComponents = Default.Array<IComponent>();
            componentTracker.Detach(componentCollection, DefaultMetadata);
            executed.ShouldEqual(listenersCount);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 10)]
        [InlineData(10, 1)]
        [InlineData(10, 10)]
        public void AddShouldUpdateComponents(int listenersCount, int componentCount)
        {
            var componentTracker = new ComponentTracker();
            var componentCollection = new ComponentCollection(this);
            componentTracker.Attach(componentCollection, DefaultMetadata);
            var executed = 0;
            var expectedCount = 0;

            for (var i = 0; i < listenersCount; i++)
                componentTracker.AddListener<IComponent, ComponentTrackerTest>((components, s, arg3) =>
                {
                    ++executed;
                    s.ShouldEqual(this);
                    arg3.ShouldEqual(DefaultMetadata);
                    components.Count.ShouldEqual(expectedCount);
                    componentCollection.Get<IComponent>().ShouldEqual(components);
                }, this);

            for (var i = 0; i < componentCount; i++)
            {
                ++expectedCount;
                executed = 0;
                componentCollection.TryAdd(new TestAttachableComponent<object>(), DefaultMetadata);
                executed.ShouldEqual(listenersCount);
            }
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 10)]
        [InlineData(10, 1)]
        [InlineData(10, 10)]
        public void RemoveShouldUpdateComponents(int listenersCount, int componentCount)
        {
            var componentTracker = new ComponentTracker();
            var componentCollection = new ComponentCollection(this);
            for (var i = 0; i < componentCount; i++)
                componentCollection.TryAdd(new TestAttachableComponent<object>());
            componentTracker.Attach(componentCollection, DefaultMetadata);
            var executed = 0;
            var expectedCount = componentCount;

            for (var i = 0; i < listenersCount; i++)
                componentTracker.AddListener<IComponent, ComponentTrackerTest>((components, s, arg3) =>
                {
                    ++executed;
                    s.ShouldEqual(this);
                    arg3.ShouldEqual(DefaultMetadata);
                    components.Count.ShouldEqual(expectedCount);
                    componentCollection.Get<IComponent>().ShouldEqual(components);
                }, this);

            foreach (var c in componentCollection.Get<IComponent>())
            {
                --expectedCount;
                executed = 0;
                componentCollection.Remove(c, DefaultMetadata);
                executed.ShouldEqual(listenersCount);
            }
        }
    }
}