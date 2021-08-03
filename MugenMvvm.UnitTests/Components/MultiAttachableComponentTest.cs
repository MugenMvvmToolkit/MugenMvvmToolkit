﻿using System.Linq;
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

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnAttachingShouldAttachOwners(int size)
        {
            var owners = new object[size];
            for (var i = 0; i < size; i++)
                owners[i] = new object();

            object? currentOwner = null;
            var methodCallCount = 0;
            _component.OnAttachingHandler = (test, context) =>
            {
                ++methodCallCount;
                test.ShouldEqual(currentOwner);
                context.ShouldEqual(Metadata);
            };
            IAttachableComponent attachable = _component;
            _component.Owners.IsEmpty.ShouldBeTrue();

            for (var i = 0; i < owners.Length; i++)
            {
                currentOwner = owners[i];
                attachable.OnAttaching(currentOwner, Metadata);
                _component.Owners.ShouldEqual(owners.Take(i + 1).ToArray());
                methodCallCount.ShouldEqual(i + 1);
            }
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
                context.ShouldEqual(Metadata);
            };
            IAttachableComponent attachable = _component;
            IDetachableComponent detachable = _component;
            for (var i = 0; i < owners.Length; i++)
                attachable.OnAttaching(owners[i], Metadata);

            _component.Owners.ShouldEqual(owners);
            methodCallCount.ShouldEqual(0);

            for (var i = 0; i < owners.Length; i++)
            {
                currentOwner = owners[i];
                detachable.OnDetached(currentOwner, Metadata);
                _component.Owners.ShouldEqual(owners.Skip(i + 1).ToArray());
                methodCallCount.ShouldEqual(i + 1);
            }

            _component.Owners.IsEmpty.ShouldBeTrue();
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
    }
}