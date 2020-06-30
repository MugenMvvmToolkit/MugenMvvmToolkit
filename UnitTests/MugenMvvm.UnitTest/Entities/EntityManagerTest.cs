using System;
using MugenMvvm.Entities;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Entities.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Entities
{
    public class EntityManagerTest : ComponentOwnerTestBase<EntityManager>
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetTrackingCollectionShouldBeHandledByComponents(int componentCount)
        {
            var entityManager = GetComponentOwner();
            var collection = new EntityTrackingCollection();
            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestEntityTrackingCollectionProviderComponent
                {
                    TryGetTrackingCollection = (o, type, arg3) =>
                    {
                        ++count;
                        o.ShouldEqual(entityManager);
                        type.ShouldEqual(typeof(EntityManager));
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
            var entityManager = GetComponentOwner();
            var snapshot = new TestEntityStateSnapshot();
            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestEntityStateSnapshotProviderComponent
                {
                    TryGetSnapshot = (e, o, type, arg3) =>
                    {
                        ++count;
                        e.ShouldEqual(entity);
                        o.ShouldEqual(entityManager);
                        type.ShouldEqual(typeof(EntityManager));
                        arg3.ShouldEqual(DefaultMetadata);
                        return snapshot;
                    }
                };
                entityManager.AddComponent(component);
            }

            var stateSnapshot = entityManager.TryGetSnapshot(entity, entityManager, DefaultMetadata);
            stateSnapshot.ShouldEqual(snapshot);
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetTrackingShouldNotifyListeners(int componentCount)
        {
            var entityManager = GetComponentOwner();
            var collection = new EntityTrackingCollection();
            var component = new TestEntityTrackingCollectionProviderComponent
            {
                TryGetTrackingCollection = (o, type, arg3) => collection
            };
            entityManager.AddComponent(component);

            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var listener = new TestEntityManagerListener
                {
                    OnTrackingCollectionCreated = (manager, o, arg3, arg4, arg5) =>
                    {
                        manager.ShouldEqual(entityManager);
                        o.ShouldEqual(collection);
                        arg3.ShouldEqual(entityManager);
                        arg4.ShouldEqual(typeof(EntityManager));
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
            var entityManager = GetComponentOwner();
            var snapshot = new TestEntityStateSnapshot();
            var component = new TestEntityStateSnapshotProviderComponent
            {
                TryGetSnapshot = (e, o, type, arg3) => snapshot
            };
            entityManager.AddComponent(component);

            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var listener = new TestEntityManagerListener
                {
                    OnSnapshotCreated = (manager, o, e, arg3, arg4, arg5) =>
                    {
                        manager.ShouldEqual(entityManager);
                        o.ShouldEqual(snapshot);
                        e.ShouldEqual(entity);
                        arg3.ShouldEqual(entityManager);
                        arg4.ShouldEqual(typeof(EntityManager));
                        arg5.ShouldEqual(DefaultMetadata);
                        ++count;
                    }
                };
                entityManager.AddComponent(listener);
            }

            entityManager.TryGetSnapshot(entity, entityManager, DefaultMetadata);
            count.ShouldEqual(componentCount);
        }

        [Fact]
        public void GetSnapshotShouldThrowNoComponents()
        {
            var entityManager = GetComponentOwner();
            ShouldThrow<InvalidOperationException>(() => entityManager.GetSnapshot(this, entityManager, DefaultMetadata));
        }


        [Fact]
        public void GetTrackingCollectionShouldThrowNoComponents()
        {
            var entityManager = GetComponentOwner();
            ShouldThrow<InvalidOperationException>(() => entityManager.GetTrackingCollection(entityManager, DefaultMetadata));
        }

        protected override EntityManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null)
        {
            return new EntityManager(collectionProvider);
        }

        #endregion
    }
}