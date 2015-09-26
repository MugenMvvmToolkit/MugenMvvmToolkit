using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class FilterableNotifiableCollectionTest : SynchronizedNotifiableCollectionTest
    {
        [TestMethod]
        public override void GlobalSettingTest()
        {
            ApplicationSettings.SetDefaultValues();
            //By default
            var collection = new FilterableNotifiableCollection<Item>();
            collection.ExecutionMode.ShouldEqual(ExecutionMode.AsynchronousOnUiThread);

            ApplicationSettings.SynchronizedCollectionExecutionMode = ExecutionMode.None;
            collection = new FilterableNotifiableCollection<Item>();
            collection.ExecutionMode.ShouldEqual(ExecutionMode.None);
        }

        [TestMethod]
        public void FilterShouldCorrectNotifyAboutChanges()
        {
            var item = new Item();
            var item2 = new Item();
            var collection =
                (FilterableNotifiableCollection<Item>)
                    CreateNotifiableCollection<Item>(ExecutionMode.None, ThreadManagerMock);
            var tracker = new NotifiableCollectionTracker<Item>(collection);

            collection.Add(item);
            collection.Contains(item).ShouldBeTrue();
            collection.Clear();
            tracker.AssertChangedEquals();

            collection.Filter = item1 => false;
            collection.Add(item);
            collection.Contains(item).ShouldBeFalse();
            tracker.AssertChangedEquals();

            collection.Filter = null;
            collection.Contains(item).ShouldBeTrue();
            tracker.AssertChangedEquals();

            collection.Add(item);
            collection.Remove(item).ShouldBeTrue();
            collection.Contains(item).ShouldBeTrue();
            tracker.AssertChangedEquals();

            collection.Filter = item1 => false;
            collection.Add(item);
            collection.Remove(item).ShouldBeFalse();
            tracker.AssertChangedEquals();

            collection.Filter = null;
            collection.Contains(item).ShouldBeTrue();
            tracker.AssertChangedEquals();
            collection.Clear();


            collection.Add(item);
            collection.Add(item2);
            collection.Filter = item1 => item == item1;
            tracker.AssertChangedEquals();

            collection[0] = item2;
            collection.Count.ShouldEqual(0);
            tracker.AssertChangedEquals();

            collection.Filter = null;
            collection[0].ShouldEqual(item2);
            tracker.AssertChangedEquals();
            collection.Clear();

            collection.Add(item);
            collection.Add(item2);
            collection.Filter = item1 => item == item1;
            tracker.AssertChangedEquals();

            collection.Clear();
            collection.Filter = null;
            collection.Count.ShouldEqual(0);
            tracker.AssertChangedEquals();
        }

        [TestMethod]
        public void CollectionShouldTrackChangesCorrectWithFilter()
        {
            const int count = 10;
            var collection =
                (FilterableNotifiableCollection<Item>)
                    CreateNotifiableCollection<Item>(ExecutionMode.None, ThreadManagerMock);
            collection.Filter = item => !item.Hidden;

            var collectionTracker = new NotifiableCollectionTracker<Item>(collection);
            collection.BatchSize = int.MaxValue;
            using (collection.SuspendNotifications())
            {
                var item = new Item();
                var items = new[] {new Item(), new Item(), new Item()};
                var items2 = new[] {new Item {Hidden = true}, new Item(), new Item {Hidden = true}};
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
            collectionTracker.ChangingItems.OrderBy(item => item.Id)
                .SequenceEqual(collection.SourceCollection.OrderBy(item => item.Id))
                .ShouldBeTrue();
            collectionTracker.AssertChangedEquals();
            collection.Count.ShouldEqual(count);
        }

        #region Overrides of CollectionTestBase

        protected override SynchronizedNotifiableCollection<T> CreateNotifiableCollection<T>(ExecutionMode executionMode,
            IThreadManager threadManager)
        {
            return new FilterableNotifiableCollection<T>
            {
                ExecutionMode = executionMode,
                ThreadManager = threadManager,
                Filter = item => true
            };
        }

        protected override ICollection<T> CreateCollection<T>(params T[] items)
        {
            return new FilterableNotifiableCollection<T>(collection: items)
            {
                ExecutionMode = ExecutionMode.None,
                Filter = item => true
            };
        }

        #endregion
    }


    [TestClass]
    public class FilterableNotifiableCollectionSerializationTest :
        SerializationTestBase<FilterableNotifiableCollection<string>>
    {
        #region Overrides of SerializationTestBase

        protected override FilterableNotifiableCollection<string> GetObject()
        {
            return
                new FilterableNotifiableCollection<string>(new ObservableCollection<string>(TestExtensions.TestStrings))
                {
                    ExecutionMode = ExecutionMode.None
                };
        }

        protected override void AssertObject(FilterableNotifiableCollection<string> deserializedObj)
        {
            deserializedObj.Items.ShouldBeType<ObservableCollection<string>>();
            deserializedObj.SequenceEqual(TestExtensions.TestStrings).ShouldBeTrue();
            deserializedObj.IsNotificationsSuspended.ShouldBeFalse();
            deserializedObj.EventsTracker.ShouldNotBeNull();
        }

        #endregion
    }

    [TestClass]
    public class FilterableNotifiableCollectionSuspendCollectionTest : CollectionTestBase
    {
        #region Overrides of CollectionTestBase

        protected override ICollection<T> CreateCollection<T>(params T[] items)
        {
            return new FilterableNotifiableCollection<T>(collection: items)
            {
                ExecutionMode = ExecutionMode.None,
                Filter = item => true
            };
        }

        #endregion
    }

    [TestClass]
    public class FilterableNotifiableCollectionEmptyFilterCollectionTest : CollectionTestBase
    {
        #region Overrides of CollectionTestBase

        protected override ICollection<T> CreateCollection<T>(params T[] items)
        {
            return new FilterableNotifiableCollection<T>(collection: items)
            {
                ExecutionMode = ExecutionMode.None,
                Filter = item => true
            };
        }

        #endregion
    }
}
