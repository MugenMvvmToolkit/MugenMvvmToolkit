using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Collections;
using MugenMvvm.UnitTests.Collections.Internal;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class CollectionDecoratorManagerTest : UnitTestBase
    {
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
            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestCollectionDecoratorListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnAdded = (c, item, index) =>
                    {
                        collection.ShouldEqual((object) c);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++added;
                    }
                });
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
        [InlineData(1, 10)]
        [InlineData(2, 1)]
        [InlineData(2, 10)]
        public void ClearShouldNotifyListeners(int listenersCount, int count)
        {
            var clear = 0;
            var collection = CreateCollection<TestCollectionItem>();
            for (var i = 0; i < count; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestCollectionDecoratorListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnReset = (c, v) =>
                    {
                        c.ShouldEqual((object) collection);
                        v.ShouldBeNull();
                        ++clear;
                    },
                    OnAdded = (_, _, _) => { }
                });
            }

            for (var i = 0; i < count; i++)
            {
                collection.Clear();
                for (var j = 0; j < count; j++)
                    collection.Add(new TestCollectionItem());
            }

            clear.ShouldEqual(count * listenersCount);
        }

        [Fact]
        public void DecoratorShouldDecorateItems()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();

            var decoratedItems = new[] {item1};
            var collection = CreateCollection(item1, item2);
            var decorator = new TestCollectionDecorator
            {
                Decorate = items =>
                {
                    items.ShouldEqual(new[] {item1, item2});
                    return decoratedItems;
                }
            };
            collection.AddComponent(decorator);
            collection.DecoratedItems().ShouldEqual(decoratedItems);
        }

        [Fact]
        public void DecoratorShouldDecorateItemsMulti4()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();

            var original = new[] {item1, item2};
            var decoratedItems1 = new[] {item2};
            var decoratedItems2 = new[] {item1};
            var collection = CreateCollection(item1, item2);
            var decorator1 = new TestCollectionDecorator
            {
                Decorate = items =>
                {
                    items.ShouldEqual(original);
                    return decoratedItems1;
                }
            };
            collection.AddComponent(decorator1);

            var decorator2 = new TestCollectionDecorator
            {
                Decorate = items =>
                {
                    items.ShouldEqual(decoratedItems1);
                    return items.Concat(decoratedItems2);
                },
                Priority = -1
            };
            collection.AddComponent(decorator2);

            collection.DecoratedItems().ShouldEqual(decoratedItems1.Concat(decoratedItems2));
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void DecoratorShouldTrackItemsMulti1(bool defaultComparer, bool filterFirst)
        {
            const int count = 100;
            var comparer = defaultComparer
                ? Comparer<object?>.Create((o, o1) => Comparer<int>.Default.Compare((int) o!, (int) o1!))
                : Comparer<object?>.Create((i, i1) => ((int) i1!).CompareTo((int) i!));
            var collection = CreateCollection<int>();
            var decorator1 = new SortingCollectionDecorator(comparer, filterFirst ? 0 : int.MaxValue);
            var decorator2 = new FilterCollectionDecorator<int>(null, filterFirst ? int.MaxValue : 0) {Filter = i => i % 2 == 0};
            collection.AddComponent(decorator1);
            collection.AddComponent(decorator2);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            tracker.Changed += () => tracker.ChangedItems.Cast<object>().ShouldEqual(collection.DecoratedItems());
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

            for (var i = 0; i < count; i++)
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

            for (var i = 0; i < count; i++)
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
            const int count = 100;
            var comparer = Comparer<object?>.Create((x1, x2) =>
            {
                var item = (TestCollectionItem) x1!;
                var collectionItem = (TestCollectionItem) x2!;
                if (defaultComparer)
                    return item.Id.CompareTo(collectionItem.Id);
                return collectionItem.Id.CompareTo(item.Id);
            });
            var collection = CreateCollection<TestCollectionItem>();
            var decorator1 = new SortingCollectionDecorator(comparer, filterFirst ? 0 : int.MaxValue);
            var decorator2 = new FilterCollectionDecorator<TestCollectionItem>(null, filterFirst ? int.MaxValue : 0) {Filter = i => i.Id % 2 == 0};
            collection.AddComponent(decorator1);
            collection.AddComponent(decorator2);

            var tracker = new DecoratorObservableCollectionTracker<TestCollectionItem>();
            tracker.Changed += () => tracker.ChangedItems.ShouldEqual(collection.DecoratedItems());
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

            for (var i = 0; i < count; i++)
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

            for (var i = 0; i < count; i++)
            {
                collection[i].Id = Guid.NewGuid().GetHashCode();
                collection.RaiseItemChanged(collection[i], null);
                tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);
            }

            for (var i = 0; i < count; i++)
            {
                collection.RemoveAt(0);
                tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);
            }

            collection.Clear();
            tracker.ChangedItems.ShouldEqual(items, TestCollectionItem.IdComparer);
        }

        [Fact]
        public void DecoratorShouldTrackItemsMulti3()
        {
            const int count = 100;
            var comparer = Comparer<object?>.Create((x1, x2) =>
            {
                var item = (TestCollectionItem) x1!;
                var collectionItem = (TestCollectionItem) x2!;
                return item.Id.CompareTo(collectionItem.Id);
            });
            var collection = CreateCollection<TestCollectionItem>();
            var tracker = new DecoratorObservableCollectionTracker<object>();
            tracker.Changed += () => tracker.ChangedItems.ShouldEqual(collection.DecoratedItems());
            collection.AddComponent(tracker);

            var decorator1 = new SortingCollectionDecorator(comparer);
            var decorator2 = new FilterCollectionDecorator<TestCollectionItem> {Filter = i => i.Id % 2 == 0};
            collection.AddComponent(decorator1);
            collection.AddComponent(decorator2);
            collection.AddComponent(new HeaderFooterCollectionDecorator {Header = "Header", Footer = "Footer"});
            collection.AddComponent(new GroupHeaderCollectionDecorator<TestCollectionItem, object>(o => o!.StableId % 2, null, null, true, -1));
            collection.AddComponent(new FlattenCollectionDecorator<TestCollectionItem>(o => new FlattenItemInfo(o.Items)));
            collection.AddComponent(new ItemHeaderFooterCollectionDecorator<TestCollectionItem>(t =>
            {
                if (t.StableId % 3 == 0)
                    return false;
                return null;
            }));
            collection.AddComponent(new ItemHeaderFooterCollectionDecorator<TestCollectionItem>(t =>
            {
                if (t.StableId % 10 == 0)
                    return true;
                return null;
            }, SortingComparer<TestCollectionItem>.Descending(o => o.StableId).Build()));
            collection.AddComponent(new LimitCollectionDecorator<TestCollectionItem>(50, item => item != null));

            collection.Add(new TestCollectionItem {Id = 1});
            tracker.ChangedItems.ShouldEqual(collection.DecoratedItems());

            var item2 = new TestCollectionItem {Id = 2};
            collection.Insert(1, item2);
            tracker.ChangedItems.ShouldEqual(collection.DecoratedItems());

            collection.Remove(item2);
            tracker.ChangedItems.ShouldEqual(collection.DecoratedItems());

            collection.RemoveAt(0);
            tracker.ChangedItems.ShouldEqual(collection.DecoratedItems());

            collection.Reset(new[]
            {
                new TestCollectionItem {Id = 1}, new TestCollectionItem {Id = 2}, new TestCollectionItem {Id = 3},
                new TestCollectionItem {Id = 4}, new TestCollectionItem {Id = 5}
            });
            tracker.ChangedItems.ShouldEqual(collection.DecoratedItems());

            collection[0] = new TestCollectionItem {Id = 200};
            tracker.ChangedItems.ShouldEqual(collection.DecoratedItems());

            collection.Move(1, 2);
            tracker.ChangedItems.ShouldEqual(collection.DecoratedItems());

            collection.Clear();
            tracker.ChangedItems.ShouldEqual(collection.DecoratedItems());

            for (var i = 0; i < count; i++)
            {
                collection.Add(new TestCollectionItem {Id = Guid.NewGuid().GetHashCode()});
                tracker.ChangedItems.ShouldEqual(collection.DecoratedItems());
            }

            for (var i = 0; i < 10; i++)
            {
                collection.Move(i, i + 1);
                tracker.ChangedItems.ShouldEqual(collection.DecoratedItems());
            }

            for (var i = 0; i < 10; i++)
            {
                collection[i] = new TestCollectionItem {Id = i + Guid.NewGuid().GetHashCode()};
                tracker.ChangedItems.ShouldEqual(collection.DecoratedItems());
            }

            for (var i = 0; i < count; i++)
            {
                collection[i].Id = Guid.NewGuid().GetHashCode();
                collection.RaiseItemChanged(collection[i], null);
                tracker.ChangedItems.ShouldEqual(collection.DecoratedItems());
            }

            for (var i = 0; i < count; i++)
            {
                collection.RemoveAt(0);
                tracker.ChangedItems.ShouldEqual(collection.DecoratedItems());
            }

            collection.Clear();
            tracker.ChangedItems.ShouldEqual(collection.DecoratedItems());
        }

        [Fact]
        public void DisposeShouldClearComponents()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();

            var decoratedItems = new[] {item1};
            var rawItems = new[] {item1, item2};
            var collection = CreateCollection(rawItems);
            collection.AddComponent(new TestCollectionDecorator
            {
                Decorate = items =>
                {
                    items.ShouldEqual(rawItems);
                    return decoratedItems;
                }
            });
            collection.DecoratedItems().ShouldEqual(decoratedItems);

            collection.Dispose();
            collection.GetComponents<object>().IsEmpty.ShouldBeTrue();
            collection.DecoratedItems().ShouldEqual(rawItems);
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

            for (var i = 0; i < listenersCount; i++)
            {
                var changedListener = new TestCollectionDecoratorListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnAdded = (c, item, index) =>
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
        public void MoveShouldNotifyListeners(int listenersCount, int count)
        {
            var moved = 0;
            var expectedOldIndex = 0;
            var expectedNewIndex = 0;
            TestCollectionItem? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            for (var i = 0; i < count + 1; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestCollectionDecoratorListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnMoved = (c, item, oldIndex, newIndex) =>
                    {
                        collection.ShouldEqual((object) c);
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

            moved.ShouldEqual(count * listenersCount);
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
            for (var i = 0; i < count; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestCollectionDecoratorListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnRemoved = (o, item, index) =>
                    {
                        o.ShouldEqual((object) collection);
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

            removed.ShouldEqual(count * listenersCount);
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
            for (var i = 0; i < count; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestCollectionDecoratorListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnRemoved = (c, item, index) =>
                    {
                        c.ShouldEqual((object) collection);
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
                collection.Remove(expectedItem);
            }

            removed.ShouldEqual(count * listenersCount);
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
            for (var i = 0; i < count; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestCollectionDecoratorListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnReplaced = (c, oldItem, newItem, index) =>
                    {
                        c.ShouldEqual((object) collection);
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

            replaced.ShouldEqual(count * listenersCount);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void ResetShouldNotifyListeners(int listenersCount, int count)
        {
            var reset = 0;
            IReadOnlyCollection<TestCollectionItem>? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            for (var i = 0; i < count; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestCollectionDecoratorListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnReset = (o, enumerable) =>
                    {
                        o.ShouldEqual((object) collection);
                        expectedItem.ShouldEqual(enumerable);
                        ++reset;
                    }
                });
            }

            for (var i = 0; i < count; i++)
            {
                expectedItem = new[] {new TestCollectionItem(), new TestCollectionItem()};
                collection.Reset(expectedItem);
            }

            reset.ShouldEqual(count * listenersCount);
        }

        [Fact]
        public void RaiseItemChangeShouldNotifyListeners()
        {
            var args = NewId();
            var item1 = NewId();
            var item2 = NewId();
            var item3 = NewId();

            var decorator1Count = 0;
            var decorator2Count = 0;
            var currentItem = item1;
            var currentIndex = 0;
            var collection = CreateCollection(item1);
            collection.AddComponent(new TestCollectionDecorator
            {
                Decorate = objects => objects.Concat(new[] {item2}),
                OnChanged = (ref object? item, ref int index, ref object? e) =>
                {
                    item.ShouldEqual(currentItem);
                    index.ShouldEqual(currentIndex);
                    e.ShouldEqual(args);
                    ++decorator1Count;
                    return true;
                },
                Priority = 0
            });
            collection.AddComponent(new TestCollectionDecorator
            {
                Decorate = objects => objects.Concat(new[] {item3}),
                OnChanged = (ref object? item, ref int index, ref object? e) =>
                {
                    item.ShouldEqual(currentItem);
                    index.ShouldEqual(currentIndex);
                    e.ShouldEqual(args);
                    ++decorator2Count;
                    return true;
                },
                Priority = -1
            });

            collection.RaiseItemChanged(item1, args);
            decorator1Count.ShouldEqual(1);
            decorator2Count.ShouldEqual(1);

            currentItem = item2;
            currentIndex = 1;
            collection.RaiseItemChanged(item2, args);
            decorator1Count.ShouldEqual(2);
            decorator2Count.ShouldEqual(2);

            currentItem = item3;
            currentIndex = 2;
            collection.RaiseItemChanged(item3, args);
            decorator1Count.ShouldEqual(2);
            decorator2Count.ShouldEqual(3);

            currentItem = NewId();
            currentIndex = -1;
            collection.RaiseItemChanged(currentItem, args);
            decorator1Count.ShouldEqual(2);
            decorator2Count.ShouldEqual(3);
        }

        protected IObservableCollection<T> CreateCollection<T>(params T[] items)
        {
            var collection = new SynchronizedObservableCollection<T>(items, ComponentCollectionManager);
            collection.AddComponent(new CollectionDecoratorManager<T>());
            return collection;
        }

        protected override IComponentCollectionManager GetComponentCollectionManager() => new ComponentCollectionManager();
    }
}