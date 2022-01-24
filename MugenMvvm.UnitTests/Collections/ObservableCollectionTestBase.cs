using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Tests.Collections;
using MugenMvvm.Tests.Internal;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Collections
{
    public abstract class ObservableCollectionTestBase : ComponentOwnerTestBase<IObservableCollection<object>>
    {
        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void AddShouldNotifyListeners(int listenersCount, int count)
        {
            var adding = 0;
            var added = 0;
            var condition = 0;
            var expectedIndex = 0;
            var canAdd = false;
            TestCollectionItem? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();

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
                collection.Add(expectedItem);
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
                collection.Add(expectedItem);
            }

            condition.ShouldEqual(count * listenersCount);
            adding.ShouldEqual(count * listenersCount);
            added.ShouldEqual(count * listenersCount);
        }

        [Fact]
        public void AddTest()
        {
            var item = new TestCollectionItem();
            var collection = CreateCollection<TestCollectionItem>();
            collection.Add(item);
            collection.Count.ShouldEqual(1);
            collection.Any(item1 => item1 == item).ShouldBeTrue();
        }

        [Fact]
        public void ClearItemsTest()
        {
            var items = new[] {new TestCollectionItem(), new TestCollectionItem()};
            var collection = CreateCollection(items);
            collection.Count.ShouldEqual(2);
            collection.Any(item => item == items[0]).ShouldBeTrue();
            collection.Any(item => item == items[1]).ShouldBeTrue();

            collection.Clear();
            if (collection.Count != 0)
                collection.Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 10)]
        [InlineData(2, 1)]
        [InlineData(2, 10)]
        public void ClearShouldNotifyListeners(int listenersCount, int count)
        {
            var clearing = 0;
            var clear = 0;
            var condition = 0;
            var canClear = false;

            var collection = CreateCollection<TestCollectionItem>();
            for (var i = 0; i < count; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestConditionCollectionComponent<TestCollectionItem>
                {
                    CanReset = (c, v) =>
                    {
                        c.ShouldEqual(collection);
                        v.ShouldBeNull();
                        ++condition;
                        return canClear;
                    },
                    CanAdd = (_, _, _) => true,
                    Priority = -i
                });

                collection.AddComponent(new TestCollectionChangingListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnResetting = (c, v) =>
                    {
                        c.ShouldEqual(collection);
                        v.ShouldBeNull();
                        ++clearing;
                    },
                    OnAdding = (_, _, _) => { }
                });

                collection.AddComponent(new TestCollectionChangedListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnReset = (c, v) =>
                    {
                        c.ShouldEqual(collection);
                        v.ShouldBeNull();
                        ++clear;
                    },
                    OnAdded = (_, _, _) => { }
                });
            }

            for (var i = 0; i < count; i++)
                collection.Clear();
            condition.ShouldEqual(count);
            clearing.ShouldEqual(0);
            clear.ShouldEqual(0);

            condition = 0;
            canClear = true;
            for (var i = 0; i < count; i++)
            {
                collection.Clear();
                for (var j = 0; j < count; j++)
                    collection.Add(new TestCollectionItem());
            }

            condition.ShouldEqual(count * listenersCount);
            clearing.ShouldEqual(count * listenersCount);
            clear.ShouldEqual(count * listenersCount);
        }

        [Fact]
        public void ContainsTest()
        {
            var item = new TestCollectionItem();
            var collection = CreateCollection<TestCollectionItem>();
            collection.Add(item);
            collection.Count.ShouldEqual(1);
            collection.Contains(item).ShouldBeTrue();
        }

        [Fact]
        public void CopyToTest()
        {
            var item = new TestCollectionItem();
            var collection = CreateCollection<TestCollectionItem>();
            collection.Add(item);

            var items = new TestCollectionItem[1];
            collection.CopyTo(items, 0);
            items[0].ShouldEqual(item);
        }

        [Fact]
        public void CreateWithItemsTest()
        {
            var items = new[] {new TestCollectionItem(), new TestCollectionItem()};
            var collection = CreateCollection(items);
            collection.Count.ShouldEqual(2);
            collection.Any(item => item == items[0]).ShouldBeTrue();
            collection.Any(item => item == items[1]).ShouldBeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void DisposeShouldBeHandledByComponents(int componentCount)
        {
            var collection = CreateCollection<object>();
            var countDisposed = 0;
            var countDisposing = 0;

            for (var i = 0; i < componentCount; i++)
            {
                collection.AddComponent(new TestDisposableComponent<IReadOnlyObservableCollection>
                {
                    OnDisposed = (o, _) =>
                    {
                        o.ShouldEqual((object) collection);
                        ++countDisposed;
                    },
                    OnDisposing = (o, _) =>
                    {
                        o.ShouldEqual((object) collection);
                        ++countDisposing;
                    }
                });
            }

            collection.Dispose();
            collection.IsDisposed.ShouldBeTrue();
            countDisposing.ShouldEqual(componentCount);
            countDisposed.ShouldEqual(componentCount);
        }

        [Fact]
        public void EnumeratorTest()
        {
            var items = new List<TestCollectionItem>();
            var collection = CreateCollection<TestCollectionItem>();
            for (var i = 0; i < 1000; i++)
            {
                items.Add(new TestCollectionItem());
                collection.Add(items[i]);
            }

            collection.ShouldEqual(items);
        }

        [Fact]
        public void ICollectionCopyToTest()
        {
            var item = new TestCollectionItem();
            var collection = CreateCollection<TestCollectionItem>();
            collection.Add(item);

            var items = new TestCollectionItem[1];
            ((ICollection) collection).CopyTo(items, 0);
            items[0].ShouldEqual(item);
        }

        [Fact]
        public void IEnumerableEnumeratorTest()
        {
            var items = new List<TestCollectionItem>();
            var collection = CreateCollection<TestCollectionItem>();
            for (var i = 0; i < 1000; i++)
            {
                items.Add(new TestCollectionItem());
                collection.Add(items[i]);
            }

            collection.OfType<TestCollectionItem>().ShouldEqual(items);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void RemoveShouldNotifyListeners(int listenersCount, int count)
        {
            var removing = 0;
            var removed = 0;
            var condition = 0;
            var expectedIndex = 0;
            var canRemove = false;
            TestCollectionItem? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();
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
                expectedItem = collection.ElementAt(0);
                expectedIndex = 0;
                collection.Remove(expectedItem);
            }

            condition.ShouldEqual(count);
            removing.ShouldEqual(0);
            removed.ShouldEqual(0);

            condition = 0;
            canRemove = true;
            for (var i = 0; i < count; i++)
            {
                expectedItem = collection.ElementAt(0);
                expectedIndex = 0;
                collection.Remove(expectedItem);
            }

            condition.ShouldEqual(count * listenersCount);
            removing.ShouldEqual(count * listenersCount);
            removed.ShouldEqual(count * listenersCount);
        }

        [Fact]
        public void RemoveTest()
        {
            var item = new TestCollectionItem();
            var collection = CreateCollection<TestCollectionItem>();
            collection.Add(item);
            collection.Count.ShouldEqual(1);
            collection.Any(item1 => item1 == item).ShouldBeTrue();

            collection.Remove(item);
            collection.Count.ShouldEqual(0);
            collection.Any(item1 => item1 == item).ShouldBeFalse();
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 50)]
        [InlineData(10, 1)]
        [InlineData(10, 50)]
        public void ResetShouldNotifyListeners(int listenersCount, int count)
        {
            var resetting = 0;
            var reset = 0;
            var condition = 0;
            var canReset = false;
            IReadOnlyCollection<TestCollectionItem>? expectedItem = null;
            var collection = CreateCollection<TestCollectionItem>();
            for (var i = 0; i < count; i++)
                collection.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestConditionCollectionComponent<TestCollectionItem>
                {
                    CanReset = (c, enumerable) =>
                    {
                        c.ShouldEqual(collection);
                        expectedItem.ShouldEqual(enumerable);
                        ++condition;
                        return canReset;
                    },
                    Priority = -i
                });

                collection.AddComponent(new TestCollectionChangingListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnResetting = (c, enumerable) =>
                    {
                        c.ShouldEqual(collection);
                        expectedItem.ShouldEqual(enumerable);
                        ++resetting;
                    }
                });

                collection.AddComponent(new TestCollectionChangedListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnReset = (c, enumerable) =>
                    {
                        c.ShouldEqual(collection);
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

            condition.ShouldEqual(count);
            resetting.ShouldEqual(0);
            reset.ShouldEqual(0);

            condition = 0;
            canReset = true;
            for (var i = 0; i < count; i++)
            {
                expectedItem = new[] {new TestCollectionItem(), new TestCollectionItem()};
                collection.Reset(expectedItem);
            }

            condition.ShouldEqual(count * listenersCount);
            resetting.ShouldEqual(count * listenersCount);
            reset.ShouldEqual(count * listenersCount);
        }

        [Fact]
        public void ResetTest()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();
            var collection = CreateCollection(new TestCollectionItem());

            collection.Reset(new[] {item1, item2});
            collection.ElementAt(0).ShouldEqual(item1);
            collection.ElementAt(1).ShouldEqual(item2);
            collection.Count.ShouldEqual(2);
        }

        protected abstract IObservableCollection<T> CreateCollection<T>(params T[] items);

        protected override IObservableCollection<object> GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) =>
            new ObservableList<object>(componentCollectionManager);
    }
}