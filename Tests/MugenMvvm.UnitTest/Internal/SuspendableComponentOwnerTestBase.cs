using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Internal
{
    public abstract class SuspendableComponentOwnerTestBase<T> : ComponentOwnerTestBase<T> where T : class, IComponentOwner, ISuspendable
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public virtual void IsSuspendedShouldBeHandledByComponents(int componentCount)
        {
            var componentOwner = GetComponentOwner();
            componentOwner.IsSuspended.ShouldBeFalse();

            for (var i = 0; i < componentCount; i++)
            {
                var suspendableComponent = GetSuspendableComponent();
                componentOwner.Components.Add(suspendableComponent);
            }

            componentOwner.IsSuspended.ShouldBeFalse();
            var suspendableComponents = componentOwner.GetComponents<TestSuspendableComponent>();
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
            var componentOwner = GetComponentOwner();
            componentOwner.Suspend().IsEmpty.ShouldBeTrue();

            var methodCallCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var suspendableComponent = GetSuspendableComponent();
                suspendableComponent.Suspend = () => new ActionToken((o, o1) => methodCallCount++);
                componentOwner.Components.Add(suspendableComponent);
            }

            var actionToken = componentOwner.Suspend();
            methodCallCount.ShouldEqual(0);

            actionToken.Dispose();
            methodCallCount.ShouldEqual(componentCount);
        }

        protected virtual TestSuspendableComponent GetSuspendableComponent()
        {
            return new TestSuspendableComponent();
        }

        #endregion
    }
}