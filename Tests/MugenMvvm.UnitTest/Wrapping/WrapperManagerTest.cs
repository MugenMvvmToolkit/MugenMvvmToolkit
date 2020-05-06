using System;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Wrapping.Internal;
using MugenMvvm.Wrapping;
using MugenMvvm.Wrapping.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Wrapping
{
    public class WrapperManagerTest : ComponentOwnerTestBase<WrapperManager>
    {
        #region Methods

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
                var component = new DelegateWrapperManager<object>((targetType, wrapperType, state, metadata) =>
                {
                    ++executeCount;
                    targetType.ShouldEqual(expectedTargetType);
                    wrapperType.ShouldEqual(wrapperType);
                    state.ShouldEqual(this);
                    metadata.ShouldEqual(DefaultMetadata);
                    return result;
                }, (o, type1, arg3, arg4) => null, this)
                {
                    Priority = -i
                };
                manager.AddComponent(component);
            }

            manager.CanWrap(expectedTargetType, expectedWrapperType, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);

            executeCount = 0;
            result = true;
            manager.CanWrap(expectedTargetType, expectedWrapperType, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(1);
        }

        [Fact]
        public void WrapShouldThrowNoComponents()
        {
            var manager = new WrapperManager();
            ShouldThrow<ArgumentException>(() => manager.Wrap(this, typeof(IComponent), DefaultMetadata));
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
                var component = new DelegateWrapperManager<object>((targetType, wrapperType, state, metadata) => true,
                    (t, wrapperType, state, metadata) =>
                    {
                        ++executeCount;
                        t.ShouldEqual(manager);
                        wrapperType.ShouldEqual(expectedWrapperType);
                        state.ShouldEqual(this);
                        metadata.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return result;
                        return null;
                    }, this)
                {
                    Priority = -i
                };
                manager.AddComponent(component);
                manager.AddComponent(new TestWrapperManagerListener
                {
                    OnWrapped = (wrapperManager, wrapper, item, wrapperType, metadata) =>
                    {
                        ++listenerExecuteCount;
                        wrapperManager.ShouldEqual(manager);
                        wrapper.ShouldEqual(result);
                        item.ShouldEqual(manager);
                        wrapperType.ShouldEqual(expectedWrapperType);
                        metadata.ShouldEqual(DefaultMetadata);
                    }
                });
            }

            manager.Wrap(manager, expectedWrapperType, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);
            listenerExecuteCount.ShouldEqual(count);
        }

        protected override WrapperManager GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new WrapperManager(collectionProvider);
        }

        #endregion
    }
}