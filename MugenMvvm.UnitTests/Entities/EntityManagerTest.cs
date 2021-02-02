using System;
using MugenMvvm.Entities;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Entities.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Entities
{
    public class EntityManagerTest : ComponentOwnerTestBase<EntityManager>
    {
        [Fact]
        public void GetSnapshotShouldThrowNoComponents()
        {
            var entityManager = GetComponentOwner(ComponentCollectionManager);
            ShouldThrow<InvalidOperationException>(() => entityManager.GetSnapshot(this, DefaultMetadata));
        }

        [Fact]
        public void GetTrackingCollectionShouldThrowNoComponents()
        {
            var entityManager = GetComponentOwner(ComponentCollectionManager);
            ShouldThrow<InvalidOperationException>(() => entityManager.GetTrackingCollection(entityManager, DefaultMetadata));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetTrackingCollectionShouldBeHandledByComponents(int componentCount)
        {
            var entityManager = GetComponentOwner(ComponentCollectionManager);
            var collection = new EntityTrackingCollection();
            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestEntityTrackingCollectionProviderComponent(entityManager)
                {
                    TryGetTrackingCollection = (o, arg3) =>
                    {
                        ++count;
                        o.ShouldEqual(entityManager);
                        arg3.ShouldEqual(DefaultMetadata);
                        return collection;
                    }
                };
                entityManager.AddComponent(component);
            }

            var trackingCollection = entityManager.TryGetTrackingCollection(entityManager, DefaultMetadata);
            trackingCollection.ShouldEqual(collection);
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetSnapshotShouldBeHandledByComponents(int componentCount)
        {
            var entity = new object();
            var entityManager = GetComponentOwner(ComponentCollectionManager);
            var snapshot = new TestEntityStateSnapshot();
            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestEntityStateSnapshotProviderComponent(entityManager)
                {
                    TryGetSnapshot = (e, arg3) =>
                    {
                        ++count;
                        e.ShouldEqual(entity);
                        arg3.ShouldEqual(DefaultMetadata);
                        return snapshot;
                    }
                };
                entityManager.AddComponent(component);
            }

            var stateSnapshot = entityManager.TryGetSnapshot(entity, DefaultMetadata);
            stateSnapshot.ShouldEqual(snapshot);
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetTrackingShouldNotifyListeners(int componentCount)
        {
            var entityManager = GetComponentOwner(ComponentCollectionManager);
            var collection = new EntityTrackingCollection();
            var component = new TestEntityTrackingCollectionProviderComponent(entityManager)
            {
                TryGetTrackingCollection = (o, arg3) => collection
            };
            entityManager.AddComponent(component);

            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var listener = new TestEntityManagerListener(entityManager)
                {
                    OnTrackingCollectionCreated = (o, arg3, arg5) =>
                    {
                        o.ShouldEqual(collection);
                        arg3.ShouldEqual(entityManager);
                        arg5.ShouldEqual(DefaultMetadata);
                        ++count;
                    }
                };
                entityManager.AddComponent(listener);
            }

            entityManager.TryGetTrackingCollection(entityManager, DefaultMetadata);
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetSnapshotShouldNotifyListeners(int componentCount)
        {
            var entity = new object();
            var entityManager = GetComponentOwner(ComponentCollectionManager);
            var snapshot = new TestEntityStateSnapshot();
            var component = new TestEntityStateSnapshotProviderComponent(entityManager)
            {
                TryGetSnapshot = (e, arg3) => snapshot
            };
            entityManager.AddComponent(component);

            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var listener = new TestEntityManagerListener(entityManager)
                {
                    OnSnapshotCreated = (o, e, arg5) =>
                    {
                        o.ShouldEqual(snapshot);
                        e.ShouldEqual(entity);
                        arg5.ShouldEqual(DefaultMetadata);
                        ++count;
                    }
                };
                entityManager.AddComponent(listener);
            }

            entityManager.TryGetSnapshot(entity, DefaultMetadata);
            count.ShouldEqual(componentCount);
        }

        protected override EntityManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}