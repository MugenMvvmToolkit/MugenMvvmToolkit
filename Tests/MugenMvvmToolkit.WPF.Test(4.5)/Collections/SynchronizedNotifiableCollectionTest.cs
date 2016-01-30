using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
using Should;

namespace MugenMvvmToolkit.Test.Collections
{
    [TestClass]
    public class SynchronizedNotifiableCollectionTest : CollectionTestBase
    {
        #region Fields

        protected ThreadManagerMock ThreadManagerMock;

        #endregion

        #region Test methods

        [TestInitialize]
        public void Init()
        {
            ThreadManagerMock = new ThreadManagerMock();
        }

        [TestMethod]
        public void WhenNotificationSuspendedEventsShouldNotBeRaised()
        {
            SynchronizedNotifiableCollection<Item> collection = CreateNotifiableCollection<Item>(ExecutionMode.None, ThreadManagerMock);
            var collectionTracker = new NotifiableCollectionTracker<Item>(collection);

            using (collection.SuspendNotifications())
            {
                for (int i = 0; i < 10; i++)
                {
                    var item = new Item();
                    collection.Add(item);
                    collection.Remove(item);
                }
                using (collection.SuspendNotifications())
                {
                    collection.Add(new Item());
                }
                collectionTracker.ChangedItems.ShouldBeEmpty();
            }
            ThreadManagerMock.InvokeOnUiThreadAsync();
            collectionTracker.AssertEquals();
        }

        [TestMethod]
        public void WhenOperationWasCanceledCollectionShouldNotBeChanged()
        {
            SynchronizedNotifiableCollection<Item> collection = CreateNotifiableCollection<Item>(ExecutionMode.None, ThreadManagerMock);
            collection.Add(new Item { Id = -1 });
            collection.CollectionChanging += (sender, args) => args.Cancel = true;
            var collectionTracker = new NotifiableCollectionTracker<Item>(collection, false);

            var item = new Item { Id = -2 };
            collection.Add(item);
            collection.Remove(item);
            collection[0] = item;
            collection.Clear();

            collection.Count.ShouldEqual(1);
            collection[0].ShouldNotEqual(item);

            collectionTracker.ChangedItems.SequenceEqual(collection).ShouldBeTrue();
            collectionTracker.ChangingItems.SequenceEqual(collection).ShouldBeTrue();
        }

        [TestMethod]
        public void CollectionShouldNotRaiseEventsUsingThreadManagerIfModeNone()
        {
            SynchronizedNotifiableCollection<Item> collection = CreateNotifiableCollection<Item>(ExecutionMode.None,
                ThreadManagerMock);
            collection.Add(new Item());
            var collectionTracker = new NotifiableCollectionTracker<Item>(collection);
            collectionTracker.AssertEquals();
            collection.Count.ShouldEqual(1);
        }

        [TestMethod]
        public void CollectionShouldRaiseEventsUsingThreadManager()
        {
            SynchronizedNotifiableCollection<Item> collection = CreateNotifiableCollection<Item>(ExecutionMode.AsynchronousOnUiThread, ThreadManagerMock);
            collection.Add(new Item());

            ThreadManagerMock.InvokeOnUiThreadAsync();
            var collectionTracker = new NotifiableCollectionTracker<Item>(collection);
            collectionTracker.AssertEquals();
        }

        [TestMethod]
        public virtual void CollectionShouldTrackChangesCorrect()
        {
            const int count = 10;
            SynchronizedNotifiableCollection<Item> collection = CreateNotifiableCollection<Item>(ExecutionMode.None, ThreadManagerMock);
            collection.BatchSize = int.MaxValue;
            var collectionTracker = new NotifiableCollectionTracker<Item>(collection);
            using (collection.SuspendNotifications())
            {
                var item = new Item();
                var items = new[] { new Item(), new Item(), new Item() };
                var items2 = new[] { new Item(), new Item(), new Item() };
                for (int i = 0; i < count; i++)
                {
                    collection.AddRange(items);
                    collection.AddRange(items2);
                    collection.RemoveRange(items);
                }
                for (int i = 0; i < collection.Count; i++)
                {
                    collection[i] = item;
                }
            }
            ThreadManagerMock.InvokeOnUiThreadAsync();
            collectionTracker.AssertEquals();
            collection.Count.ShouldEqual(count * 3);
        }

        [TestMethod]
        public virtual void CollectionShouldTrackChangesCorrectBatchSize()
        {
            const int count = 10;
            SynchronizedNotifiableCollection<Item> collection = CreateNotifiableCollection<Item>(ExecutionMode.None, ThreadManagerMock);
            collection.BatchSize = 10;
            var collectionTracker = new NotifiableCollectionTracker<Item>(collection);
            using (collection.SuspendNotifications())
            {
                var item = new Item();
                var items = new[] { new Item(), new Item(), new Item() };
                var items2 = new[] { new Item(), new Item(), new Item() };
                for (int i = 0; i < count; i++)
                {
                    collection.AddRange(items);
                    collection.AddRange(items2);
                    collection.RemoveRange(items);
                }
                for (int i = 0; i < collection.Count; i++)
                {
                    collection[i] = item;
                }
            }
            ThreadManagerMock.InvokeOnUiThreadAsync();
            collectionTracker.AssertEquals();
            collection.Count.ShouldEqual(count * 3);
        }

        #endregion

        #region Methods

        protected virtual SynchronizedNotifiableCollection<T> CreateNotifiableCollection<T>(ExecutionMode executionMode,
            IThreadManager threadManager)
        {
            return new SynchronizedNotifiableCollection<T>
            {
                ThreadManager = threadManager
            };
        }

        #endregion

        #region Overrides of CollectionTestBase

        protected override ICollection<T> CreateCollection<T>(params T[] items)
        {
            if (items.Length == 0)
                return new SynchronizedNotifiableCollection<T>();
            return new SynchronizedNotifiableCollection<T>(items);
        }

        #endregion
    }

    [TestClass]
    public class SynchronizedNotifiableCollectionSerializationTest :
        SerializationTestBase<SynchronizedNotifiableCollection<string>>
    {
        #region Overrides of SerializationTestBase

        protected override SynchronizedNotifiableCollection<string> GetObject()
        {
            return new SynchronizedNotifiableCollection<string>(TestExtensions.TestStrings);
        }

        protected override void AssertObject(SynchronizedNotifiableCollection<string> deserializedObj)
        {
            deserializedObj.SequenceEqual(TestExtensions.TestStrings).ShouldBeTrue();
            deserializedObj.IsNotificationsSuspended.ShouldBeFalse();
        }

        #endregion
    }

    [TestClass]
    public class SynchronizedNotifiableCollectionSuspendCollectionTest : CollectionTestBase
    {
        #region Overrides of CollectionTestBase

        protected override ICollection<T> CreateCollection<T>(params T[] items)
        {
            return new SynchronizedNotifiableCollection<T>(collection: items);
        }

        #endregion
    }
}
