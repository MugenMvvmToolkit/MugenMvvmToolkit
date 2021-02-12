using System;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Components.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Components
{
    public class ComponentTrackerTest : UnitTestBase, IDisposable
    {
        private readonly ComponentCollection _componentCollection;
        private readonly ComponentTracker _componentTracker;

        public ComponentTrackerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _componentCollection = new ComponentCollection(this, ComponentCollectionManager);
            _componentTracker = new ComponentTracker();
            MugenService.Configuration.InitializeInstance<IComponentCollectionManager>(new ComponentCollectionManager());
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 10)]
        [InlineData(10, 1)]
        [InlineData(10, 10)]
        public void AttachDetachShouldUpdateComponents(int listenersCount, int componentCount)
        {
            var executed = 0;
            ItemOrArray<IComponent> expectedComponents = default;

            for (var i = 0; i < listenersCount; i++)
            {
                _componentTracker.AddListener<IComponent, ComponentTrackerTest>((components, s, arg3) =>
                {
                    ++executed;
                    s.ShouldEqual(this);
                    arg3.ShouldEqual(DefaultMetadata);
                    expectedComponents.ShouldEqual(components);
                }, this);
            }

            for (var i = 0; i < componentCount; i++)
                _componentCollection.TryAdd(new TestAttachableComponent<object>());

            expectedComponents = _componentCollection.Get<IComponent>();
            _componentTracker.Attach(_componentCollection, DefaultMetadata);
            executed.ShouldEqual(listenersCount);

            executed = 0;
            expectedComponents = Array.Empty<IComponent>();
            _componentTracker.Detach(_componentCollection, DefaultMetadata);
            executed.ShouldEqual(listenersCount);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 10)]
        [InlineData(10, 1)]
        [InlineData(10, 10)]
        public void AddShouldUpdateComponents(int listenersCount, int componentCount)
        {
            _componentTracker.Attach(_componentCollection, DefaultMetadata);
            var executed = 0;
            var expectedCount = 0;

            for (var i = 0; i < listenersCount; i++)
            {
                _componentTracker.AddListener<IComponent, ComponentTrackerTest>((components, s, arg3) =>
                {
                    ++executed;
                    s.ShouldEqual(this);
                    arg3.ShouldEqual(DefaultMetadata);
                    components.Count.ShouldEqual(expectedCount);
                    _componentCollection.Get<IComponent>().ShouldEqual(components);
                }, this);
            }

            for (var i = 0; i < componentCount; i++)
            {
                ++expectedCount;
                executed = 0;
                _componentCollection.TryAdd(new TestAttachableComponent<object>(), DefaultMetadata);
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
            for (var i = 0; i < componentCount; i++)
                _componentCollection.TryAdd(new TestAttachableComponent<object>());
            _componentTracker.Attach(_componentCollection, DefaultMetadata);
            var executed = 0;
            var expectedCount = componentCount;

            for (var i = 0; i < listenersCount; i++)
            {
                _componentTracker.AddListener<IComponent, ComponentTrackerTest>((components, s, arg3) =>
                {
                    ++executed;
                    s.ShouldEqual(this);
                    arg3.ShouldEqual(DefaultMetadata);
                    components.Count.ShouldEqual(expectedCount);
                    _componentCollection.Get<IComponent>().ShouldEqual(components);
                }, this);
            }

            foreach (var c in _componentCollection.Get<IComponent>())
            {
                --expectedCount;
                executed = 0;
                _componentCollection.Remove(c, DefaultMetadata);
                executed.ShouldEqual(listenersCount);
            }
        }

        public void Dispose() => MugenService.Configuration.Clear<IComponentCollectionManager>();
    }
}