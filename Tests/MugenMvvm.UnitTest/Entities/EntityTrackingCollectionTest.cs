using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Entities;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Entities.Internal;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Entities
{
    public class EntityTrackingCollectionTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldAddRemoveClear(int count)
        {
            var items = new List<object>();
            var collection = new EntityTrackingCollection();
            collection.HasChanges.ShouldBeFalse();

            for (var i = 0; i < count; i++)
            {
                var o = new object();
                items.Add(o);
                collection.SetState(o, EntityState.Added);
            }

            collection.Count.ShouldEqual(count);
            collection.HasChanges.ShouldBeTrue();

            var changes = collection.GetChanges(this, (entity, test) => entity.State != EntityState.Unchanged);
            changes.Count.ShouldEqual(count);
            items.All(entry => changes.Any(stateEntry => stateEntry.Entity == entry)).ShouldBeTrue();

            changes = collection.GetChanges(this, (entity, test) => entity.State == EntityState.Added);
            changes.Count.ShouldEqual(count);
            items.All(entry => changes.Any(stateEntry => stateEntry.Entity == entry)).ShouldBeTrue();

            foreach (var item in items)
            {
                collection.GetState(item).ShouldEqual(EntityState.Added);
                collection.GetChanges(this, (entity, t) => entity.Entity == item && entity.State == EntityState.Added).Single().Entity.ShouldEqual(item);

                collection.SetState(item, EntityState.Unchanged);
            }

            collection.Count.ShouldEqual(count);
            collection.HasChanges.ShouldBeFalse();
            collection.GetChanges(this, (entity, test) => entity.State != EntityState.Unchanged).ShouldBeEmpty();

            collection.SetState(new object(), EntityState.Added);
            collection.HasChanges.ShouldBeTrue();

            collection.Clear();
            collection.Count.ShouldEqual(0);
            collection.HasChanges.ShouldBeFalse();
            collection.GetChanges(this, (entity, test) => entity.State != EntityState.Unchanged).ShouldBeEmpty();
        }

        [Fact]
        public void ShouldRemoveObjectOnDetachState()
        {
            var target = new object();
            var collection = new EntityTrackingCollection();

            collection.GetState(target).ShouldEqual(EntityState.Detached);
            collection.SetState(target, EntityState.Added);
            collection.GetState(target).ShouldEqual(EntityState.Added);

            collection.SetState(target, EntityState.Detached);
            collection.GetState(target).ShouldEqual(EntityState.Detached);
            collection.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldUseCustomEqualityComparer()
        {
            var comparer = new TestEqualityComparer<object>
            {
                GetHashCode = model => ((TestModel) model).IntProperty,
                Equals = (model, testModel) => ((TestModel) model).IntProperty == ((TestModel) testModel).IntProperty
            };
            var collection = new EntityTrackingCollection(comparer);

            var item1 = new TestModel {IntProperty = 1};
            var item2 = new TestModel {IntProperty = 1};
            collection.SetState(item1, EntityState.Added);
            collection.Count.ShouldEqual(1);

            collection.SetState(item2, EntityState.Added);
            collection.Count.ShouldEqual(1);

            var changes = collection.GetChanges(this, (entity, test) => entity.State == EntityState.Added);
            changes.Count.ShouldEqual(1);
            changes[0].Entity.ShouldEqual(item2);
        }

        [Fact]
        public void ShouldNotifyListeners()
        {
            var entity = new object();
            var stateFrom = EntityState.Detached;
            var expectedChangingState = EntityState.Modified;
            var expectedChangedState = EntityState.Added;
            var changingInvokeCount = 0;
            var changedInvokeCount = 0;
            var collection = new EntityTrackingCollection();
            var changingListener = new TestEntityStateChangingListener
            {
                OnEntityStateChanging = (trackingCollection, o, arg3, arg4, arg5) =>
                {
                    ++changingInvokeCount;
                    o.ShouldEqual(entity);
                    arg3.ShouldEqual(stateFrom);
                    arg4.ShouldEqual(expectedChangingState);
                    arg5.ShouldEqual(DefaultMetadata);
                    trackingCollection.ShouldEqual(collection);
                    return expectedChangedState;
                }
            };
            var changedListener = new TestEntityStateChangedListener
            {
                OnEntityStateChanged = (trackingCollection, o, arg3, arg4, arg5) =>
                {
                    ++changedInvokeCount;
                    o.ShouldEqual(entity);
                    arg3.ShouldEqual(stateFrom);
                    arg4.ShouldEqual(expectedChangedState);
                    arg5.ShouldEqual(DefaultMetadata);
                    trackingCollection.ShouldEqual(collection);
                }
            };
            collection.AddComponent(changingListener);
            collection.AddComponent(changedListener);

            collection.SetState(entity, expectedChangingState, DefaultMetadata);
            changingInvokeCount.ShouldEqual(1);
            changedInvokeCount.ShouldEqual(1);

            stateFrom = expectedChangedState;
            expectedChangedState = EntityState.Detached;
            collection.Clear(DefaultMetadata);
            changingInvokeCount.ShouldEqual(1);
            changedInvokeCount.ShouldEqual(2);
        }

        #endregion

        #region Nested types

        private sealed class TestModel
        {
            #region Properties

            public int IntProperty { get; set; }

            #endregion
        }

        #endregion
    }
}