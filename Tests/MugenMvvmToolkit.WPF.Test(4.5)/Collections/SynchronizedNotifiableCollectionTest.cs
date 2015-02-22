using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        public virtual void GlobalSettingTest()
        {
            ApplicationSettings.SetDefaultValues();
            //By default
            var collection = new SynchronizedNotifiableCollection<Item>();
            collection.ExecutionMode.ShouldEqual(ExecutionMode.AsynchronousOnUiThread);

            ApplicationSettings.SynchronizedCollectionExecutionMode = ExecutionMode.None;
            collection = new SynchronizedNotifiableCollection<Item>();
            collection.ExecutionMode.ShouldEqual(ExecutionMode.None);
        }

        [TestMethod]
        public void WhenNotificationSuspendedEventsShouldNotBeRaised()
        {
            SynchronizedNotifiableCollection<Item> collection = CreateNotifiableCollection<Item>(ExecutionMode.None,
                ThreadManagerMock);
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
            collectionTracker.AssertEquals();
        }

        [TestMethod]
        public void WhenOperationWasCanceledCollectionShouldNotBeChanged()
        {
            SynchronizedNotifiableCollection<Item> collection = CreateNotifiableCollection<Item>(ExecutionMode.None,
                ThreadManagerMock);
            collection.Add(new Item {Id = new Guid("0C32E17E-020C-4E05-9B90-AE247B8BE703")});

            collection.CollectionChanging += (sender, args) => args.Cancel = true;
            var collectionTracker = new NotifiableCollectionTracker<Item>(collection);

            var item = new Item {Id = new Guid("3C39C0C0-DFBA-4683-8473-0950085478E9")};
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
        public void CollectionShouldRaiseEventsUsingThreadManagerIfModeAsynchronous()
        {
            SynchronizedNotifiableCollection<Item> collection =
                CreateNotifiableCollection<Item>(ExecutionMode.Asynchronous, ThreadManagerMock);
            collection.Add(new Item());
            collection.NotificationCount.ShouldEqual(0);

            ThreadManagerMock.InvokeAsync();
            var collectionTracker = new NotifiableCollectionTracker<Item>(collection);
            collectionTracker.AssertEquals();
            collection.NotificationCount.ShouldEqual(1);
        }

        [TestMethod]
        public void CollectionShouldRaiseEventsUsingThreadManagerIfModeAsynchronousInUi()
        {
            SynchronizedNotifiableCollection<Item> collection =
                CreateNotifiableCollection<Item>(ExecutionMode.AsynchronousOnUiThread, ThreadManagerMock);
            collection.Add(new Item());
            collection.NotificationCount.ShouldEqual(0);

            ThreadManagerMock.InvokeOnUiThreadAsync();
            var collectionTracker = new NotifiableCollectionTracker<Item>(collection);
            collectionTracker.AssertEquals();
            collection.NotificationCount.ShouldEqual(1);
        }

        [TestMethod]
        public void CollectionShouldRaiseEventsUsingThreadManagerIfModeSynchronousInUi()
        {
            SynchronizedNotifiableCollection<Item> collection =
                CreateNotifiableCollection<Item>(ExecutionMode.SynchronousOnUiThread, ThreadManagerMock);
            collection.Add(new Item());
            collection.NotificationCount.ShouldEqual(0);

            ThreadManagerMock.InvokeOnUiThread();
            var collectionTracker = new NotifiableCollectionTracker<Item>(collection);
            collectionTracker.AssertEquals();
            collection.NotificationCount.ShouldEqual(1);
        }

        [TestMethod]
        public virtual void CollectionShouldTrackChangesCorrect()
        {
            const int count = 10;
            SynchronizedNotifiableCollection<Item> collection = CreateNotifiableCollection<Item>(ExecutionMode.None,
                ThreadManagerMock);
            var collectionTracker = new NotifiableCollectionTracker<Item>(collection);
            collection.BatchSize = int.MaxValue;
            using (collection.SuspendNotifications())
            {
                var item = new Item();
                var items = new[] {new Item(), new Item(), new Item()};
                var items2 = new[] {new Item(), new Item(), new Item()};
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
            collectionTracker.AssertEquals();
            collection.Count.ShouldEqual(count*3);
        }

        [TestMethod]
        public void CollectionShouldReturnCountUsingMode()
        {
            SynchronizedNotifiableCollection<Item> collection =
                CreateNotifiableCollection<Item>(ExecutionMode.AsynchronousOnUiThread, ThreadManagerMock);
            IList list = collection;
            IList<Item> genericList = collection;
            collection.Add(new Item());

            collection.NotificationMode = NotificationCollectionMode.None;
            list.Count.ShouldEqual(1);
            genericList.Count.ShouldEqual(1);
            collection.Count.ShouldEqual(1);
            collection.NotificationCount.ShouldEqual(0);

            collection.NotificationMode = NotificationCollectionMode.CollectionIntefaceUseNotificationValue;
            list.Count.ShouldEqual(0);
            genericList.Count.ShouldEqual(1);
            collection.Count.ShouldEqual(1);
            collection.NotificationCount.ShouldEqual(0);

            collection.NotificationMode = NotificationCollectionMode.GenericCollectionInterfaceUseNotificationValue;
            list.Count.ShouldEqual(1);
            genericList.Count.ShouldEqual(0);
            collection.Count.ShouldEqual(1);
            collection.NotificationCount.ShouldEqual(0);

            collection.NotificationMode = NotificationCollectionMode.GenericCollectionInterfaceUseNotificationValue
                                   | NotificationCollectionMode.CollectionIntefaceUseNotificationValue;
            list.Count.ShouldEqual(0);
            genericList.Count.ShouldEqual(0);
            collection.Count.ShouldEqual(1);
            collection.NotificationCount.ShouldEqual(0);
        }

        [TestMethod]
        public void CollectionShouldChangeEventToClearIfUsingBatchSize()
        {
            bool isInvoked = false;
            SynchronizedNotifiableCollection<Item> collection = CreateNotifiableCollection<Item>(ExecutionMode.None,
                ThreadManagerMock);
            collection.CollectionChanged +=
                (sender, args) =>
                {
                    args.Action.ShouldEqual(NotifyCollectionChangedAction.Reset);
                    isInvoked = true;
                };
            collection.BatchSize = 1;
            using (collection.SuspendNotifications())
            {
                collection.Add(new Item());
                collection.Add(new Item());
            }
            isInvoked.ShouldBeTrue();
        }

        #endregion

        #region Methods

        protected virtual SynchronizedNotifiableCollection<T> CreateNotifiableCollection<T>(ExecutionMode executionMode,
            IThreadManager threadManager)
        {
            return new SynchronizedNotifiableCollection<T>
            {
                ExecutionMode = executionMode,
                ThreadManager = threadManager
            };
        }

        #endregion

        #region Overrides of CollectionTestBase

        protected override ICollection<T> CreateCollection<T>(params T[] items)
        {
            if (items.Length == 0)
                return new SynchronizedNotifiableCollection<T> { ExecutionMode = ExecutionMode.None };
            return new SynchronizedNotifiableCollection<T>(items) { ExecutionMode = ExecutionMode.None };
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
            return new SynchronizedNotifiableCollection<string>(TestExtensions.TestStrings)
            {
                ExecutionMode = ExecutionMode.None
            };
        }

        protected override void AssertObject(SynchronizedNotifiableCollection<string> deserializedObj)
        {
            deserializedObj.SequenceEqual(TestExtensions.TestStrings).ShouldBeTrue();
            deserializedObj.IsNotificationsSuspended.ShouldBeFalse();
            deserializedObj.EventsTracker.ShouldNotBeNull();
        }

        #endregion
    }

    [TestClass]
    public class SynchronizedNotifiableCollectionSuspendCollectionTest : CollectionTestBase
    {
        #region Overrides of CollectionTestBase

        protected override ICollection<T> CreateCollection<T>(params T[] items)
        {
            return new SynchronizedNotifiableCollection<T>(collection: items)
            {
                ExecutionMode = ExecutionMode.None,
            };
        }

        #endregion
    }
}