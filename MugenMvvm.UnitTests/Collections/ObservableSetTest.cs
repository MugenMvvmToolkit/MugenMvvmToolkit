using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Tests.Collections;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Collections
{
    public class ObservableSetTest : ObservableCollectionTestBase, IEqualityComparer<TestCollectionItem?>
    {
        [Fact]
        public void AddOrUpdateShouldCheckForEquality()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem {Id = item1.Id};
            var set = new ObservableSet<TestCollectionItem>(this);
            set.AddOrUpdate(item1).ShouldBeTrue();
            set.Single().ShouldEqual(item1);
            set.AddOrUpdate(item2).ShouldBeTrue();
            set.Single().ShouldEqual(item2);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void AddOrUpdateShouldNotifyListeners(int listenersCount, int count)
        {
            var adding = 0;
            var added = 0;
            var condition = 0;
            var expectedIndex = 0;
            var canAdd = false;
            TestCollectionItem? expectedItem = null;
            var collection = new ObservableSet<TestCollectionItem>(this);

            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestConditionCollectionComponent<TestCollectionItem>
                {
                    CanAdd = (c, item, index) =>
                    {
                        c.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++condition;
                        return canAdd;
                    },
                    Priority = -i
                });

                collection.AddComponent(new TestCollectionChangingListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnAdding = (c, item, index) =>
                    {
                        c.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++adding;
                    }
                });

                collection.AddComponent(new TestCollectionChangedListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnAdded = (c, item, index) =>
                    {
                        c.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++added;
                    }
                });
            }

            for (var i = 0; i < count; i++)
            {
                expectedItem = new TestCollectionItem();
                expectedIndex = 0;
                collection.Add(expectedItem).ShouldBeFalse();
            }

            condition.ShouldEqual(count);
            adding.ShouldEqual(0);
            added.ShouldEqual(0);

            condition = 0;
            canAdd = true;
            for (var i = 0; i < count; i++)
            {
                expectedItem = new TestCollectionItem();
                expectedIndex = i;
                collection.Add(expectedItem).ShouldBeTrue();
            }

            condition.ShouldEqual(count * listenersCount);
            adding.ShouldEqual(count * listenersCount);
            added.ShouldEqual(count * listenersCount);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void AddOrUpdateShouldNotifyListenersReplace(int listenersCount, int count)
        {
            var replacing = 0;
            var replaced = 0;
            var replaceCondition = 0;
            var expectedIndex = 0;
            var canReplace = false;
            TestCollectionItem? expectedOldItem = null, expectedNewItem = null;
            var collection = new ObservableSet<TestCollectionItem>(this);
            for (var i = 0; i < count; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestConditionCollectionComponent<TestCollectionItem>
                {
                    CanReplace = (c, oldItem, newItem, index) =>
                    {
                        c.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        oldItem.ShouldEqual(expectedOldItem);
                        newItem.ShouldEqual(expectedNewItem);
                        ++replaceCondition;
                        return canReplace;
                    },
                    Priority = -i
                });

                collection.AddComponent(new TestCollectionChangingListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnReplacing = (c, oldItem, newItem, index) =>
                    {
                        c.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        oldItem.ShouldEqual(expectedOldItem);
                        newItem.ShouldEqual(expectedNewItem);
                        ++replacing;
                    }
                });

                collection.AddComponent(new TestCollectionChangedListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnReplaced = (c, oldItem, newItem, index) =>
                    {
                        c.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        oldItem.ShouldEqual(expectedOldItem);
                        newItem.ShouldEqual(expectedNewItem);
                        ++replaced;
                    }
                });
            }

            for (var i = 0; i < count; i++)
            {
                expectedOldItem = collection.ElementAt(i);
                expectedNewItem = new TestCollectionItem {Id = expectedOldItem.Id};
                expectedIndex = i;
                collection.AddOrUpdate(expectedNewItem).ShouldBeFalse();
            }

            replaceCondition.ShouldEqual(count);
            replacing.ShouldEqual(0);
            replaced.ShouldEqual(0);

            replaceCondition = 0;
            canReplace = true;
            for (var i = 0; i < count; i++)
            {
                expectedOldItem = collection.ElementAt(i);
                expectedNewItem = new TestCollectionItem {Id = expectedOldItem.Id};
                expectedIndex = i;
                collection.AddOrUpdate(expectedNewItem).ShouldBeTrue();
            }

            replaceCondition.ShouldEqual(count * listenersCount);
            replacing.ShouldEqual(count * listenersCount);
            replaced.ShouldEqual(count * listenersCount);
        }

        [Fact]
        public void AddShouldCheckForEquality()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem {Id = item1.Id};
            var set = new ObservableSet<TestCollectionItem>(this);
            set.Add(item1).ShouldBeTrue();
            set.Add(item2).ShouldBeFalse();

            set.Contains(item1).ShouldBeTrue();
            set.Contains(item2).ShouldBeTrue();

            set.Remove(item2).ShouldBeTrue();
            set.Remove(item1).ShouldBeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void ValueEnumeratorTest(int size)
        {
            var items = new List<TestCollectionItem>();
            var set = CreateSet<TestCollectionItem>();
            for (var i = 0; i < size; i++)
            {
                items.Add(new TestCollectionItem());
                set.Add(items[i]);
            }

            var index = 0;
            foreach (var item in set)
                set.ElementAt(index++).ShouldEqual(item);
        }

#pragma warning disable CS8714
        protected override IObservableCollection<T> CreateCollection<T>(params T[] items) => CreateSet(items);
#pragma warning restore CS8714

        private ObservableSet<T> CreateSet<T>(params T[] items) where T : notnull => new(items, null, ComponentCollectionManager);

        public bool Equals(TestCollectionItem? x, TestCollectionItem? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(TestCollectionItem? obj) => obj!.Id;
    }
}