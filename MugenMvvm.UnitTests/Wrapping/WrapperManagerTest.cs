using System;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Wrapping.Internal;
using MugenMvvm.Wrapping;
using MugenMvvm.Wrapping.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Wrapping
{
    public class WrapperManagerTest : ComponentOwnerTestBase<WrapperManager>
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CanWrapShouldBeHandledByComponents(int count)
        {
            var manager = new WrapperManager();
            var executeCount = 0;
            var expectedTargetType = GetType();
            var expectedWrapperType = typeof(bool);
            var result = false;
            for (var i = 0; i < count; i++)
            {
                var component = new DelegateWrapperManager<object, object>((wrapperType, targetType, metadata) =>
                {
                    ++executeCount;
                    targetType.ShouldEqual(expectedTargetType);
                    wrapperType.ShouldEqual(wrapperType);
                    metadata.ShouldEqual(DefaultMetadata);
                    return result;
                }, (o, type1, arg4) => null)
                {
                    Priority = -i
                };
                manager.AddComponent(component);
            }

            manager.CanWrap(expectedWrapperType, expectedTargetType, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);

            executeCount = 0;
            result = true;
            manager.CanWrap(expectedWrapperType, expectedTargetType, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void WrapShouldBeHandledByComponents(int count)
        {
            var manager = new WrapperManager();
            var executeCount = 0;
            var listenerExecuteCount = 0;
            var expectedWrapperType = typeof(bool);
            object result = true;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new DelegateWrapperManager<object, object>((wrapperType, targetType, metadata) => true,
                    (wrapperType, t, metadata) =>
                    {
                        ++executeCount;
                        t.ShouldEqual(manager);
                        wrapperType.ShouldEqual(expectedWrapperType);
                        metadata.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return result;
                        return null;
                    })
                {
                    Priority = -i
                };
                manager.AddComponent(component);
                manager.AddComponent(new TestWrapperManagerListener(manager)
                {
                    OnWrapped = (wrapper, item, metadata) =>
                    {
                        ++listenerExecuteCount;
                        wrapper.ShouldEqual(result);
                        item.ShouldEqual(manager);
                        metadata.ShouldEqual(DefaultMetadata);
                    }
                });
            }

            manager.Wrap(expectedWrapperType, manager, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);
            listenerExecuteCount.ShouldEqual(count);
        }

        protected override WrapperManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new(collectionProvider);

        [Fact]
        public void WrapShouldThrowNoComponents()
        {
            var manager = new WrapperManager();
            ShouldThrow<ArgumentException>(() => manager.Wrap(typeof(IComponent), this, DefaultMetadata));
        }
    }
}