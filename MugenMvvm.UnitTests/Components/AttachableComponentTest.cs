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
        public void OnAttachingShouldAttachOwner()
        {
            var methodCallCount = 0;
            _component.OnAttachingHandler = (test, context) =>
            {
                ++methodCallCount;
                test.ShouldEqual(this);
                context.ShouldEqual(Metadata);
            };
            IAttachableComponent attachable = _component;
            _component.IsAttached.ShouldBeFalse();

            attachable.OnAttaching(this, Metadata);
            _component.IsAttached.ShouldBeTrue();
            _component.Owner.ShouldEqual(this);
            methodCallCount.ShouldEqual(1);
        }

        [Fact]
        public void OnAttachedShouldIgnoreWrongOwner()
        {
            IAttachableComponent attachable = _component;
            attachable.OnAttached(new object(), Metadata);
            _component.IsAttached.ShouldBeFalse();
        }

        [Fact]
        public void OnAttachedShouldThrowIfOwnerInitialized()
        {
            IAttachableComponent attachable = _component;
            attachable.OnAttaching(this, Metadata);
            ShouldThrow<InvalidOperationException>(() => attachable.OnAttaching(this, Metadata));
        }

        [Fact]
        public void OnAttachedShouldCallInternalMethod()
        {
            var methodCallCount = 0;
            _component.OnAttachedHandler = (test, context) =>
            {
                ++methodCallCount;
                test.ShouldEqual(this);
                context.ShouldEqual(Metadata);
            };

            IAttachableComponent attachable = _component;
            attachable.OnAttached(this, Metadata);
            methodCallCount.ShouldEqual(1);
        }

        [Fact]
        public void OnDetachedShouldDetachOwner()
        {
            var methodCallCount = 0;
            _component.OnDetachedHandler = (test, context) =>
            {
                ++methodCallCount;
                test.ShouldEqual(this);
                context.ShouldEqual(Metadata);
            };
            IAttachableComponent attachable = _component;
            IDetachableComponent detachable = _component;
            attachable.OnAttaching(this, Metadata);

            _component.IsAttached.ShouldBeTrue();
            _component.Owner.ShouldEqual(this);
            methodCallCount.ShouldEqual(0);

            detachable.OnDetached(this, Metadata);
            methodCallCount.ShouldEqual(1);
            _component.IsAttached.ShouldBeFalse();
        }

        [Fact]
        public void OnDetachingShouldCallInternalMethod()
        {
            var methodCallCount = 0;
            IDetachableComponent attachable = _component;
            _component.OnDetachingHandler = (test, context) =>
            {
                ++methodCallCount;
                test.ShouldEqual(this);
                context.ShouldEqual(Metadata);
            };
            attachable.OnDetaching(this, Metadata);
            methodCallCount.ShouldEqual(1);

            attachable.OnDetaching(this, Metadata);
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