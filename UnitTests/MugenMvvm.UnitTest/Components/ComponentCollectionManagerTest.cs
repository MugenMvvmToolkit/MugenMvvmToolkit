﻿using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Components.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Components
{
    public class ComponentCollectionManagerTest : ComponentOwnerTestBase<IComponentCollectionManager>
    {
        #region Methods

        [Fact]
        public void GetComponentCollectionShouldUseComponentCollectionAsFallback()
        {
            var collectionProvider = GetComponentOwner();
            var componentCollection = collectionProvider.GetComponentCollection(this);
            componentCollection.ShouldBeType<ComponentCollection>();
        }

        [Fact]
        public void GetComponentCollectionShouldBeHandledByComponents()
        {
            var executed = 0;
            var result = new ComponentCollection(this);
            var collectionProvider = GetComponentOwner();
            var component = new TestComponentCollectionProviderComponent
            {
                TryGetComponentCollection = (c, o, context) =>
                {
                    ++executed;
                    c.ShouldEqual(collectionProvider);
                    o.ShouldEqual(this);
                    context.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };
            collectionProvider.AddComponent(component);

            collectionProvider.GetComponentCollection(this, DefaultMetadata).ShouldEqual(result);
            executed.ShouldEqual(1);
        }

        [Fact]
        public void GetComponentCollectionShouldNotifyListeners()
        {
            var executed = 0;
            var result = new ComponentCollection(this);
            var collectionProvider = GetComponentOwner();
            var component = new TestComponentCollectionProviderComponent
            {
                TryGetComponentCollection = (c, o, context) =>
                {
                    c.ShouldEqual(collectionProvider);
                    return result;
                }
            };
            collectionProvider.AddComponent(component);

            var listener = new TestComponentCollectionManagerListener();
            listener.OnComponentCollectionCreated = (provider, collection, arg3) =>
            {
                executed++;
                provider.ShouldEqual(collectionProvider);
                collection.ShouldEqual(result);
                arg3.ShouldEqual(DefaultMetadata);
            };
            collectionProvider.AddComponent(listener);

            collectionProvider.GetComponentCollection(this, DefaultMetadata).ShouldEqual(result);
            executed.ShouldEqual(1);
        }

        public override void ComponentOwnerShouldUseCollectionFactory(bool globalValue)
        {
        }

        protected override IComponentCollectionManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null)
        {
            return new ComponentCollectionManager();
        }

        #endregion
    }
}