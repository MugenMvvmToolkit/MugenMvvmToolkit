using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.Infrastructure;
using MugenMvvmToolkit.Test.TestInfrastructure;
using Should;

namespace MugenMvvmToolkit.Test.Collections
{
    [TestClass]
    public class TrackingCollectionTest : TestBase
    {
        #region Fields

        private readonly StateTransitionManagerMock _transitionManager = new StateTransitionManagerMock();

        #endregion

        #region Methods

        [TestMethod]
        public void UpdateStateParametersTest()
        {
            var target = new object();
            bool isInvoked = false;
            var oldState = EntityState.Unchanged;
            var newState = EntityState.Unchanged;
            bool validate = false;

            _transitionManager.ChangeState = (state, entityState, arg3) =>
            {
                isInvoked = true;
                state.ShouldEqual(oldState);
                entityState.ShouldEqual(newState);
                arg3.ShouldEqual(validate);
                return entityState;
            };
            ITrackingCollection collection = Create(_transitionManager);

            oldState = EntityState.Detached;
            newState = EntityState.Added;
            collection.UpdateState(target, newState, validate);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            oldState = EntityState.Added;
            newState = EntityState.Modified;
            validate = true;
            collection.UpdateState(target, newState, validate);
        }

        [TestMethod]
        public void CollectionShouldUseStateFromManager()
        {
            var target = new object();
            _transitionManager.ChangeState = (state, entityState, arg3) => entityState;
            var collection = Create(_transitionManager);

            collection.GetState(target).ShouldEqual(EntityState.Detached);
            collection.UpdateState(target, EntityState.Added, false);
            collection.GetState(target).ShouldEqual(EntityState.Added);

            collection.UpdateState(target, EntityState.Unchanged, false);
            collection.GetState(target).ShouldEqual(EntityState.Unchanged);
        }

        [TestMethod]
        public void CollectionShouldRemoveObjectOnDetachState()
        {
            var target = new object();
            _transitionManager.ChangeState = (state, entityState, arg3) => entityState;
            var collection = Create(_transitionManager);

            collection.GetState(target).ShouldEqual(EntityState.Detached);
            collection.UpdateState(target, EntityState.Added, false);
            collection.GetState(target).ShouldEqual(EntityState.Added);

            collection.UpdateState(target, EntityState.Detached, false);
            collection.GetState(target).ShouldEqual(EntityState.Detached);
            collection.Count.ShouldEqual(0);
        }

        [TestMethod]
        public void MainOperationTest()
        {
            const int count = 100;
            var items = new List<object>();
            _transitionManager.ChangeState = (state, entityState, arg3) => entityState;
            var collection = Create(_transitionManager);
            collection.HasChanges.ShouldBeFalse();

            for (int i = 0; i < 100; i++)
            {
                var o = new object();
                items.Add(o);
                collection.UpdateState(o, EntityState.Added);
            }

            collection.Count.ShouldEqual(count);
            collection.HasChanges.ShouldBeTrue();

            var changes = collection.GetChanges();
            changes.Count.ShouldEqual(count);
            items.All(entry => changes.Any(stateEntry => stateEntry.Entity == entry)).ShouldBeTrue();

            changes = collection.GetChanges(EntityState.Added);
            changes.Count.ShouldEqual(count);
            items.All(entry => changes.Any(stateEntry => stateEntry.Entity == entry)).ShouldBeTrue();

            foreach (var item in items)
            {
                collection.GetState(item).ShouldEqual(EntityState.Added);
                collection.Contains(item).ShouldBeTrue();
                collection.Find<object>(entity => entity.Entity == item && entity.State.IsAdded()).Single().ShouldEqual(item);

                collection.UpdateState(item, EntityState.Unchanged);
            }
            collection.Count.ShouldEqual(count);
            collection.HasChanges.ShouldBeFalse();
            collection.GetChanges().ShouldBeEmpty();

            collection.UpdateState(new object(), EntityState.Added);
            collection.HasChanges.ShouldBeTrue();

            collection.Clear();
            collection.Count.ShouldEqual(0);
            collection.HasChanges.ShouldBeFalse();
            collection.GetChanges().ShouldBeEmpty();
        }

        protected virtual ITrackingCollection Create(IStateTransitionManager transitionManager = null,
            IEqualityComparer<object> comparer = null)
        {
            return new TrackingCollection(transitionManager, comparer);
        }

        #endregion
    }

    [TestClass]
    public class TrackingCollectionSerializationTest : SerializationTestBase<TrackingCollection>
    {
        #region Fields

        private static readonly object Target = new object();

        #endregion

        #region Overrides of SerializationTestBase

        [Ignore]
        public override void TestXmlSerialization()
        {         
        }

        protected override TrackingCollection GetObject()
        {
            var c = new TrackingCollection();
            c.UpdateState(Target, EntityState.Added);
            return c;
        }

        protected override void AssertObject(TrackingCollection deserializedObj)
        {
            deserializedObj.GetChanges(EntityState.Added).Single().Entity.ShouldNotBeNull();
            deserializedObj.Count.ShouldEqual(1);
            deserializedObj.StateTransitionManager.ShouldNotBeNull();
        }

        #endregion
    }
}