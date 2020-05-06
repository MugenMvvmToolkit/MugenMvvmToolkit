using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.UnitTest.Collections.Internal;
using MugenMvvm.UnitTest.Models;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Collections.Components
{
    public class DecoratorManagerTest : UnitTestBase
    {
        #region Fields

        protected const int TestIterationCount = 50;

        #endregion

        #region Methods

        [Fact]
        public void DecoratorShouldDecorateItems()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();

            var decoratedItems = new[] { item1 };
            var collection = CreateCollection(item1, item2);
            collection.AddComponent(new DecoratorManager<TestCollectionItem>());
            var decorator = new TestObservableCollectionDecorator<TestCollectionItem>
            {
                DecorateItems = items =>
                {
                    items.SequenceEqual(new[] { item1, item2 }).ShouldBeTrue();
                    return decoratedItems;
                }
            };
            collection.AddComponent(decorator);
            collection.DecorateItems().SequenceEqual(decoratedItems).ShouldBeTrue();
        }

        [Fact]
        public void DecoratorShouldDecorateItemsMulti()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();

            var original = new[] { item1, item2 };
            var decoratedItems1 = new[] { item2 };
            var decoratedItems2 = new[] { item1 };
            var collection = CreateCollection(item1, item2);
            collection.AddComponent(new DecoratorManager<TestCollectionItem>());
            var decorator1 = new TestObservableCollectionDecorator<TestCollectionItem>
            {
                DecorateItems = items =>
                {
                    items.SequenceEqual(original).ShouldBeTrue();
                    return decoratedItems1;
                }
            };
            collection.AddComponent(decorator1);

            var decorator2 = new TestObservableCollectionDecorator<TestCollectionItem>
            {
                DecorateItems = items =>
                {
                    items.SequenceEqual(decoratedItems1).ShouldBeTrue();
                    return items.Concat(decoratedItems2);
                },
                Priority = -1
            };
            collection.AddComponent(decorator2);

            collection.DecorateItems().SequenceEqual(decoratedItems1.Concat(decoratedItems2)).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void DecoratorShouldTrackItemsMulti1(bool defaultComparer, bool filterFirst)
        {
            var comparer = defaultComparer ? Comparer<int>.Default : Comparer<int>.Create((i, i1) => i1.CompareTo(i));
            var observableCollection = CreateCollection<int>();
            observableCollection.AddComponent(new DecoratorManager<int>());
            var decorator1 = new OrderedObservableCollectionDecorator<int>(comparer);
            var decorator2 = new FilterObservableCollectionDecorator<int> { Filter = i => i % 2 == 0 };
            if (filterFirst)
                decorator2.Priority = int.MaxValue;
            else
                decorator1.Priority = int.MaxValue;
            observableCollection.AddComponent(decorator1);
            observableCollection.AddComponent(decorator2);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.OrderBy(i => i, comparer).Where(decorator2.Filter);

            observableCollection.Add(1);
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            observableCollection.Insert(1, 2);
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            observableCollection.Remove(2);
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            observableCollection.RemoveAt(0);
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            observableCollection.Reset(new[] { 1, 2, 3, 4, 5 });
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            observableCollection[0] = 200;
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            observableCollection.Move(1, 2);
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            observableCollection.Clear();
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            for (var i = 0; i < 100; i++)
            {
                observableCollection.Add(Guid.NewGuid().GetHashCode());
                tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
            }

            for (var i = 0; i < 10; i++)
            {
                observableCollection.Move(i, i + 1);
                tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
            }

            for (var i = 0; i < 10; i++)
            {
                observableCollection[i] = i + Guid.NewGuid().GetHashCode();
                tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
            }

            for (var i = 0; i < 100; i++)
            {
                observableCollection.RemoveAt(0);
                tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
            }

            observableCollection.Clear();
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void DecoratorShouldTrackItemsMulti2(bool defaultComparer, bool filterFirst)
        {
            var comparer = Comparer<TestCollectionItem>.Create((item, collectionItem) =>
            {
                if (defaultComparer)
                    return item.Id.CompareTo(collectionItem.Id);
                return collectionItem.Id.CompareTo(item.Id);
            });
            var observableCollection = CreateCollection<TestCollectionItem>();
            observableCollection.AddComponent(new DecoratorManager<TestCollectionItem>());
            var decorator1 = new OrderedObservableCollectionDecorator<TestCollectionItem>(comparer);
            var decorator2 = new FilterObservableCollectionDecorator<TestCollectionItem> { Filter = i => i.Id % 2 == 0 };
            if (filterFirst)
                decorator2.Priority = int.MaxValue;
            else
                decorator1.Priority = int.MaxValue;
            observableCollection.AddComponent(decorator1);
            observableCollection.AddComponent(decorator2);

            var tracker = new DecoratorObservableCollectionTracker<TestCollectionItem>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.OrderBy(i => i, comparer).Where(decorator2.Filter);

            observableCollection.Add(new TestCollectionItem { Id = 1 });
            tracker.ChangedItems.SequenceEqual(items, TestCollectionItem.IdComparer).ShouldBeTrue();

            var item2 = new TestCollectionItem { Id = 2 };
            observableCollection.Insert(1, item2);
            tracker.ChangedItems.SequenceEqual(items, TestCollectionItem.IdComparer).ShouldBeTrue();

            observableCollection.Remove(item2);
            tracker.ChangedItems.SequenceEqual(items, TestCollectionItem.IdComparer).ShouldBeTrue();

            observableCollection.RemoveAt(0);
            tracker.ChangedItems.SequenceEqual(items, TestCollectionItem.IdComparer).ShouldBeTrue();

            observableCollection.Reset(new[]
            {
                new TestCollectionItem {Id = 1}, new TestCollectionItem {Id = 2}, new TestCollectionItem {Id = 3},
                new TestCollectionItem {Id = 4}, new TestCollectionItem {Id = 5}
            });
            tracker.ChangedItems.SequenceEqual(items, TestCollectionItem.IdComparer).ShouldBeTrue();

            observableCollection[0] = new TestCollectionItem { Id = 200 };
            tracker.ChangedItems.SequenceEqual(items, TestCollectionItem.IdComparer).ShouldBeTrue();

            observableCollection.Move(1, 2);
            tracker.ChangedItems.SequenceEqual(items, TestCollectionItem.IdComparer).ShouldBeTrue();

            observableCollection.Clear();
            tracker.ChangedItems.SequenceEqual(items, TestCollectionItem.IdComparer).ShouldBeTrue();

            for (var i = 0; i < 100; i++)
            {
                observableCollection.Add(new TestCollectionItem { Id = Guid.NewGuid().GetHashCode() });
                tracker.ChangedItems.SequenceEqual(items, TestCollectionItem.IdComparer).ShouldBeTrue();
            }

            for (var i = 0; i < 10; i++)
            {
                observableCollection.Move(i, i + 1);
                tracker.ChangedItems.SequenceEqual(items, TestCollectionItem.IdComparer).ShouldBeTrue();
            }

            for (var i = 0; i < 10; i++)
            {
                observableCollection[i] = new TestCollectionItem { Id = i + Guid.NewGuid().GetHashCode() };
                tracker.ChangedItems.SequenceEqual(items, TestCollectionItem.IdComparer).ShouldBeTrue();
            }

            for (var i = 0; i < 100; i++)
            {
                observableCollection[i].Id = Guid.NewGuid().GetHashCode();
                observableCollection.RaiseItemChanged(observableCollection[i], null);
                tracker.ChangedItems.SequenceEqual(items, TestCollectionItem.IdComparer).ShouldBeTrue();
            }

            for (var i = 0; i < 100; i++)
            {
                observableCollection.RemoveAt(0);
                tracker.ChangedItems.SequenceEqual(items, TestCollectionItem.IdComparer).ShouldBeTrue();
            }

            observableCollection.Clear();
            tracker.ChangedItems.SequenceEqual(items, TestCollectionItem.IdComparer).ShouldBeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void AddShouldNotifyListeners(int listenersCount)
        {
            var added = 0;
            var expectedIndex = 0;
            TestCollectionItem? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            collection.AddComponent(new DecoratorManager<TestCollectionItem>());

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestDecoratorObservableCollectionChangedListener<TestCollectionItem>(collection)
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
                expectedItem = new TestCollectionItem();
                expectedIndex = i;
                collection.Add(expectedItem);
            }

            added.ShouldEqual(TestIterationCount * listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void InsertShouldNotifyListeners(int listenersCount)
        {
            var added = 0;
            var expectedIndex = 0;
            TestCollectionItem? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            collection.AddComponent(new DecoratorManager<TestCollectionItem>());

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestDecoratorObservableCollectionChangedListener<TestCollectionItem>(collection)
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
                expectedItem = new TestCollectionItem();
                expectedIndex = 0;
                collection.Insert(0, expectedItem);
            }

            added.ShouldEqual(TestIterationCount * listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ReplaceShouldNotifyListeners(int listenersCount)
        {
            var replaced = 0;
            var expectedIndex = 0;
            TestCollectionItem? expectedOldItem = null, expectedNewItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            collection.AddComponent(new DecoratorManager<TestCollectionItem>());
            for (var i = 0; i < TestIterationCount; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestDecoratorObservableCollectionChangedListener<TestCollectionItem>(collection)
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
                expectedNewItem = new TestCollectionItem();
                expectedOldItem = collection[i];
                expectedIndex = i;
                collection[i] = expectedNewItem;
            }

            replaced.ShouldEqual(TestIterationCount * listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void MoveShouldNotifyListeners(int listenersCount)
        {
            var moved = 0;
            var expectedOldIndex = 0;
            var expectedNewIndex = 0;
            TestCollectionItem? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            collection.AddComponent(new DecoratorManager<TestCollectionItem>());
            for (var i = 0; i < TestIterationCount + 1; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestDecoratorObservableCollectionChangedListener<TestCollectionItem>(collection)
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

            moved.ShouldEqual(TestIterationCount * listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void RemoveShouldNotifyListeners(int listenersCount)
        {
            var removed = 0;
            var expectedIndex = 0;
            TestCollectionItem? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            collection.AddComponent(new DecoratorManager<TestCollectionItem>());
            for (var i = 0; i < TestIterationCount; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestDecoratorObservableCollectionChangedListener<TestCollectionItem>(collection)
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

            removed.ShouldEqual(TestIterationCount * listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void RemoveAtShouldNotifyListeners(int listenersCount)
        {
            var removed = 0;
            var expectedIndex = 0;
            TestCollectionItem? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            collection.AddComponent(new DecoratorManager<TestCollectionItem>());
            for (var i = 0; i < TestIterationCount; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestDecoratorObservableCollectionChangedListener<TestCollectionItem>(collection)
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

            removed.ShouldEqual(TestIterationCount * listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ResetShouldNotifyListeners(int listenersCount)
        {
            var reset = 0;
            IEnumerable<TestCollectionItem>? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            collection.AddComponent(new DecoratorManager<TestCollectionItem>());
            for (var i = 0; i < TestIterationCount; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestDecoratorObservableCollectionChangedListener<TestCollectionItem>(collection)
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
                expectedItem = new[] { new TestCollectionItem(), new TestCollectionItem() };
                collection.Reset(expectedItem);
            }

            reset.ShouldEqual(TestIterationCount * listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void ClearShouldNotifyListeners(int listenersCount)
        {
            const int TestIterationCount = 10;
            var clear = 0;
            var collection = CreateCollection<TestCollectionItem>();
            collection.AddComponent(new DecoratorManager<TestCollectionItem>());
            for (var i = 0; i < TestIterationCount; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestDecoratorObservableCollectionChangedListener<TestCollectionItem>(collection)
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
            {
                collection.Clear();
                for (var j = 0; j < TestIterationCount; j++)
                    collection.Add(new TestCollectionItem());
            }

            clear.ShouldEqual(TestIterationCount * listenersCount);
        }

        protected IObservableCollection<T> CreateCollection<T>(params T[] items)
        {
            return new SynchronizedObservableCollection<T>(items);
        }

        #endregion
    }
}