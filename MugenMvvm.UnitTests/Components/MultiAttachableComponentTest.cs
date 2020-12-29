using System.Linq;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTests.Components.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Components
{
    public class MultiAttachableComponentTest : UnitTestBase
    {
        #region Methods

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
            var testAttachableComponent = new TestMultiAttachableComponent<object>();
            testAttachableComponent.OnAttachedHandler = (test, context) =>
            {
                ++methodCallCount;
                test.ShouldEqual(currentOwner);
                context.ShouldEqual(DefaultMetadata);
            };
            IAttachableComponent attachable = testAttachableComponent;
            testAttachableComponent.Owners.IsEmpty.ShouldBeTrue();

            for (var i = 0; i < owners.Length; i++)
            {
                currentOwner = owners[i];
                attachable.OnAttached(currentOwner, DefaultMetadata);
                testAttachableComponent.Owners.AsList().ShouldEqual(owners.Take(i + 1).ToArray());
                methodCallCount.ShouldEqual(i + 1);
            }
        }

        [Fact]
        public void OnAttachingShouldCallInternalMethod()
        {
            var methodCallCount = 0;
            var canAttach = false;
            var testAttachableComponent = new TestMultiAttachableComponent<MultiAttachableComponentTest>();
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
            var testAttachableComponent = new TestMultiAttachableComponent<object>();
            testAttachableComponent.OnDetachedHandler = (test, context) =>
            {
                ++methodCallCount;
                test.ShouldEqual(currentOwner);
                context.ShouldEqual(DefaultMetadata);
            };
            IAttachableComponent attachable = testAttachableComponent;
            IDetachableComponent detachable = testAttachableComponent;
            for (var i = 0; i < owners.Length; i++)
                attachable.OnAttached(owners[i], DefaultMetadata);

            testAttachableComponent.Owners.AsList().ShouldEqual(owners);
            methodCallCount.ShouldEqual(0);

            for (var i = 0; i < owners.Length; i++)
            {
                currentOwner = owners[i];
                detachable.OnDetached(currentOwner, DefaultMetadata);
                testAttachableComponent.Owners.AsList().ShouldEqual(owners.Skip(i + 1).ToArray());
                methodCallCount.ShouldEqual(i + 1);
            }

            testAttachableComponent.Owners.IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void OnDetachingShouldCallInternalMethod()
        {
            var methodCallCount = 0;
            var canDetach = false;
            var testAttachableComponent = new TestMultiAttachableComponent<MultiAttachableComponentTest>();

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