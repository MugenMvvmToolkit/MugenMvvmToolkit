using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTests.Components.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Components
{
    public class AttachableComponentTest : UnitTestBase
    {
        private readonly TestAttachableComponent<AttachableComponentTest> _component;

        public AttachableComponentTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _component = new TestAttachableComponent<AttachableComponentTest>();
        }

        [Fact]
        public void OnAttachedShouldAttachOwner()
        {
            var methodCallCount = 0;
            _component.OnAttachedHandler = (test, context) =>
            {
                ++methodCallCount;
                test.ShouldEqual(this);
                context.ShouldEqual(DefaultMetadata);
            };
            IAttachableComponent attachable = _component;
            _component.IsAttached.ShouldBeFalse();

            attachable.OnAttached(this, DefaultMetadata);
            _component.IsAttached.ShouldBeTrue();
            _component.Owner.ShouldEqual(this);
            methodCallCount.ShouldEqual(1);
        }

        [Fact]
        public void OnAttachedShouldIgnoreWrongOwner()
        {
            IAttachableComponent attachable = _component;
            attachable.OnAttached(new object(), DefaultMetadata);
            _component.IsAttached.ShouldBeFalse();
        }

        [Fact]
        public void OnAttachedShouldThrowIfOwnerInitialized()
        {
            IAttachableComponent attachable = _component;
            attachable.OnAttached(this, DefaultMetadata);
            ShouldThrow<InvalidOperationException>(() => { attachable.OnAttached(this, DefaultMetadata); });
        }

        [Fact]
        public void OnAttachingShouldCallInternalMethod()
        {
            var methodCallCount = 0;
            var canAttach = false;
            _component.OnAttachingHandler = (test, context) =>
            {
                ++methodCallCount;
                test.ShouldEqual(this);
                context.ShouldEqual(DefaultMetadata);
                return canAttach;
            };

            IAttachableComponent attachable = _component;
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
            _component.OnDetachedHandler = (test, context) =>
            {
                ++methodCallCount;
                test.ShouldEqual(this);
                context.ShouldEqual(DefaultMetadata);
            };
            IAttachableComponent attachable = _component;
            IDetachableComponent detachable = _component;
            attachable.OnAttached(this, DefaultMetadata);

            _component.IsAttached.ShouldBeTrue();
            _component.Owner.ShouldEqual(this);
            methodCallCount.ShouldEqual(0);

            detachable.OnDetached(this, DefaultMetadata);
            methodCallCount.ShouldEqual(1);
            _component.IsAttached.ShouldBeFalse();
        }

        [Fact]
        public void OnDetachingShouldCallInternalMethod()
        {
            var methodCallCount = 0;
            var canDetach = false;

            IDetachableComponent attachable = _component;
            attachable.OnDetaching(this, DefaultMetadata).ShouldBeTrue();

            _component.OnDetachingHandler = (test, context) =>
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

        [Fact]
        public void OwnerShouldThrowNotAttached() =>
            ShouldThrow<InvalidOperationException>(() =>
            {
                var v = _component.Owner;
            });
    }
}