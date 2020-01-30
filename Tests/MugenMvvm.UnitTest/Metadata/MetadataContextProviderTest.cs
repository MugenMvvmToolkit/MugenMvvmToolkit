using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Metadata
{
    public class MetadataContextProviderTest : ComponentOwnerTestBase<MetadataContextProvider>
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetReadOnlyMetadataContextShouldBeHandledByComponents(int count)
        {
            var result = DefaultMetadata;
            var contextProvider = new MetadataContextProvider();
            var items = new ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>(DefaultMetadata);
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var canReturn = i == count - 1;
                var component = new TestMetadataContextProviderComponent();
                component.Priority = -i;
                component.TryGetReadOnlyMetadataContext = (o, context) =>
                {
                    ++invokeCount;
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
            var contextProvider = new MetadataContextProvider();
            var items = new ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>(DefaultMetadata);
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var canReturn = i == count - 1;
                var component = new TestMetadataContextProviderComponent();
                component.Priority = -i;
                component.TryGetMetadataContext = (o, context) =>
                {
                    ++invokeCount;
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
            var contextProvider = new MetadataContextProvider();
            var component = new TestMetadataContextProviderComponent {TryGetReadOnlyMetadataContext = (o, context) => result};
            contextProvider.AddComponent(component);
            var invokeCount = 0;

            for (var i = 0; i < componentCount; i++)
            {
                var listener = new TestMetadataContextProviderListener
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
            var contextProvider = new MetadataContextProvider();
            var component = new TestMetadataContextProviderComponent {TryGetMetadataContext = (o, context) => result};
            contextProvider.AddComponent(component);
            var invokeCount = 0;

            for (var i = 0; i < componentCount; i++)
            {
                var listener = new TestMetadataContextProviderListener
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

        protected override MetadataContextProvider GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new MetadataContextProvider(collectionProvider);
        }

        #endregion
    }
}