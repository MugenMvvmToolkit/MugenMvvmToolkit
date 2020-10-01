using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.UnitTests.Collections.Internal;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class CollectionDecoratorManagerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void DecoratorShouldDecorateItems()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();

            var decoratedItems = new[] {item1};
            var collection = CreateCollection(item1, item2);
            CollectionDecoratorManager.GetOrAdd(collection);
            var decorator = new TestCollectionDecorator
            {
                DecorateItems = items =>
                {
                    items.ShouldEqual(new[] {item1, item2});
                    return decoratedItems;
                }
            };
            collection.AddComponent(decorator);
            collection.DecorateItems().ShouldEqual(decoratedItems);
        }

        [Fact]
        public void DecoratorShouldDecorateItemsMulti()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();

            var original = new[] {item1, item2};
            var decoratedItems1 = new[] {item2};
            var decoratedItems2 = new[] {item1};
            var collection = CreateCollection(item1, item2);
            CollectionDecoratorManager.GetOrAdd(collection);
            var decorator1 = new TestCollectionDecorator
            {
                DecorateItems = items =>
                {
                    items.ShouldEqual(original);
                    return decoratedItems1;
                }
            };
            collection.AddComponent(decorator1);

            var decorator2 = new TestCollectionDecorator
            {
                DecorateItems = items =>
                {
                    items.ShouldEqual(decoratedItems1);
                    return items.Concat(decoratedItems2);
                },
                Priority = -1
            };
            collection.AddComponent(decorator2);

            collection.DecorateItems().ShouldEqual(decoratedItems1.Concat(decoratedItems2));
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void DecoratorShouldTrackItemsMulti1(bool defaultComparer, bool filterFirst)
        {
            var comparer = defaultComparer ? Comparer<object?>.Create((o, o1) => Comparer<int>.Default.Compare((int) o!, (int) o1!)) : Comparer<object?>.Create((i, i1) => ((int) i1!).CompareTo((int) i!));
            var collection = CreateCollection<int>();
            CollectionDecoratorManager.GetOrAdd(collection);
            var decorator1 = new SortingCollectionDecorator(comparer);
            var decorator2 = new FilterCollectionDecorator<int> {Filter = i => i % 2 == 0};
            if (filterFirst)
                decorator2.Priority = int.MaxValue;
            else
                decorator1.Priority = int.MaxValue;
            collection.AddComponent(decorator1);
            collection.AddComponent(decorator2);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            collection.AddComponent(tracker);
            var items = collection.OrderBy(i => i, comparer).Where(decorator2.Filter);

            collection.Add(1);
            tracker.ChangedItems.ShouldEqual(items);

            collection.Insert(1, 2);
            tracker.ChangedItems.ShouldEqual(items);

            collection.Remove(2);
            tracker.ChangedItems.ShouldEqual(items);

            collection.RemoveAt(0);
            tracker.ChangedItems.ShouldEqual(items);

            collection.Reset(new[] {1, 2, 3, 4, 5});
            tracker.ChangedItems.ShouldEqual(items);

            collection[0] = 200;
            tracker.ChangedItems.ShouldEqual(items);

            collection.Move(1, 2);
            tracker.ChangedItems.ShouldEqual(items);

            collection.Clear();
            tracker.ChangedItems.ShouldEqual(items);

            for (var i = 0; i < 100; i++)
            {
                collection.Add(Guid.NewGuid().GetHashCode());
                tracker.ChangedItems.ShouldEqual(items);
            }

            for (var i = 0; i < 10; i++)
            {
                collection.Move(i, i + 1);
                tracker.ChangedItems.ShouldEqual(items);
            }

            for (var i = 0; i < 10; i++)
            {
                collection[i] = i + Guid.NewGuid().GetHashCode();
                tracker.ChangedItems.ShouldEqual(items);
            }

            for (var i = 0; i < 100; i++)
            {
                collection.RemoveAt(0);
                tracker.ChangedItems.ShouldEqual(items);
            }

            collection.Clear();
            tracker.ChangedItems.ShouldEqual(items);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void DecoratorShouldTrackItemsMulti2(bool defaultComparer, bool filterFirst)
        {
            var comparer = Comparer<object?>.Create((x1, x2) =>
            {
                var item = (TestCollectionItem) x1!;
                var collectionItem = (TestCollectionItem) x2!;
                if (defaultComparer)
                    return item.Id.CompareTo(collectionItem.Id);
                return collectionItem.Id.CompareTo(item.Id);
            });
            var collection = CreateCollection<TestCollectionItem>();
            CollectionDecoratorManager.GetOrAdd(collection);
            var decorator1 = new SortingCollectionDecorator(comparer);
            var decorator2 = new FilterCollectionDecorator<TestCollectionItem> {Filter = i => i.Id % 2 == 0};
            if (filterFirst)
                decorator2.Priority = int.MaxValue;
            else
                decorator1.Priority = int.MaxValue;
            collection.AddComponent(decorator1);
            collection.AddComponent(decorator2);

            var tracker = new DecoratorObservableCollectionTracker<TestCollectionItem>();
            collection.AddComponent(tracker);
            var items = collection.OrderBy(i => i, comparer).Where(decorator2.Filter);

            collection.Add(new TestCollectionItem {Id = 1});
            tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);

            var item2 = new TestCollectionItem {Id = 2};
            collection.Insert(1, item2);
            tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);

            collection.Remove(item2);
            tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);

            collection.RemoveAt(0);
            tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);

            collection.Reset(new[]
            {
                new TestCollectionItem {Id = 1}, new TestCollectionItem {Id = 2}, new TestCollectionItem {Id = 3},
                new TestCollectionItem {Id = 4}, new TestCollectionItem {Id = 5}
            });
            tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);

            collection[0] = new TestCollectionItem {Id = 200};
            tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);

            collection.Move(1, 2);
            tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);

            collection.Clear();
            tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);

            for (var i = 0; i < 100; i++)
            {
                collection.Add(new TestCollectionItem {Id = Guid.NewGuid().GetHashCode()});
                tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);
            }

            for (var i = 0; i < 10; i++)
            {
                collection.Move(i, i + 1);
                tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);
            }

            for (var i = 0; i < 10; i++)
            {
                collection[i] = new TestCollectionItem {Id = i + Guid.NewGuid().GetHashCode()};
                tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);
            }

            for (var i = 0; i < 100; i++)
            {
                collection[i].Id = Guid.NewGuid().GetHashCode();
                collection.RaiseItemChanged(collection[i], null);
                tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);
            }

            for (var i = 0; i < 100; i++)
            {
                collection.RemoveAt(0);
                tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);
            }

            collection.Clear();
            tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void AddShouldNotifyListeners(int listenersCount, int count)
        {
            var added = 0;
            var expectedIndex = 0;
            TestCollectionItem? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            CollectionDecoratorManager.GetOrAdd(collection);

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestCollectionDecoratorListener<TestCollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnAdded = (item, index) =>
                    {
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++added;
                    }
                };
                collection.AddComponent(changedListener);
            }

            for (var i = 0; i < count; i++)
            {
                expectedItem = new TestCollectionItem();
                expectedIndex = i;
                collection.Add(expectedItem);
            }

            added.ShouldEqual(count * listenersCount);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void InsertShouldNotifyListeners(int listenersCount, int count)
        {
            var added = 0;
            var expectedIndex = 0;
            TestCollectionItem? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            CollectionDecoratorManager.GetOrAdd(collection);

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestCollectionDecoratorListener<TestCollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnAdded = (item, index) =>
                    {
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++added;
                    }
                };
                collection.AddComponent(changedListener);
            }

            for (var i = 0; i < count; i++)
            {
                expectedItem = new TestCollectionItem();
                expectedIndex = 0;
                collection.Insert(0, expectedItem);
            }

            added.ShouldEqual(count * listenersCount);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void ReplaceShouldNotifyListeners(int listenersCount, int count)
        {
            var replaced = 0;
            var expectedIndex = 0;
            TestCollectionItem? expectedOldItem = null, expectedNewItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            CollectionDecoratorManager.GetOrAdd(collection);
            for (var i = 0; i < count; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestCollectionDecoratorListener<TestCollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnReplaced = (oldItem, newItem, index) =>
                    {
                        expectedIndex.ShouldEqual(index);
                        oldItem.ShouldEqual(expectedOldItem);
                        newItem.ShouldEqual(expectedNewItem);
                        ++replaced;
                    }
                };
                collection.AddComponent(changedListener);
            }

            for (var i = 0; i < count; i++)
            {
                expectedNewItem = new TestCollectionItem();
                expectedOldItem = collection[i];
                expectedIndex = i;
                collection[i] = expectedNewItem;
            }

            replaced.ShouldEqual(count * listenersCount);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void MoveShouldNotifyListeners(int listenersCount, int count)
        {
            var moved = 0;
            var expectedOldIndex = 0;
            var expectedNewIndex = 0;
            TestCollectionItem? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            CollectionDecoratorManager.GetOrAdd(collection);

            for (var i = 0; i < count + 1; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestCollectionDecoratorListener<TestCollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnMoved = (item, oldIndex, newIndex) =>
                    {
                        oldIndex.ShouldEqual(expectedOldIndex);
                        newIndex.ShouldEqual(expectedNewIndex);
                        expectedItem.ShouldEqual(item);
                        ++moved;
                    }
                };
                collection.AddComponent(changedListener);
            }

            for (var i = 0; i < count; i++)
            {
                expectedOldIndex = i;
                expectedNewIndex = i + 1;
                expectedItem = collection[expectedOldIndex];
                collection.Move(expectedOldIndex, expectedNewIndex);
            }

            moved.ShouldEqual(count * listenersCount);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void RemoveShouldNotifyListeners(int listenersCount, int count)
        {
            var removed = 0;
            var expectedIndex = 0;
            TestCollectionItem? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            CollectionDecoratorManager.GetOrAdd(collection);

            for (var i = 0; i < count; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestCollectionDecoratorListener<TestCollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnRemoved = (item, index) =>
                    {
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++removed;
                    }
                };
                collection.AddComponent(changedListener);
            }

            for (var i = 0; i < count; i++)
            {
                expectedItem = collection[0];
                expectedIndex = 0;
                collection.Remove(expectedItem);
            }

            removed.ShouldEqual(count * listenersCount);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void RemoveAtShouldNotifyListeners(int listenersCount, int count)
        {
            var removed = 0;
            var expectedIndex = 0;
            TestCollectionItem? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            CollectionDecoratorManager.GetOrAdd(collection);

            for (var i = 0; i < count; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestCollectionDecoratorListener<TestCollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnRemoved = (item, index) =>
                    {
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++removed;
                    }
                };
                collection.AddComponent(changedListener);
            }

            for (var i = 0; i < count; i++)
            {
                expectedItem = collection[0];
                expectedIndex = 0;
                collection.RemoveAt(expectedIndex);
            }

            removed.ShouldEqual(count * listenersCount);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void ResetShouldNotifyListeners(int listenersCount, int count)
        {
            var reset = 0;
            IEnumerable<TestCollectionItem>? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            CollectionDecoratorManager.GetOrAdd(collection);

            for (var i = 0; i < count; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestCollectionDecoratorListener<TestCollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnReset = enumerable =>
                    {
                        expectedItem.ShouldEqual(enumerable);
                        ++reset;
                    }
                };
                collection.AddComponent(changedListener);
            }

            for (var i = 0; i < count; i++)
            {
                expectedItem = new[] {new TestCollectionItem(), new TestCollectionItem()};
                collection.Reset(expectedItem);
            }

            reset.ShouldEqual(count * listenersCount);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 10)]
        [InlineData(2, 1)]
        [InlineData(2, 10)]
        public void ClearShouldNotifyListeners(int listenersCount, int count)
        {
            var clear = 0;
            var collection = CreateCollection<TestCollectionItem>();
            CollectionDecoratorManager.GetOrAdd(collection);

            for (var i = 0; i < count; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestCollectionDecoratorListener<TestCollectionItem>(collection)
                {
                    ThrowErrorNullDelegate = true,
                    OnReset = v =>
                    {
                        v.ShouldBeNull();
                        ++clear;
                    },
                    OnAdded = (item, arg3) => { }
                };
                collection.AddComponent(changedListener);
            }

            for (var i = 0; i < count; i++)
            {
                collection.Clear();
                for (var j = 0; j < count; j++)
                    collection.Add(new TestCollectionItem());
            }

            clear.ShouldEqual(count * listenersCount);
        }

        protected IObservableCollection<T> CreateCollection<T>(params T[] items) => new SynchronizedObservableCollection<T>(items);

        #endregion
    }
}