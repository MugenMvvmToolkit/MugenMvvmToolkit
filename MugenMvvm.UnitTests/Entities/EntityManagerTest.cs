﻿using System;
using MugenMvvm.Entities;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Tests.Entities;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Entities
{
    public class EntityManagerTest : ComponentOwnerTestBase<EntityManager>
    {
        [Fact]
        public void GetSnapshotShouldThrowNoComponents() => ShouldThrow<InvalidOperationException>(() => EntityManager.GetSnapshot(this, DefaultMetadata));

        [Fact]
        public void GetTrackingCollectionShouldThrowNoComponents() =>
            ShouldThrow<InvalidOperationException>(() => EntityManager.GetTrackingCollection(EntityManager, DefaultMetadata));

        protected override IEntityManager GetEntityManager() => GetComponentOwner(ComponentCollectionManager);

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetTrackingCollectionShouldBeHandledByComponents(int componentCount)
        {
            var collection = new EntityTrackingCollection();
            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                EntityManager.AddComponent(new TestEntityTrackingCollectionProviderComponent
                {
                    TryGetTrackingCollection = (m, o, arg3) =>
                    {
                        ++count;
                        m.ShouldEqual(EntityManager);
                        o.ShouldEqual(this);
                        arg3.ShouldEqual(DefaultMetadata);
                        return collection;
                    }
                });
            }

            var trackingCollection = EntityManager.TryGetTrackingCollection(this, DefaultMetadata);
            trackingCollection.ShouldEqual(collection);
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetSnapshotShouldBeHandledByComponents(int componentCount)
        {
            var entity = new object();
            var snapshot = new TestEntityStateSnapshot();
            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                EntityManager.AddComponent(new TestEntityStateSnapshotProviderComponent
                {
                    TryGetSnapshot = (m, e, arg3) =>
                    {
                        ++count;
                        m.ShouldEqual(EntityManager);
                        e.ShouldEqual(entity);
                        arg3.ShouldEqual(DefaultMetadata);
                        return snapshot;
                    }
                });
            }

            var stateSnapshot = EntityManager.TryGetSnapshot(entity, DefaultMetadata);
            stateSnapshot.ShouldEqual(snapshot);
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetTrackingShouldNotifyListeners(int componentCount)
        {
            var collection = new EntityTrackingCollection();
            var component = new TestEntityTrackingCollectionProviderComponent
            {
                TryGetTrackingCollection = (_, _, _) => collection
            };
            EntityManager.AddComponent(component);

            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                EntityManager.AddComponent(new TestEntityManagerListener
                {
                    OnTrackingCollectionCreated = (m, o, arg3, arg5) =>
                    {
                        m.ShouldEqual(EntityManager);
                        o.ShouldEqual(collection);
                        arg3.ShouldEqual(EntityManager);
                        arg5.ShouldEqual(DefaultMetadata);
                        ++count;
                    }
                });
            }

            EntityManager.TryGetTrackingCollection(EntityManager, DefaultMetadata);
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetSnapshotShouldNotifyListeners(int componentCount)
        {
            var entity = new object();
            var snapshot = new TestEntityStateSnapshot();
            var component = new TestEntityStateSnapshotProviderComponent
            {
                TryGetSnapshot = (_, _, _) => snapshot
            };
            EntityManager.AddComponent(component);

            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                EntityManager.AddComponent(new TestEntityManagerListener
                {
                    OnSnapshotCreated = (m, o, e, arg5) =>
                    {
                        m.ShouldEqual(EntityManager);
                        o.ShouldEqual(snapshot);
                        e.ShouldEqual(entity);
                        arg5.ShouldEqual(DefaultMetadata);
                        ++count;
                    }
                });
            }

            EntityManager.TryGetSnapshot(entity, DefaultMetadata);
            count.ShouldEqual(componentCount);
        }

        protected override EntityManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}