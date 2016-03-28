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
        public void FilterShouldCorrectNotifyAboutChanges()
        {
            ThreadManagerMock.IsUiThread = true;
            var item = new Item();
            var item2 = new Item();
            var collection = (FilterableNotifiableCollection<Item>)CreateNotifiableCollection<Item>(ExecutionMode.None, ThreadManagerMock);
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

            collection.IsClearIgnoreFilter = true;
            collection.Clear();
            collection.Filter = null;
            collection.Count.ShouldEqual(0);
            tracker.AssertChangedEquals();

            collection.Add(item);
            collection.Add(item2);
            collection.Filter = item1 => item == item1;
            tracker.AssertChangedEquals();

            collection.IsClearIgnoreFilter = false;
            collection.Clear();
            collection.Filter = null;
            collection.Count.ShouldEqual(1);
            tracker.AssertChangedEquals();
        }

        [TestMethod]
        public void CollectionShouldTrackChangesCorrectWithFilter()
        {
            const int count = 10;
            var collection = (FilterableNotifiableCollection<Item>)CreateNotifiableCollection<Item>(ExecutionMode.None, ThreadManagerMock);
            collection.Filter = item => !item.Hidden;

            var collectionTracker = new NotifiableCollectionTracker<Item>(collection);
            using (collection.SuspendNotifications())
            {
                var item = new Item();
                var items = new[] { new Item(), new Item(), new Item() };
                var items2 = new[] { new Item { Hidden = true }, new Item(), new Item { Hidden = true } };
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
            collectionTracker.ChangingItems.OrderBy(item => item.Id)
                .SequenceEqual(collection.SourceCollection.OrderBy(item => item.Id))
                .ShouldBeTrue();
            collectionTracker.AssertChangedEquals();
            collection.Count.ShouldEqual(count);
        }

        [TestMethod]
        public void CollectionShouldTrackChangesCorrectInSourceCollection()
        {
            const int count = 10;
            var collection = new ObservableCollection<Item>();
            var filterableCollection = new FilterableNotifiableCollection<Item>(collection, ThreadManagerMock) { Filter = _ => true };
            var collectionTracker = new NotifiableCollectionTracker<Item>(filterableCollection);

            var item = new Item();
            var items = new[] { new Item(), new Item(), new Item() };
            var items2 = new[] { new Item { Hidden = true }, new Item(), new Item { Hidden = true } };
            for (int i = 0; i < count; i++)
            {
                collection.AddRange(items);
                collection.SequenceEqual(filterableCollection).ShouldBeTrue();
                collection.AddRange(items2);
                collection.SequenceEqual(filterableCollection).ShouldBeTrue();
                collection.RemoveRange(items);
                collection.SequenceEqual(filterableCollection).ShouldBeTrue();
            }
            for (int i = 0; i < collection.Count; i++)
            {
                collection[i] = item;
                collection.SequenceEqual(filterableCollection).ShouldBeTrue();
            }

            ThreadManagerMock.InvokeOnUiThreadAsync();
            collectionTracker.AssertChangedEquals();
            collection.Count.ShouldEqual(count * items2.Length);

            collection.Clear();
            ThreadManagerMock.InvokeOnUiThreadAsync();
            collection.SequenceEqual(filterableCollection).ShouldBeTrue();
            collection.Count.ShouldEqual(0);
            collectionTracker.AssertChangedEquals();
        }

        #region Overrides of CollectionTestBase

        protected override SynchronizedNotifiableCollection<T> CreateNotifiableCollection<T>(ExecutionMode executionMode, IThreadManager threadManager)
        {
            return new FilterableNotifiableCollection<T>(threadManager)
            {
                Filter = item => true
            };
        }

        protected override ICollection<T> CreateCollection<T>(params T[] items)
        {
            return new FilterableNotifiableCollection<T>(collection: items)
            {
                Filter = item => true
            };
        }

        #endregion
    }

    [TestClass]
    public class FilterableNotifiableCollectionTestNoFilter : FilterableNotifiableCollectionTest
    {
        #region Overrides of FilterableNotifiableCollectionTestNoFilter

        protected override SynchronizedNotifiableCollection<T> CreateNotifiableCollection<T>(ExecutionMode executionMode, IThreadManager threadManager)
        {
            return new FilterableNotifiableCollection<T>(threadManager);
        }

        protected override ICollection<T> CreateCollection<T>(params T[] items)
        {
            return new FilterableNotifiableCollection<T>(collection: items);
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
            return new FilterableNotifiableCollection<string>(new List<string>(TestExtensions.TestStrings));
        }

        protected override void AssertObject(FilterableNotifiableCollection<string> deserializedObj)
        {
            deserializedObj.Items.ShouldBeType<List<string>>();
            deserializedObj.SequenceEqual(TestExtensions.TestStrings).ShouldBeTrue();
            deserializedObj.IsNotificationsSuspended.ShouldBeFalse();
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
                Filter = item => true
            };
        }

        #endregion
    }
}
