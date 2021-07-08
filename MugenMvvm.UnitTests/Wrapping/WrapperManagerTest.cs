using System;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Tests.Wrapping;
using MugenMvvm.UnitTests.Components;
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
            var executeCount = 0;
            var expectedTargetType = GetType();
            var expectedWrapperType = typeof(bool);
            var result = false;
            for (var i = 0; i < count; i++)
            {
                WrapperManager.AddComponent(new DelegateWrapperManager<object, object>((wrapperType, targetType, metadata) =>
                {
                    ++executeCount;
                    targetType.ShouldEqual(expectedTargetType);
                    wrapperType.ShouldEqual(wrapperType);
                    metadata.ShouldEqual(DefaultMetadata);
                    return result;
                }, (o, type1, arg4) => null)
                {
                    Priority = -i
                });
            }

            WrapperManager.CanWrap(expectedWrapperType, expectedTargetType, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);

            executeCount = 0;
            result = true;
            WrapperManager.CanWrap(expectedWrapperType, expectedTargetType, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void WrapShouldBeHandledByComponents(int count)
        {
            var executeCount = 0;
            var listenerExecuteCount = 0;
            var expectedWrapperType = typeof(bool);
            object result = true;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                WrapperManager.AddComponent(new DelegateWrapperManager<object, object>((wrapperType, targetType, metadata) => true,
                    (wrapperType, t, metadata) =>
                    {
                        ++executeCount;
                        t.ShouldEqual(WrapperManager);
                        wrapperType.ShouldEqual(expectedWrapperType);
                        metadata.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return result;
                        return null;
                    })
                {
                    Priority = -i
                });
                WrapperManager.AddComponent(new TestWrapperManagerListener
                {
                    OnWrapped = (m, wrapper, item, metadata) =>
                    {
                        ++listenerExecuteCount;
                        m.ShouldEqual(WrapperManager);
                        wrapper.ShouldEqual(result);
                        item.ShouldEqual(WrapperManager);
                        metadata.ShouldEqual(DefaultMetadata);
                    }
                });
            }

            WrapperManager.Wrap(expectedWrapperType, WrapperManager, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);
            listenerExecuteCount.ShouldEqual(count);
        }

        [Fact]
        public void WrapShouldThrowNoComponents() => ShouldThrow<ArgumentException>(() => WrapperManager.Wrap(typeof(IComponent), this, DefaultMetadata));

        protected override IWrapperManager GetWrapperManager() => GetComponentOwner(ComponentCollectionManager);

        protected override WrapperManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}