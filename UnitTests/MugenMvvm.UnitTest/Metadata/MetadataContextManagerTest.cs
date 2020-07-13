using System;
using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Metadata.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Metadata
{
    public class MetadataContextManagerTest : ComponentOwnerTestBase<MetadataContextManager>
    {
        #region Methods

        [Fact]
        public void GetReadOnlyMetadataContextShouldThrowNoComponents()
        {
            ShouldThrow<InvalidOperationException>(() => new MetadataContextManager().GetReadOnlyMetadataContext());
        }

        [Fact]
        public void GetMetadataContextShouldThrowNoComponents()
        {
            ShouldThrow<InvalidOperationException>(() => new MetadataContextManager().GetMetadataContext());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetReadOnlyMetadataContextShouldBeHandledByComponents(int count)
        {
            var result = DefaultMetadata;
            var contextProvider = new MetadataContextManager();
            var items = ItemOrList.FromRawValue<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>(DefaultMetadata);
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var canReturn = i == count - 1;
                var component = new TestMetadataContextProviderComponent();
                component.Priority = -i;
                component.TryGetReadOnlyMetadataContext = (m, o, context) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(contextProvider);
                    o.ShouldEqual(this);
                    context.ShouldEqual(items);
                    if (canReturn)
                        return result;
                    return null;
                };
                contextProvider.AddComponent(component);
            }

            contextProvider.GetReadOnlyMetadataContext(this, items).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMetadataContextShouldBeHandledByComponents(int count)
        {
            var result = new MetadataContext();
            var contextProvider = new MetadataContextManager();
            var items = ItemOrList.FromRawValue<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>(DefaultMetadata);
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var canReturn = i == count - 1;
                var component = new TestMetadataContextProviderComponent();
                component.Priority = -i;
                component.TryGetMetadataContext = (m, o, context) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(contextProvider);
                    o.ShouldEqual(this);
                    context.ShouldEqual(items);
                    if (canReturn)
                        return result;
                    return null;
                };
                contextProvider.AddComponent(component);
            }

            contextProvider.GetMetadataContext(this, items).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetReadOnlyMetadataContextShouldNotifyListeners(int componentCount)
        {
            var result = DefaultMetadata;
            var contextProvider = new MetadataContextManager();
            var component = new TestMetadataContextProviderComponent { TryGetReadOnlyMetadataContext = (m, o, context) => result };
            contextProvider.AddComponent(component);
            var invokeCount = 0;

            for (var i = 0; i < componentCount; i++)
            {
                var listener = new TestMetadataContextManagerListener
                {
                    OnReadOnlyContextCreated = (provider, context, arg3) =>
                    {
                        ++invokeCount;
                        provider.ShouldEqual(contextProvider);
                        context.ShouldEqual(result);
                        arg3.ShouldEqual(this);
                    }
                };
                contextProvider.AddComponent(listener);
            }

            contextProvider.GetReadOnlyMetadataContext(this).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMetadataContextShouldNotifyListeners(int componentCount)
        {
            var result = new MetadataContext();
            var contextProvider = new MetadataContextManager();
            var component = new TestMetadataContextProviderComponent { TryGetMetadataContext = (m, o, context) => result };
            contextProvider.AddComponent(component);
            var invokeCount = 0;

            for (var i = 0; i < componentCount; i++)
            {
                var listener = new TestMetadataContextManagerListener
                {
                    OnContextCreated = (provider, context, arg3) =>
                    {
                        ++invokeCount;
                        provider.ShouldEqual(contextProvider);
                        context.ShouldEqual(result);
                        arg3.ShouldEqual(this);
                    }
                };
                contextProvider.AddComponent(listener);
            }

            contextProvider.GetMetadataContext(this).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        protected override MetadataContextManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null)
        {
            return new MetadataContextManager(collectionProvider);
        }

        #endregion
    }
}