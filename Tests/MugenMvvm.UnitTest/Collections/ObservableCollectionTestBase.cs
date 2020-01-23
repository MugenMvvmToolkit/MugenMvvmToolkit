using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Models;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Collections
{
    public class ObservableCollectionTestBase : ComponentOwnerTestBase<IObservableCollection<object>>
    {
        #region Fields

        protected const int TestIterationCount = 50;

        #endregion

        #region Methods

        [Fact]
        public void EnumeratorTest()
        {
            var items = new List<CollectionItem>();
            var collection = CreateCollection<CollectionItem>();
            for (var i = 0; i < 1000; i++)
            {
                items.Add(new CollectionItem());
                collection.Add(items[i]);
            }

            collection.SequenceEqual(items).ShouldBeTrue();
        }

        [Fact]
        public void IEnumerableEnumeratorTest()
        {
            var items = new List<CollectionItem>();
            var collection = CreateCollection<CollectionItem>();
            for (var i = 0; i < 1000; i++)
            {
                items.Add(new CollectionItem());
                collection.Add(items[i]);
            }

            collection.OfType<CollectionItem>().SequenceEqual(items).ShouldBeTrue();
        }

        [Fact]
        public void CreateWithItemsTest()
        {
            var items = new[] { new CollectionItem(), new CollectionItem() };
            var collection = CreateCollection(items);
            collection.Count.ShouldEqual(2);
            collection.Any(item => item == items[0]).ShouldBeTrue();
            collection.Any(item => item == items[1]).ShouldBeTrue();
        }

        [Fact]
        public void AddTest()
        {
            var item = new CollectionItem();
            var collection = CreateCollection<CollectionItem>();
            collection.Add(item);
            collection.Count.ShouldEqual(1);
            collection.Any(item1 => item1 == item).ShouldBeTrue();
        }

        [Fact]
        public void IListAddTest()
        {
            var item = new CollectionItem();
            var collection = (IList)CreateCollection<CollectionItem>();
            collection.Add(item);
            collection.Count.ShouldEqual(1);
            collection.OfType<CollectionItem>().Any(item1 => item1 == item).ShouldBeTrue();
        }

        [Fact]
        public void InsertTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();
            var collection = CreateCollection<CollectionItem>();
            collection.Insert(0, item1);
            collection.Count.ShouldEqual(1);

            collection.Insert(0, item2);
            collection.Count.ShouldEqual(2);

            collection[0].ShouldEqual(item2);
            collection[1].ShouldEqual(item1);
        }

        [Fact]
        public void IListInsertTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();
            var collection = (IList)CreateCollection<CollectionItem>();
            collection.Insert(0, item1);
            collection.Count.ShouldEqual(1);

            collection.Insert(0, item2);
            collection.Count.ShouldEqual(2);

            collection[0].ShouldEqual(item2);
            collection[1].ShouldEqual(item1);
        }

        [Fact]
        public void GetSetTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();
            var collection = CreateCollection<CollectionItem>();
            collection.Add(item1);

            collection[0].ShouldEqual(item1);
            collection[0] = item2;
            collection[0].ShouldEqual(item2);
        }

        [Fact]
        public void IListGetSetTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();
            var collection = (IList)CreateCollection<CollectionItem>();
            collection.Add(item1);

            collection[0].ShouldEqual(item1);
            collection[0] = item2;
            collection[0].ShouldEqual(item2);
        }

        [Fact]
        public void MoveTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();
            var collection = CreateCollection(item1, item2);

            collection[0].ShouldEqual(item1);
            collection[1].ShouldEqual(item2);

            collection.Move(0, 1);

            collection[0].ShouldEqual(item2);
            collection[1].ShouldEqual(item1);
        }

        [Fact]
        public void ResetTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();
            var collection = CreateCollection(new CollectionItem());

            collection.Reset(new[] { item1, item2 });
            collection[0].ShouldEqual(item1);
            collection[1].ShouldEqual(item2);
            collection.Count.ShouldEqual(2);
        }

        [Fact]
        public void IndexOfTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();
            var collection = CreateCollection<CollectionItem>();

            collection.IndexOf(item1).ShouldBeLessThan(0);

            collection.Insert(0, item1);
            collection.IndexOf(item1).ShouldEqual(0);

            collection.Insert(0, item2);
            collection.IndexOf(item2).ShouldEqual(0);
            collection.IndexOf(item1).ShouldEqual(1);
        }

        [Fact]
        public void IListIndexOfTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();
            var collection = (IList)CreateCollection<CollectionItem>();

            collection.IndexOf(item1).ShouldBeLessThan(0);

            collection.Insert(0, item1);
            collection.IndexOf(item1).ShouldEqual(0);

            collection.Insert(0, item2);
            collection.IndexOf(item2).ShouldEqual(0);
            collection.IndexOf(item1).ShouldEqual(1);
        }

        [Fact]
        public void RemoveAtTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();
            var collection = CreateCollection<CollectionItem>();
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
        public void IListRemoveAtTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();
            var collection = (IList)CreateCollection<CollectionItem>();
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
        public void RemoveTest()
        {
            var item = new CollectionItem();
            var collection = CreateCollection<CollectionItem>();
            collection.Add(item);
            collection.Count.ShouldEqual(1);
            collection.Any(item1 => item1 == item).ShouldBeTrue();

            collection.Remove(item);
            collection.Count.ShouldEqual(0);
            collection.Any(item1 => item1 == item).ShouldBeFalse();
        }

        [Fact]
        public void IListRemoveTest()
        {
            var item = new CollectionItem();
            var collection = (IList)CreateCollection<CollectionItem>();
            collection.Add(item);
            collection.Count.ShouldEqual(1);
            collection.OfType<CollectionItem>().Any(item1 => item1 == item).ShouldBeTrue();

            collection.Remove(item);
            collection.Count.ShouldEqual(0);
            collection.OfType<CollectionItem>().Any(item1 => item1 == item).ShouldBeFalse();
        }

        [Fact]
        public void ContainsTest()
        {
            var item = new CollectionItem();
            var collection = CreateCollection<CollectionItem>();
            collection.Add(item);
            collection.Count.ShouldEqual(1);
            collection.Contains(item).ShouldBeTrue();
        }

        [Fact]
        public void IListContainsTest()
        {
            var item = new CollectionItem();
            var collection = (IList)CreateCollection<CollectionItem>();
            collection.Add(item);
            collection.Count.ShouldEqual(1);
            collection.Contains(item).ShouldBeTrue();
        }

        [Fact]
        public void CopyToTest()
        {
            var item = new CollectionItem();
            var collection = CreateCollection<CollectionItem>();
            collection.Add(item);

            var items = new CollectionItem[1];
            collection.CopyTo(items, 0);
            items[0].ShouldEqual(item);
        }

        [Fact]
        public void ICollectionCopyToTest()
        {
            var item = new CollectionItem();
            var collection = (IList)CreateCollection<CollectionItem>();
            collection.Add(item);

            var items = new CollectionItem[1];
            collection.CopyTo(items, 0);
            items[0].ShouldEqual(item);
        }

        [Fact]
        public void ClearItemsTest()
        {
            var items = new[] { new CollectionItem(), new CollectionItem() };
            var collection = CreateCollection(items);
            collection.Count.ShouldEqual(2);
            collection.Any(item => item == items[0]).ShouldBeTrue();
            collection.Any(item => item == items[1]).ShouldBeTrue();

            collection.Clear();
            if (collection.Count != 0)
                collection.Count.ShouldEqual(0);
        }

        [Fact]
        public void IListClearItemsTest()
        {
            var items = new[] { new CollectionItem(), new CollectionItem() };
            var collection = (IList)CreateCollection(items);
            collection.Count.ShouldEqual(2);
            collection.OfType<CollectionItem>().Any(item => item == items[0]).ShouldBeTrue();
            collection.OfType<CollectionItem>().Any(item => item == items[1]).ShouldBeTrue();

            collection.Clear();
            if (collection.Count != 0)
                collection.Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldBeginEndBatchUpdateNotifyListeners(int listenersCount)
        {
            var begin = 0;
            var end = 0;
            var collection = CreateCollection<CollectionItem>();

            for (var i = 0; i < listenersCount; i++)
            {
                var collectionListener = new TestObservableCollectionBatchUpdateListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnBeginBatchUpdate = items => begin++,
                    OnEndBatchUpdate = items => end++
                };

                collection.AddComponent(collectionListener);
            }

            var beginBatchUpdate1 = collection.BeginBatchUpdate();
            begin.ShouldEqual(listenersCount);
            end.ShouldEqual(0);

            var beginBatchUpdate2 = collection.BeginBatchUpdate();
            begin.ShouldEqual(listenersCount);
            end.ShouldEqual(0);

            beginBatchUpdate1.Dispose();
            begin.ShouldEqual(listenersCount);
            end.ShouldEqual(0);

            beginBatchUpdate2.Dispose();
            begin.ShouldEqual(listenersCount);
            end.ShouldEqual(listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldAddNotifyListeners(int listenersCount)
        {
            var adding = 0;
            var added = 0;
            var condition = 0;
            var expectedIndex = 0;
            var canAdd = false;
            CollectionItem? expectedItem = null;
            var collection = CreateCollection<CollectionItem>();

            for (var i = 0; i < listenersCount; i++)
            {
                var conditionListener = new TestConditionObservableCollectionComponent<CollectionItem>(collection)
                {
                    CanAdd = (items, item, index) =>
                    {
                        items.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++condition;
                        return canAdd;
                    },
                    Priority = -i
                };
                collection.AddComponent(conditionListener);

                var changingListener = new TestObservableCollectionChangingListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnAdding = (items, item, index) =>
                    {
                        items.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++adding;
                    }
                };
                collection.AddComponent(changingListener);

                var changedListener = new TestObservableCollectionChangedListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnAdded = (items, item, index) =>
                    {
                        items.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++added;
                    }
                };
                collection.AddComponent(changedListener);
            }

            for (var i = 0; i < TestIterationCount; i++)
            {
                expectedItem = new CollectionItem();
                expectedIndex = 0;
                collection.Add(expectedItem);
            }

            condition.ShouldEqual(TestIterationCount);
            adding.ShouldEqual(0);
            added.ShouldEqual(0);

            condition = 0;
            canAdd = true;
            for (var i = 0; i < TestIterationCount; i++)
            {
                expectedItem = new CollectionItem();
                expectedIndex = i;
                collection.Add(expectedItem);
            }

            condition.ShouldEqual(TestIterationCount * listenersCount);
            adding.ShouldEqual(TestIterationCount * listenersCount);
            added.ShouldEqual(TestIterationCount * listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldInsertNotifyListeners(int listenersCount)
        {
            var adding = 0;
            var added = 0;
            var condition = 0;
            var expectedIndex = 0;
            var canAdd = false;
            CollectionItem? expectedItem = null;
            var collection = CreateCollection<CollectionItem>();

            for (var i = 0; i < listenersCount; i++)
            {
                var conditionListener = new TestConditionObservableCollectionComponent<CollectionItem>(collection)
                {
                    CanAdd = (items, item, index) =>
                    {
                        items.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++condition;
                        return canAdd;
                    },
                    Priority = -i
                };
                collection.AddComponent(conditionListener);

                var changingListener = new TestObservableCollectionChangingListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnAdding = (items, item, index) =>
                    {
                        items.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++adding;
                    }
                };
                collection.AddComponent(changingListener);

                var changedListener = new TestObservableCollectionChangedListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnAdded = (items, item, index) =>
                    {
                        items.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++added;
                    }
                };
                collection.AddComponent(changedListener);
            }

            for (var i = 0; i < TestIterationCount; i++)
            {
                expectedItem = new CollectionItem();
                expectedIndex = 0;
                collection.Insert(0, expectedItem);
            }

            condition.ShouldEqual(TestIterationCount);
            adding.ShouldEqual(0);
            added.ShouldEqual(0);

            condition = 0;
            canAdd = true;
            for (var i = 0; i < TestIterationCount; i++)
            {
                expectedItem = new CollectionItem();
                expectedIndex = 0;
                collection.Insert(0, expectedItem);
            }

            condition.ShouldEqual(TestIterationCount * listenersCount);
            adding.ShouldEqual(TestIterationCount * listenersCount);
            added.ShouldEqual(TestIterationCount * listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldReplaceNotifyListeners(int listenersCount)
        {
            var replacing = 0;
            var replaced = 0;
            var condition = 0;
            var expectedIndex = 0;
            var canReplace = false;
            CollectionItem? expectedOldItem = null, expectedNewItem = null;
            var collection = CreateCollection<CollectionItem>();
            for (var i = 0; i < TestIterationCount; i++)
                collection.Add(new CollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var conditionListener = new TestConditionObservableCollectionComponent<CollectionItem>(collection)
                {
                    CanReplace = (items, oldItem, newItem, index) =>
                    {
                        items.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        oldItem.ShouldEqual(expectedOldItem);
                        newItem.ShouldEqual(expectedNewItem);
                        ++condition;
                        return canReplace;
                    },
                    Priority = -i
                };
                collection.AddComponent(conditionListener);

                var changingListener = new TestObservableCollectionChangingListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnReplacing = (items, oldItem, newItem, index) =>
                    {
                        items.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        oldItem.ShouldEqual(expectedOldItem);
                        newItem.ShouldEqual(expectedNewItem);
                        ++replacing;
                    }
                };
                collection.AddComponent(changingListener);

                var changedListener = new TestObservableCollectionChangedListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnReplaced = (items, oldItem, newItem, index) =>
                    {
                        items.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        oldItem.ShouldEqual(expectedOldItem);
                        newItem.ShouldEqual(expectedNewItem);
                        ++replaced;
                    }
                };
                collection.AddComponent(changedListener);
            }

            for (var i = 0; i < TestIterationCount; i++)
            {
                expectedNewItem = new CollectionItem();
                expectedOldItem = collection[i];
                expectedIndex = i;
                collection[i] = expectedNewItem;
            }

            condition.ShouldEqual(TestIterationCount);
            replacing.ShouldEqual(0);
            replaced.ShouldEqual(0);

            condition = 0;
            canReplace = true;
            for (var i = 0; i < TestIterationCount; i++)
            {
                expectedNewItem = new CollectionItem();
                expectedOldItem = collection[i];
                expectedIndex = i;
                collection[i] = expectedNewItem;
            }

            condition.ShouldEqual(TestIterationCount * listenersCount);
            replacing.ShouldEqual(TestIterationCount * listenersCount);
            replaced.ShouldEqual(TestIterationCount * listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldMoveNotifyListeners(int listenersCount)
        {
            var moving = 0;
            var moved = 0;
            var condition = 0;
            var expectedOldIndex = 0;
            var expectedNewIndex = 0;
            var canMove = false;
            CollectionItem? expectedItem = null;
            var collection = CreateCollection<CollectionItem>();
            for (var i = 0; i < TestIterationCount + 1; i++)
                collection.Add(new CollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var conditionListener = new TestConditionObservableCollectionComponent<CollectionItem>(collection)
                {
                    CanMove = (items, item, oldIndex, newIndex) =>
                    {
                        items.ShouldEqual(collection);
                        oldIndex.ShouldEqual(expectedOldIndex);
                        newIndex.ShouldEqual(expectedNewIndex);
                        expectedItem.ShouldEqual(item);
                        ++condition;
                        return canMove;
                    },
                    Priority = -i
                };
                collection.AddComponent(conditionListener);

                var changingListener = new TestObservableCollectionChangingListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnMoving = (items, item, oldIndex, newIndex) =>
                    {
                        items.ShouldEqual(collection);
                        oldIndex.ShouldEqual(expectedOldIndex);
                        newIndex.ShouldEqual(expectedNewIndex);
                        expectedItem.ShouldEqual(item);
                        ++moving;
                    }
                };
                collection.AddComponent(changingListener);

                var changedListener = new TestObservableCollectionChangedListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnMoved = (items, item, oldIndex, newIndex) =>
                    {
                        items.ShouldEqual(collection);
                        oldIndex.ShouldEqual(expectedOldIndex);
                        newIndex.ShouldEqual(expectedNewIndex);
                        expectedItem.ShouldEqual(item);
                        ++moved;
                    }
                };
                collection.AddComponent(changedListener);
            }

            for (var i = 0; i < TestIterationCount; i++)
            {
                expectedOldIndex = i;
                expectedNewIndex = i + 1;
                expectedItem = collection[expectedOldIndex];
                collection.Move(expectedOldIndex, expectedNewIndex);
            }

            condition.ShouldEqual(TestIterationCount);
            moving.ShouldEqual(0);
            moved.ShouldEqual(0);

            condition = 0;
            canMove = true;
            for (var i = 0; i < TestIterationCount; i++)
            {
                expectedOldIndex = i;
                expectedNewIndex = i + 1;
                expectedItem = collection[expectedOldIndex];
                collection.Move(expectedOldIndex, expectedNewIndex);
            }

            condition.ShouldEqual(TestIterationCount * listenersCount);
            moving.ShouldEqual(TestIterationCount * listenersCount);
            moved.ShouldEqual(TestIterationCount * listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldRemoveNotifyListeners(int listenersCount)
        {
            var removing = 0;
            var removed = 0;
            var condition = 0;
            var expectedIndex = 0;
            var canRemove = false;
            CollectionItem? expectedItem = null;
            var collection = CreateCollection<CollectionItem>();
            for (var i = 0; i < TestIterationCount; i++)
                collection.Add(new CollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var conditionListener = new TestConditionObservableCollectionComponent<CollectionItem>(collection)
                {
                    CanRemove = (items, item, index) =>
                    {
                        items.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++condition;
                        return canRemove;
                    },
                    Priority = -i
                };
                collection.AddComponent(conditionListener);

                var changingListener = new TestObservableCollectionChangingListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnRemoving = (items, item, index) =>
                    {
                        items.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++removing;
                    }
                };
                collection.AddComponent(changingListener);

                var changedListener = new TestObservableCollectionChangedListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnRemoved = (items, item, index) =>
                    {
                        items.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++removed;
                    }
                };
                collection.AddComponent(changedListener);
            }

            for (var i = 0; i < TestIterationCount; i++)
            {
                expectedItem = collection[0];
                expectedIndex = 0;
                collection.Remove(expectedItem);
            }

            condition.ShouldEqual(TestIterationCount);
            removing.ShouldEqual(0);
            removed.ShouldEqual(0);

            condition = 0;
            canRemove = true;
            for (var i = 0; i < TestIterationCount; i++)
            {
                expectedItem = collection[0];
                expectedIndex = 0;
                collection.Remove(expectedItem);
            }

            condition.ShouldEqual(TestIterationCount * listenersCount);
            removing.ShouldEqual(TestIterationCount * listenersCount);
            removed.ShouldEqual(TestIterationCount * listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldRemoveAtNotifyListeners(int listenersCount)
        {
            var removing = 0;
            var removed = 0;
            var condition = 0;
            var expectedIndex = 0;
            var canRemove = false;
            CollectionItem? expectedItem = null;
            var collection = CreateCollection<CollectionItem>();
            for (var i = 0; i < TestIterationCount; i++)
                collection.Add(new CollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var conditionListener = new TestConditionObservableCollectionComponent<CollectionItem>(collection)
                {
                    CanRemove = (items, item, index) =>
                    {
                        items.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++condition;
                        return canRemove;
                    },
                    Priority = -i
                };
                collection.AddComponent(conditionListener);

                var changingListener = new TestObservableCollectionChangingListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnRemoving = (items, item, index) =>
                    {
                        items.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++removing;
                    }
                };
                collection.AddComponent(changingListener);

                var changedListener = new TestObservableCollectionChangedListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnRemoved = (items, item, index) =>
                    {
                        items.ShouldEqual(collection);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++removed;
                    }
                };
                collection.AddComponent(changedListener);
            }

            for (var i = 0; i < TestIterationCount; i++)
            {
                expectedItem = collection[0];
                expectedIndex = 0;
                collection.RemoveAt(expectedIndex);
            }

            condition.ShouldEqual(TestIterationCount);
            removing.ShouldEqual(0);
            removed.ShouldEqual(0);

            condition = 0;
            canRemove = true;
            for (var i = 0; i < TestIterationCount; i++)
            {
                expectedItem = collection[0];
                expectedIndex = 0;
                collection.RemoveAt(expectedIndex);
            }

            condition.ShouldEqual(TestIterationCount * listenersCount);
            removing.ShouldEqual(TestIterationCount * listenersCount);
            removed.ShouldEqual(TestIterationCount * listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldResetNotifyListeners(int listenersCount)
        {
            var resetting = 0;
            var reset = 0;
            var condition = 0;
            var canReset = false;
            IEnumerable<CollectionItem>? expectedItem = null;
            var collection = CreateCollection<CollectionItem>();
            for (var i = 0; i < TestIterationCount; i++)
                collection.Add(new CollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var conditionListener = new TestConditionObservableCollectionComponent<CollectionItem>(collection)
                {
                    CanReset = (items, enumerable) =>
                    {
                        items.ShouldEqual(collection);
                        expectedItem.SequenceEqual(enumerable).ShouldBeTrue();
                        ++condition;
                        return canReset;
                    },
                    Priority = -i
                };
                collection.AddComponent(conditionListener);

                var changingListener = new TestObservableCollectionChangingListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnResetting = (items, enumerable) =>
                    {
                        items.ShouldEqual(collection);
                        expectedItem.SequenceEqual(enumerable).ShouldBeTrue();
                        ++resetting;
                    }
                };
                collection.AddComponent(changingListener);

                var changedListener = new TestObservableCollectionChangedListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnReset = (items, enumerable) =>
                    {
                        items.ShouldEqual(collection);
                        expectedItem.SequenceEqual(enumerable).ShouldBeTrue();
                        ++reset;
                    }
                };
                collection.AddComponent(changedListener);
            }

            for (var i = 0; i < TestIterationCount; i++)
            {
                expectedItem = new[] { new CollectionItem(), new CollectionItem() };
                collection.Reset(expectedItem);
            }

            condition.ShouldEqual(TestIterationCount);
            resetting.ShouldEqual(0);
            reset.ShouldEqual(0);

            condition = 0;
            canReset = true;
            for (var i = 0; i < TestIterationCount; i++)
            {
                expectedItem = new[] { new CollectionItem(), new CollectionItem() };
                collection.Reset(expectedItem);
            }

            condition.ShouldEqual(TestIterationCount * listenersCount);
            resetting.ShouldEqual(TestIterationCount * listenersCount);
            reset.ShouldEqual(TestIterationCount * listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void ShouldClearNotifyListeners(int listenersCount)
        {
            const int TestIterationCount = 10;
            var clearing = 0;
            var clear = 0;
            var condition = 0;
            var canClear = false;

            var collection = CreateCollection<CollectionItem>();
            for (var i = 0; i < TestIterationCount; i++)
                collection.Add(new CollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var conditionListener = new TestConditionObservableCollectionComponent<CollectionItem>(collection)
                {
                    CanClear = items =>
                    {
                        items.ShouldEqual(collection);
                        ++condition;
                        return canClear;
                    },
                    CanAdd = (items, item, arg3) => true,
                    Priority = -i
                };
                collection.AddComponent(conditionListener);

                var changingListener = new TestObservableCollectionChangingListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnClearing = items =>
                    {
                        items.ShouldEqual(collection);
                        ++clearing;
                    },
                    OnAdding = (items, item, arg3) => { }
                };
                collection.AddComponent(changingListener);

                var changedListener = new TestObservableCollectionChangedListener<CollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnCleared = items =>
                    {
                        items.ShouldEqual(collection);
                        ++clear;
                    },
                    OnAdded = (items, item, arg3) => { }
                };
                collection.AddComponent(changedListener);
            }

            for (var i = 0; i < TestIterationCount; i++)
                collection.Clear();
            condition.ShouldEqual(TestIterationCount);
            clearing.ShouldEqual(0);
            clear.ShouldEqual(0);

            condition = 0;
            canClear = true;
            for (var i = 0; i < TestIterationCount; i++)
            {
                collection.Clear();
                for (var j = 0; j < TestIterationCount; j++)
                    collection.Add(new CollectionItem());
            }

            condition.ShouldEqual(TestIterationCount * listenersCount);
            clearing.ShouldEqual(TestIterationCount * listenersCount);
            clear.ShouldEqual(TestIterationCount * listenersCount);
        }

        protected virtual IObservableCollection<T> CreateCollection<T>(params T[] items)
        {
            return new SynchronizedObservableCollection<T>(items);
        }

        protected override IObservableCollection<object> GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new SynchronizedObservableCollection<object>(collectionProvider);
        }

        #endregion
    }
}