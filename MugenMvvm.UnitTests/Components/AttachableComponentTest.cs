using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTests.Components.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Components
{
    public class AttachableComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void OwnerShouldThrowNotAttached()
        {
            var testAttachableComponent = new TestAttachableComponent<AttachableComponentTest>();
            ShouldThrow<InvalidOperationException>(() =>
            {
                var v = testAttachableComponent.Owner;
            });
        }

        [Fact]
        public void OnAttachedShouldThrowIfOwnerInitialized()
        {
            var testAttachableComponent = new TestAttachableComponent<AttachableComponentTest>();
            IAttachableComponent attachable = testAttachableComponent;

            attachable.OnAttached(this, DefaultMetadata);
            ShouldThrow<InvalidOperationException>(() => { attachable.OnAttached(this, DefaultMetadata); });
        }

        [Fact]
        public void OnAttachedShouldIgnoreWrongOwner()
        {
            var testAttachableComponent = new TestAttachableComponent<AttachableComponentTest>();
            IAttachableComponent attachable = testAttachableComponent;

            attachable.OnAttached(new object(), DefaultMetadata);
            testAttachableComponent.IsAttached.ShouldBeFalse();
        }

        [Fact]
        public void OnAttachedShouldAttachOwner()
        {
            var methodCallCount = 0;
            var testAttachableComponent = new TestAttachableComponent<AttachableComponentTest>();
            testAttachableComponent.OnAttachedHandler = (test, context) =>
            {
                ++methodCallCount;
                test.ShouldEqual(this);
                context.ShouldEqual(DefaultMetadata);
            };
            IAttachableComponent attachable = testAttachableComponent;
            testAttachableComponent.IsAttached.ShouldBeFalse();

            attachable.OnAttached(this, DefaultMetadata);
            testAttachableComponent.IsAttached.ShouldBeTrue();
            testAttachableComponent.Owner.ShouldEqual(this);
            methodCallCount.ShouldEqual(1);
        }

        [Fact]
        public void OnAttachingShouldCallInternalMethod()
        {
            var methodCallCount = 0;
            var canAttach = false;
            var testAttachableComponent = new TestAttachableComponent<AttachableComponentTest>();
            testAttachableComponent.OnAttachingHandler = (test, context) =>
            {
                ++methodCallCount;
                test.ShouldEqual(this);
                context.ShouldEqual(DefaultMetadata);
                return canAttach;
            };

            IAttachableComponent attachable = testAttachableComponent;
            attachable.OnAttaching(this, DefaultMetadata).ShouldEqual(canAttach);
            methodCallCount.ShouldEqual(1);

            canAttach = true;
            attachable.OnAttaching(this, DefaultMetadata).ShouldEqual(canAttach);
            methodCallCount.ShouldEqual(2);
        }

        [Fact]
        public void OnDetachedShouldDetachOwner()
        {
            var methodCallCount = 0;
            var testAttachableComponent = new TestAttachableComponent<AttachableComponentTest>();
            testAttachableComponent.OnDetachedHandler = (test, context) =>
            {
                ++methodCallCount;
                test.ShouldEqual(this);
                context.ShouldEqual(DefaultMetadata);
            };
            IAttachableComponent attachable = testAttachableComponent;
            IDetachableComponent detachable = testAttachableComponent;
            attachable.OnAttached(this, DefaultMetadata);

            testAttachableComponent.IsAttached.ShouldBeTrue();
            testAttachableComponent.Owner.ShouldEqual(this);
            methodCallCount.ShouldEqual(0);

            detachable.OnDetached(this, DefaultMetadata);
            methodCallCount.ShouldEqual(1);
            testAttachableComponent.IsAttached.ShouldBeFalse();
        }

        [Fact]
        public void OnDetachingShouldCallInternalMethod()
        {
            var methodCallCount = 0;
            var canDetach = false;
            var testAttachableComponent = new TestAttachableComponent<AttachableComponentTest>();

            IDetachableComponent attachable = testAttachableComponent;
            attachable.OnDetaching(this, DefaultMetadata).ShouldBeTrue();

            testAttachableComponent.OnDetachingHandler = (test, context) =>
            {
                ++methodCallCount;
                test.ShouldEqual(this);
                context.ShouldEqual(DefaultMetadata);
                return canDetach;
            };
            attachable.OnDetaching(this, DefaultMetadata).ShouldEqual(canDetach);
            methodCallCount.ShouldEqual(1);

            canDetach = true;
            attachable.OnDetaching(this, DefaultMetadata).ShouldEqual(canDetach);
            methodCallCount.ShouldEqual(2);
        }

        #endregion
    }
}