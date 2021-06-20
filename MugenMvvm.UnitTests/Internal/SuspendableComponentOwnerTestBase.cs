using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Internal;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Internal
{
    public abstract class SuspendableComponentOwnerTestBase<T> : ComponentOwnerTestBase<T> where T : class, IComponentOwner, ISuspendable
    {
        protected SuspendableComponentOwnerTestBase(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public virtual void IsSuspendedShouldBeHandledByComponents(int componentCount)
        {
            var componentOwner = GetComponentOwner(ComponentCollectionManager);
            componentOwner.IsSuspended.ShouldBeFalse();

            for (var i = 0; i < componentCount; i++)
            {
                var suspendableComponent = GetSuspendableComponent();
                componentOwner.Components.TryAdd(suspendableComponent);
            }

            componentOwner.IsSuspended.ShouldBeFalse();
            var suspendableComponents = componentOwner.GetComponents<TestSuspendableComponent<T>>();
            foreach (var suspendableComponent in suspendableComponents)
                suspendableComponent.IsSuspended = true;
            componentOwner.IsSuspended.ShouldBeTrue();

            foreach (var suspendableComponent in suspendableComponents)
                suspendableComponent.IsSuspended = false;
            componentOwner.IsSuspended.ShouldBeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public virtual void SuspendedShouldUseComponentValues(int componentCount)
        {
            var componentOwner = GetComponentOwner(ComponentCollectionManager);
            componentOwner.Suspend(this, DefaultMetadata).IsEmpty.ShouldBeTrue();

            var methodCallCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var suspendableComponent = GetSuspendableComponent();
                suspendableComponent.TrySuspend = (o, s, m) =>
                {
                    o.ShouldEqual(componentOwner);
                    s.ShouldEqual(this);
                    m.ShouldEqual(DefaultMetadata);
                    return ActionToken.FromDelegate((o, o1) => methodCallCount++);
                };
                componentOwner.Components.TryAdd(suspendableComponent);
            }

            var actionToken = componentOwner.Suspend(this, DefaultMetadata);
            methodCallCount.ShouldEqual(0);

            actionToken.Dispose();
            methodCallCount.ShouldEqual(componentCount);
        }

        protected virtual TestSuspendableComponent<T> GetSuspendableComponent() => new();
    }
}