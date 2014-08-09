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
    public class OrderedNotifiableCollectionTest : SynchronizedNotifiableCollectionTest
    {
        [TestMethod]
        public override void GlobalSettingTest()
        {
            ApplicationSettings.SetDefaultValues();
            //By default
            var collection = new OrderedNotifiableCollection<Item>((item, item1) => item.Id.CompareTo(item1.Id));
            collection.ExecutionMode.ShouldEqual(ExecutionMode.AsynchronousOnUiThread);

            ApplicationSettings.SynchronizedCollectionExecutionMode = ExecutionMode.None;
            collection = new OrderedNotifiableCollection<Item>();
            collection.ExecutionMode.ShouldEqual(ExecutionMode.None);
        }

        private static OrderedNotifiableCollection<Item> CreateNotifiableCollection(ExecutionMode executionMode,
            IThreadManager threadManager, IEnumerable<Item> items = null)
        {
            if (items == null)
                items = Enumerable.Empty<Item>();
            return new OrderedNotifiableCollection<Item>(items, (item, item1) => item.Id.CompareTo(item1.Id))
            {
                ExecutionMode = executionMode,
                ThreadManager = threadManager
            };
        }

        #region Overrides of CollectionTestBase

        [TestMethod]
        public override void CollectionShouldTrackChangesCorrect()
        {
            const int count = 10;
            SynchronizedNotifiableCollection<Item> collection = CreateNotifiableCollection<Item>(ExecutionMode.None,
                ThreadManagerMock);
            var collectionTracker = new NotifiableCollectionTracker<Item>(collection);
            collection.BatchSize = int.MaxValue;
            var items = new[] {new Item(), new Item(), new Item()};
            var items2 = new[] {new Item(), new Item(), new Item()};
            using (collection.SuspendNotifications())
            {
                for (int i = 0; i < count; i++)
                {
                    collection.AddRange(items);
                    collection.AddRange(items2);
                    collection.RemoveRange(items);
                }
            }
            collectionTracker.AssertEquals();
            collection.Count.ShouldEqual(count*3);
        }

        protected override SynchronizedNotifiableCollection<T> CreateNotifiableCollection<T>(ExecutionMode executionMode,
            IThreadManager threadManager)
        {
            Should.BeOfType(typeof (Item), "type", typeof (T));
            return
                (SynchronizedNotifiableCollection<T>)(object)CreateNotifiableCollection(executionMode, threadManager);
        }

        protected override ICollection<T> CreateCollection<T>(params T[] items)
        {
            Should.BeOfType(typeof (Item), "type", typeof (T));
            return (ICollection<T>) CreateNotifiableCollection(ExecutionMode.None, null, items.OfType<Item>());
        }

        #endregion
    }

    [TestClass]
    public class OrderedNotifiableCollectionSerializationTest :
        SerializationTestBase<OrderedNotifiableCollection<string>>
    {
        #region Overrides of SerializationTestBase

        protected override OrderedNotifiableCollection<string> GetObject()
        {
            return new OrderedNotifiableCollection<string>(TestExtensions.TestStrings)
            {
                ExecutionMode = ExecutionMode.None
            };
        }

        protected override void AssertObject(OrderedNotifiableCollection<string> deserializedObj)
        {
            deserializedObj.Items.ShouldBeType<OrderedListInternal<string>>();
            deserializedObj.SequenceEqual(TestExtensions.TestStrings).ShouldBeTrue();
            deserializedObj.IsNotificationsSuspended.ShouldBeFalse();
            deserializedObj.EventsTracker.ShouldNotBeNull();
        }

        #endregion
    }
}