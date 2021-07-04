using System.Linq;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTests.Components.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Components
{
    public class MultiAttachableComponentTest : UnitTestBase
    {
        private readonly TestMultiAttachableComponent<object> _component;

        public MultiAttachableComponentTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _component = new TestMultiAttachableComponent<object>();
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

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnAttachedShouldAttachOwners(int size)
        {
            var owners = new object[size];
            for (var i = 0; i < size; i++)
                owners[i] = new object();

            object? currentOwner = null;
            var methodCallCount = 0;
            _component.OnAttachedHandler = (test, context) =>
            {
                ++methodCallCount;
                test.ShouldEqual(currentOwner);
                context.ShouldEqual(DefaultMetadata);
            };
            IAttachableComponent attachable = _component;
            _component.Owners.IsEmpty.ShouldBeTrue();

            for (var i = 0; i < owners.Length; i++)
            {
                currentOwner = owners[i];
                attachable.OnAttached(currentOwner, DefaultMetadata);
                _component.Owners.ShouldEqual(owners.Take(i + 1).ToArray());
                methodCallCount.ShouldEqual(i + 1);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnDetachedShouldDetachOwner(int size)
        {
            var owners = new object[size];
            for (var i = 0; i < size; i++)
                owners[i] = new object();

            var methodCallCount = 0;
            object? currentOwner = null;
            _component.OnDetachedHandler = (test, context) =>
            {
                ++methodCallCount;
                test.ShouldEqual(currentOwner);
                context.ShouldEqual(DefaultMetadata);
            };
            IAttachableComponent attachable = _component;
            IDetachableComponent detachable = _component;
            for (var i = 0; i < owners.Length; i++)
                attachable.OnAttached(owners[i], DefaultMetadata);

            _component.Owners.ShouldEqual(owners);
            methodCallCount.ShouldEqual(0);

            for (var i = 0; i < owners.Length; i++)
            {
                currentOwner = owners[i];
                detachable.OnDetached(currentOwner, DefaultMetadata);
                _component.Owners.ShouldEqual(owners.Skip(i + 1).ToArray());
                methodCallCount.ShouldEqual(i + 1);
            }

            _component.Owners.IsEmpty.ShouldBeTrue();
        }
    }
}