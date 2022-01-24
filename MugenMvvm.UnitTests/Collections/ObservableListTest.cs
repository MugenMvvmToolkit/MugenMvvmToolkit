using System.Collections;
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
    public class ObservableListTest : ObservableCollectionTestBase
    {
        [Fact]
        public void GetSetTest()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();
            var collection = CreateList<TestCollectionItem>();
            collection.Add(item1);

            collection[0].ShouldEqual(item1);
            collection[0] = item2;
            collection[0].ShouldEqual(item2);
        }

        [Fact]
        public void IListAddTest()
        {
            var item = new TestCollectionItem();
            var collection = (IList) CreateCollection<TestCollectionItem>();
            collection.Add(item);
            collection.Count.ShouldEqual(1);
            collection.OfType<TestCollectionItem>().Any(item1 => item1 == item).ShouldBeTrue();
        }

        [Fact]
        public void IListClearItemsTest()
        {
            var items = new[] {new TestCollectionItem(), new TestCollectionItem()};
            var collection = (IList) CreateCollection(items);
            collection.Count.ShouldEqual(2);
            collection.OfType<TestCollectionItem>().Any(item => item == items[0]).ShouldBeTrue();
            collection.OfType<TestCollectionItem>().Any(item => item == items[1]).ShouldBeTrue();

            collection.Clear();
            if (collection.Count != 0)
                collection.Count.ShouldEqual(0);
        }

        [Fact]
        public void IListContainsTest()
        {
            var item = new TestCollectionItem();
            var collection = (IList) CreateCollection<TestCollectionItem>();
            collection.Add(item);
            collection.Count.ShouldEqual(1);
            collection.Contains(item).ShouldBeTrue();
        }

        [Fact]
        public void IListGetSetTest()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();
            var collection = (IList) CreateCollection<TestCollectionItem>();
            collection.Add(item1);

            collection[0].ShouldEqual(item1);
            collection[0] = item2;
            collection[0].ShouldEqual(item2);
        }

        [Fact]
        public void IListIndexOfTest()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();
            var collection = (IList) CreateCollection<TestCollectionItem>();

            collection.IndexOf(item1).ShouldBeLessThan(0);

            collection.Insert(0, item1);
            collection.IndexOf(item1).ShouldEqual(0);

            collection.Insert(0, item2);
            collection.IndexOf(item2).ShouldEqual(0);
            collection.IndexOf(item1).ShouldEqual(1);
        }

        [Fact]
        public void IListInsertTest()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();
            var collection = (IList) CreateCollection<TestCollectionItem>();
            collection.Insert(0, item1);
            collection.Count.ShouldEqual(1);

            collection.Insert(0, item2);
            collection.Count.ShouldEqual(2);

            collection[0].ShouldEqual(item2);
            collection[1].ShouldEqual(item1);
        }

        [Fact]
        public void IListRemoveAtTest()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();
            var collection = (IList) CreateCollection<TestCollectionItem>();
            collection.Insert(0, item1);
            collection.Insert(0, item2);
            collection.Count.ShouldEqual(2);

            collection.RemoveAt(1);
            collection.Count.ShouldEqual(1);
            collection[0].ShouldEqual(item2);

            collection.RemoveAt(0);
            collection.Count.ShouldEqual(0);
        }

        [Fact]
        public void IListRemoveTest()
        {
            var item = new TestCollectionItem();
            var collection = (IList) CreateList<TestCollectionItem>();
            collection.Add(item);
            collection.Count.ShouldEqual(1);
            collection.OfType<TestCollectionItem>().Any(item1 => item1 == item).ShouldBeTrue();

            collection.Remove(item);
            collection.Count.ShouldEqual(0);
            collection.OfType<TestCollectionItem>().Any(item1 => item1 == item).ShouldBeFalse();
        }

        [Fact]
        public void IndexOfTest()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();
            var collection = CreateList<TestCollectionItem>();

            collection.IndexOf(item1).ShouldBeLessThan(0);

            collection.Insert(0, item1);
            collection.IndexOf(item1).ShouldEqual(0);

            collection.Insert(0, item2);
            collection.IndexOf(item2).ShouldEqual(0);
            collection.IndexOf(item1).ShouldEqual(1);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void InsertShouldNotifyListeners(int listenersCount, int count)
        {
            var adding = 0;
            var added = 0;
            var condition = 0;
            var expectedIndex = 0;
            var canAdd = false;
            TestCollectionItem? expectedItem = null;
            var collection = CreateList<TestCollectionItem>();

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
                collection.Insert(0, expectedItem);
            }

            condition.ShouldEqual(count);
            adding.ShouldEqual(0);
            added.ShouldEqual(0);

            condition = 0;
            canAdd = true;
            for (var i = 0; i < count; i++)
            {
                expectedItem = new TestCollectionItem();
                expectedIndex = 0;
                collection.Insert(0, expectedItem);
            }

            condition.ShouldEqual(count * listenersCount);
            adding.ShouldEqual(count * listenersCount);
            added.ShouldEqual(count * listenersCount);
        }

        [Fact]
        public void InsertTest()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();
            var collection = CreateList<TestCollectionItem>();
            collection.Insert(0, item1);
            collection.Count.ShouldEqual(1);

            collection.Insert(0, item2);
            collection.Count.ShouldEqual(2);

            collection[0].ShouldEqual(item2);
            collection[1].ShouldEqual(item1);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void MoveShouldNotifyListeners(int listenersCount, int count)
        {
            var moving = 0;
            var moved = 0;
            var condition = 0;
            var expectedOldIndex = 0;
            var expectedNewIndex = 0;
            var canMove = false;
            TestCollectionItem? expectedItem = null;
            var collection = CreateList<TestCollectionItem>();
            for (var i = 0; i < count + 1; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestConditionCollectionComponent<TestCollectionItem>
                {
                    CanMove = (c, item, oldIndex, newIndex) =>
                    {
                        c.ShouldEqual(collection);
                        oldIndex.ShouldEqual(expectedOldIndex);
                        newIndex.ShouldEqual(expectedNewIndex);
                        expectedItem.ShouldEqual(item);
                        ++condition;
                        return canMove;
                    },
                    Priority = -i
                });

                collection.AddComponent(new TestCollectionChangingListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnMoving = (c, item, oldIndex, newIndex) =>
                    {
                        c.ShouldEqual(collection);
                        oldIndex.ShouldEqual(expectedOldIndex);
                        newIndex.ShouldEqual(expectedNewIndex);
                        expectedItem.ShouldEqual(item);
                        ++moving;
                    }
                });

                collection.AddComponent(new TestCollectionChangedListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnMoved = (c, item, oldIndex, newIndex) =>
                    {
                        c.ShouldEqual(collection);
                        oldIndex.ShouldEqual(expectedOldIndex);
                        newIndex.ShouldEqual(expectedNewIndex);
                        expectedItem.ShouldEqual(item);
                        ++moved;
                    }
                });
            }

            for (var i = 0; i < count; i++)
            {
                expectedOldIndex = i;
                expectedNewIndex = i + 1;
                expectedItem = collection[expectedOldIndex];
                collection.Move(expectedOldIndex, expectedNewIndex);
            }

            condition.ShouldEqual(count);
            moving.ShouldEqual(0);
            moved.ShouldEqual(0);

            condition = 0;
            canMove = true;
            for (var i = 0; i < count; i++)
            {
                expectedOldIndex = i;
                expectedNewIndex = i + 1;
                expectedItem = collection[expectedOldIndex];
                collection.Move(expectedOldIndex, expectedNewIndex);
            }

            condition.ShouldEqual(count * listenersCount);
            moving.ShouldEqual(count * listenersCount);
            moved.ShouldEqual(count * listenersCount);
        }

        [Fact]
        public void MoveTest()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();
            var collection = CreateList(item1, item2);

            collection[0].ShouldEqual(item1);
            collection[1].ShouldEqual(item2);

            collection.Move(0, 1);

            collection[0].ShouldEqual(item2);
            collection[1].ShouldEqual(item1);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void RemoveAtShouldNotifyListeners(int listenersCount, int count)
        {
            var removing = 0;
            var removed = 0;
            var condition = 0;
            var expectedIndex = 0;
            var canRemove = false;
            TestCollectionItem? expectedItem = null;
            var collection = CreateList<TestCollectionItem>();
            for (var i = 0; i < count; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestConditionCollectionComponent<TestCollectionItem>
                {
                    CanRemove = (c, item, index) =>
                    {
                        c.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++condition;
                        return canRemove;
                    },
                    Priority = -i
                });

                collection.AddComponent(new TestCollectionChangingListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnRemoving = (c, item, index) =>
                    {
                        c.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++removing;
                    }
                });

                collection.AddComponent(new TestCollectionChangedListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnRemoved = (c, item, index) =>
                    {
                        c.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++removed;
                    }
                });
            }

            for (var i = 0; i < count; i++)
            {
                expectedItem = collection[0];
                expectedIndex = 0;
                collection.RemoveAt(expectedIndex);
            }

            condition.ShouldEqual(count);
            removing.ShouldEqual(0);
            removed.ShouldEqual(0);

            condition = 0;
            canRemove = true;
            for (var i = 0; i < count; i++)
            {
                expectedItem = collection[0];
                expectedIndex = 0;
                collection.RemoveAt(expectedIndex);
            }

            condition.ShouldEqual(count * listenersCount);
            removing.ShouldEqual(count * listenersCount);
            removed.ShouldEqual(count * listenersCount);
        }

        [Fact]
        public void RemoveAtTest()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();
            var collection = CreateList<TestCollectionItem>();
            collection.Insert(0, item1);
            collection.Insert(0, item2);
            collection.Count.ShouldEqual(2);

            collection.RemoveAt(1);
            collection.Count.ShouldEqual(1);
            collection[0].ShouldEqual(item2);

            collection.RemoveAt(0);
            collection.Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void ReplaceShouldNotifyListeners(int listenersCount, int count)
        {
            var replacing = 0;
            var replaced = 0;
            var condition = 0;
            var expectedIndex = 0;
            var canReplace = false;
            TestCollectionItem? expectedOldItem = null, expectedNewItem = null;
            var collection = CreateList<TestCollectionItem>();
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
                        ++condition;
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
                expectedNewItem = new TestCollectionItem();
                expectedOldItem = collection[i];
                expectedIndex = i;
                collection[i] = expectedNewItem;
            }

            condition.ShouldEqual(count);
            replacing.ShouldEqual(0);
            replaced.ShouldEqual(0);

            condition = 0;
            canReplace = true;
            for (var i = 0; i < count; i++)
            {
                expectedNewItem = new TestCollectionItem();
                expectedOldItem = collection[i];
                expectedIndex = i;
                collection[i] = expectedNewItem;
            }

            condition.ShouldEqual(count * listenersCount);
            replacing.ShouldEqual(count * listenersCount);
            replaced.ShouldEqual(count * listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void ValueEnumeratorTest(int size)
        {
            var items = new List<TestCollectionItem>();
            var collection = new ObservableList<TestCollectionItem>(items, ComponentCollectionManager);
            for (var i = 0; i < size; i++)
            {
                items.Add(new TestCollectionItem());
                collection.Add(items[i]);
            }

            var index = 0;
            foreach (var item in collection)
                collection[index++].ShouldEqual(item);
        }

        protected virtual IObservableList<T> CreateList<T>(params T[] items) => new ObservableList<T>(items, ComponentCollectionManager);

        protected override IObservableCollection<T> CreateCollection<T>(params T[] items) => CreateList(items);
    }
}