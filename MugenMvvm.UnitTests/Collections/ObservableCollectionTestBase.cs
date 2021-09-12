﻿using System.Collections;
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
        public void GetSetTest()
        {
            var item1 = new TestCollectionItem();
            var item2 = new TestCollectionItem();
            var collection = CreateCollection<TestCollectionItem>();
            collection.Add(item1);

            collection[0].ShouldEqual(item1);
            collection[0] = item2;
            collection[0].ShouldEqual(item2);
        }

        [Fact]
        public void ICollectionCopyToTest()
        {
            var item = new TestCollectionItem();
            var collection = (IList) CreateCollection<TestCollectionItem>();
            collection.Add(item);

            var items = new TestCollectionItem[1];
            collection.CopyTo(items, 0);
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
            var collection = (IList) CreateCollection<TestCollectionItem>();
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
            var collection = CreateCollection<TestCollectionItem>();

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
            var collection = CreateCollection<TestCollectionItem>();
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
            var collection = CreateCollection<TestCollectionItem>();
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
            var collection = CreateCollection(item1, item2);

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
            var collection = CreateCollection<TestCollectionItem>();
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
                expectedItem = collection[0];
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
                expectedItem = collection[0];
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
        public void ReplaceShouldNotifyListeners(int listenersCount, int count)
        {
            var replacing = 0;
            var replaced = 0;
            var condition = 0;
            var expectedIndex = 0;
            var canReplace = false;
            TestCollectionItem? expectedOldItem = null, expectedNewItem = null;
            var collection = CreateCollection<TestCollectionItem>();
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
            collection[0].ShouldEqual(item1);
            collection[1].ShouldEqual(item2);
            collection.Count.ShouldEqual(2);
        }

        protected abstract IObservableCollection<T> CreateCollection<T>(params T[] items);

        protected override IObservableCollection<object> GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) =>
            new SynchronizedObservableCollection<object>(componentCollectionManager);
    }
}