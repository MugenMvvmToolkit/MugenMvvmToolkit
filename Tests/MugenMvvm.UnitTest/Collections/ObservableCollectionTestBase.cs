using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections.Decorators;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.UnitTest.TestInfrastructure;
using MugenMvvm.UnitTest.TestModels;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Collections
{
    public abstract class ObservableCollectionTestBase : UnitTestBase
    {
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
        public void ContainsTest()
        {
            var item = new CollectionItem();
            var collection = CreateCollection<CollectionItem>();
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BeginEndBatchUpdateShouldCallListener(bool decorators)
        {
            var begin = 0;
            var end = 0;

            var collection = CreateCollection<CollectionItem>();
            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnBeginBatchUpdate = items => begin++,
                OnEndBatchUpdate = items => end++
            };
            if (decorators)
                collection.DecoratorListeners.Add(collectionListener);
            else
                collection.AddListener(collectionListener);

            var beginBatchUpdate1 = collection.BeginBatchUpdate();
            begin.ShouldEqual(1);
            end.ShouldEqual(0);

            var beginBatchUpdate2 = collection.BeginBatchUpdate();
            begin.ShouldEqual(1);
            end.ShouldEqual(0);

            beginBatchUpdate1.Dispose();
            begin.ShouldEqual(1);
            end.ShouldEqual(0);

            beginBatchUpdate2.Dispose();
            begin.ShouldEqual(1);
            end.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddNotificationTest(bool decorators)
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var expectedIndex = 0;
            var expectedItem = item1;
            var canAdd = false;
            var adding = 0;
            var added = 0;

            var collection = CreateCollection<CollectionItem>();
            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnAdding = (items, item, index) =>
                {
                    adding++;
                    item.ShouldEqual(expectedItem);
                    expectedIndex.ShouldEqual(index);
                    return canAdd;
                },
                OnAdded = (items, item, index) =>
                {
                    added++;
                    item.ShouldEqual(expectedItem);
                    expectedIndex.ShouldEqual(index);
                }
            };
            if (decorators)
                collection.DecoratorListeners.Add(collectionListener);
            else
            {
                collection.AddListener(collectionListener);

                collection.Add(item1);
                collection.Count.ShouldEqual(0);
                adding.ShouldEqual(1);
                added.ShouldEqual(0);
            }

            canAdd = true;
            collection.Add(item1);
            collection.Count.ShouldEqual(1);
            if (!decorators)
                adding.ShouldEqual(2);
            added.ShouldEqual(1);


            expectedIndex = 1;
            expectedItem = item2;
            collection.Add(item2);
            collection.Count.ShouldEqual(2);
            if (!decorators)
                adding.ShouldEqual(3);
            added.ShouldEqual(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InsertNotificationTest(bool decorators)
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var expectedIndex = 0;
            var expectedItem = item1;
            var canAdd = false;
            var adding = 0;
            var added = 0;

            var collection = CreateCollection<CollectionItem>();
            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnAdding = (items, item, index) =>
                {
                    adding++;
                    item.ShouldEqual(expectedItem);
                    expectedIndex.ShouldEqual(index);
                    return canAdd;
                },
                OnAdded = (items, item, index) =>
                {
                    added++;
                    item.ShouldEqual(expectedItem);
                    expectedIndex.ShouldEqual(index);
                }
            };
            if (decorators)
                collection.DecoratorListeners.Add(collectionListener);
            else
            {
                collection.AddListener(collectionListener);

                collection.Insert(0, item1);
                collection.Count.ShouldEqual(0);
                adding.ShouldEqual(1);
                added.ShouldEqual(0);
            }


            canAdd = true;
            collection.Insert(0, item1);
            collection.Count.ShouldEqual(1);
            if (!decorators)
                adding.ShouldEqual(2);
            added.ShouldEqual(1);

            expectedItem = item2;
            collection.Insert(0, item2);
            collection.Count.ShouldEqual(2);
            if (!decorators)
                adding.ShouldEqual(3);
            added.ShouldEqual(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReplaceNotificationTest(bool decorators)
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var expectedIndex = 0;
            var expectedOldItem = item1;
            var expectedNewItem = item2;
            var canReplace = false;
            var replacing = 0;
            var replaced = 0;

            var collection = CreateCollection(item1, item2);
            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnReplacing = (items, oldItem, newItem, index) =>
                {
                    replacing++;
                    expectedOldItem.ShouldEqual(oldItem);
                    expectedNewItem.ShouldEqual(newItem);
                    expectedIndex.ShouldEqual(index);
                    return canReplace;
                },
                OnReplaced = (items, oldItem, newItem, index) =>
                {
                    replaced++;
                    expectedOldItem.ShouldEqual(oldItem);
                    expectedNewItem.ShouldEqual(newItem);
                    expectedIndex.ShouldEqual(index);
                }
            };
            if (decorators)
                collection.DecoratorListeners.Add(collectionListener);
            else
            {
                collection.AddListener(collectionListener);

                collection[0] = item2;
                collection[0].ShouldEqual(item1);
                replacing.ShouldEqual(1);
                replaced.ShouldEqual(0);
            }


            canReplace = true;
            collection[0] = item2;
            collection[0].ShouldEqual(item2);
            if (!decorators)
                replacing.ShouldEqual(2);
            replaced.ShouldEqual(1);

            expectedOldItem = item2;
            expectedNewItem = item1;
            expectedIndex = 1;
            collection[1] = item1;
            collection[1].ShouldEqual(item1);
            if (!decorators)
                replacing.ShouldEqual(3);
            replaced.ShouldEqual(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MoveNotificationTest(bool decorators)
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var expectedItem = item1;
            var expectedOldIndex = 0;
            var expectedNewIndex = 1;
            var canMove = false;
            var moving = 0;
            var moved = 0;

            var collection = CreateCollection(item1, item2);
            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnMoving = (items, item, oldIndex, newIndex) =>
                {
                    moving++;
                    expectedItem.ShouldEqual(item);
                    expectedOldIndex.ShouldEqual(oldIndex);
                    expectedNewIndex.ShouldEqual(newIndex);
                    return canMove;
                },
                OnMoved = (items, item, oldIndex, newIndex) =>
                {
                    moved++;
                    expectedItem.ShouldEqual(item);
                    expectedOldIndex.ShouldEqual(oldIndex);
                    expectedNewIndex.ShouldEqual(newIndex);
                }
            };

            if (decorators)
                collection.DecoratorListeners.Add(collectionListener);
            else
            {
                collection.AddListener(collectionListener);

                collection.Move(0, 1);
                collection[0].ShouldEqual(item1);
                moving.ShouldEqual(1);
                moved.ShouldEqual(0);
            }

            canMove = true;
            collection.Move(0, 1);
            collection[1].ShouldEqual(item1);
            if (!decorators)
                moving.ShouldEqual(2);
            moved.ShouldEqual(1);

            expectedOldIndex = 1;
            expectedNewIndex = 0;
            collection.Move(1, 0);
            collection[1].ShouldEqual(item2);
            if (!decorators)
                moving.ShouldEqual(3);
            moved.ShouldEqual(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RemoveNotificationTest(bool decorators)
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var expectedIndex = 1;
            var expectedItem = item2;
            var canRemove = false;
            var removing = 0;
            var removed = 0;

            var collection = CreateCollection(item1, item2);
            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnRemoving = (items, item, index) =>
                {
                    removing++;
                    item.ShouldEqual(expectedItem);
                    expectedIndex.ShouldEqual(index);
                    return canRemove;
                },
                OnRemoved = (items, item, index) =>
                {
                    removed++;
                    item.ShouldEqual(expectedItem);
                    expectedIndex.ShouldEqual(index);
                }
            };

            if (decorators)
                collection.DecoratorListeners.Add(collectionListener);
            else
            {
                collection.AddListener(collectionListener);

                collection.Remove(item2);
                collection.Count.ShouldEqual(2);
                removing.ShouldEqual(1);
                removed.ShouldEqual(0);
            }

            canRemove = true;
            collection.Remove(item2);
            collection.Count.ShouldEqual(1);
            if (!decorators)
                removing.ShouldEqual(2);
            removed.ShouldEqual(1);


            expectedIndex = 0;
            expectedItem = item1;
            collection.Remove(item1);
            collection.Count.ShouldEqual(0);
            if (!decorators)
                removing.ShouldEqual(3);
            removed.ShouldEqual(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RemoveAtNotificationTest(bool decorators)
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var expectedIndex = 1;
            var expectedItem = item2;
            var canRemove = false;
            var removing = 0;
            var removed = 0;

            var collection = CreateCollection(item1, item2);
            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnRemoving = (items, item, index) =>
                {
                    removing++;
                    item.ShouldEqual(expectedItem);
                    expectedIndex.ShouldEqual(index);
                    return canRemove;
                },
                OnRemoved = (items, item, index) =>
                {
                    removed++;
                    item.ShouldEqual(expectedItem);
                    expectedIndex.ShouldEqual(index);
                }
            };
            if (decorators)
                collection.DecoratorListeners.Add(collectionListener);
            else
            {
                collection.AddListener(collectionListener);

                collection.RemoveAt(1);
                collection.Count.ShouldEqual(2);
                removing.ShouldEqual(1);
                removed.ShouldEqual(0);
            }

            canRemove = true;
            collection.RemoveAt(1);
            collection.Count.ShouldEqual(1);
            if (!decorators)
                removing.ShouldEqual(2);
            removed.ShouldEqual(1);


            expectedIndex = 0;
            expectedItem = item1;
            collection.RemoveAt(0);
            collection.Count.ShouldEqual(0);
            if (!decorators)
                removing.ShouldEqual(3);
            removed.ShouldEqual(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ResetNotificationTest(bool decorators)
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var resetItems = new[] { item1, item2 };

            var expectedItem = resetItems;
            var canReset = false;
            var resetting = 0;
            var reset = 0;

            var collection = CreateCollection(new CollectionItem());
            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnResetting = (_, items) =>
                {
                    resetting++;
                    items.ShouldEqual(expectedItem);
                    return canReset;
                },
                OnReset = (_, items) =>
                {
                    reset++;
                    items.ShouldEqual(expectedItem);
                }
            };
            if (decorators)
                collection.DecoratorListeners.Add(collectionListener);
            else
            {
                collection.AddListener(collectionListener);

                collection.Reset(resetItems);
                collection.Count.ShouldEqual(1);
                resetting.ShouldEqual(1);
                reset.ShouldEqual(0);
            }

            canReset = true;
            collection.Reset(resetItems);
            collection.SequenceEqual(resetItems).ShouldBeTrue();
            if (!decorators)
                resetting.ShouldEqual(2);
            reset.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ClearNotificationTest(bool decorators)
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var canClear = false;
            var clearing = 0;
            var cleared = 0;

            var collection = CreateCollection(item1, item2);
            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnClearing = _ =>
                {
                    clearing++;
                    return canClear;
                },
                OnCleared = _ => { cleared++; }
            };
            if (decorators)
                collection.DecoratorListeners.Add(collectionListener);
            else
            {
                collection.AddListener(collectionListener);

                collection.Clear();
                collection.Count.ShouldEqual(2);
                clearing.ShouldEqual(1);
                cleared.ShouldEqual(0);
            }

            canClear = true;
            collection.Clear();
            collection.Count.ShouldEqual(0);
            if (!decorators)
                clearing.ShouldEqual(2);
            cleared.ShouldEqual(1);
        }


        [Fact]
        public void AddNotificationDecoratorsTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var expectedIndex = 0;
            var expectedItem = item1;

            var decoratedIndex = 0;
            var decoratedItem = item1;

            var canAdd = false;
            var adding = 0;
            var added = 0;

            var collection = CreateCollection<CollectionItem>();
            var decorator = new TestObservableCollectionDecorator<CollectionItem>
            {
                ThrowErrorNullDelegate = true,
                OnAdded = (ref CollectionItem item, ref int index) =>
                {
                    adding++;
                    item.ShouldEqual(expectedItem);
                    expectedIndex.ShouldEqual(index);

                    item = decoratedItem;
                    index = decoratedIndex;

                    return canAdd;
                }
            };
            collection.Decorators.Add(decorator);

            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnAdded = (items, item, index) =>
                {
                    added++;
                    item.ShouldEqual(decoratedItem);
                    decoratedIndex.ShouldEqual(index);
                }
            };

            collection.DecoratorListeners.Add(collectionListener);

            collection.Add(item1);
            collection.Count.ShouldEqual(1);
            adding.ShouldEqual(1);
            added.ShouldEqual(0);

            canAdd = true;
            expectedIndex = 1;
            collection.Add(item1);
            collection.Count.ShouldEqual(2);

            adding.ShouldEqual(2);
            added.ShouldEqual(1);

            expectedIndex = 2;
            expectedItem = item2;

            decoratedIndex = 0;
            decoratedItem = item1;
            collection.Add(item2);
            collection.Count.ShouldEqual(3);
            adding.ShouldEqual(3);
            added.ShouldEqual(2);
        }


        [Fact]
        public void InsertNotificationDecoratorsTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var expectedIndex = 0;
            var expectedItem = item1;

            var decoratedIndex = 0;
            var decoratedItem = item1;

            var canAdd = false;
            var adding = 0;
            var added = 0;

            var collection = CreateCollection<CollectionItem>();
            var decorator = new TestObservableCollectionDecorator<CollectionItem>
            {
                ThrowErrorNullDelegate = true,
                OnAdded = (ref CollectionItem item, ref int index) =>
                {
                    adding++;
                    item.ShouldEqual(expectedItem);
                    expectedIndex.ShouldEqual(index);

                    item = decoratedItem;
                    index = decoratedIndex;

                    return canAdd;
                }
            };
            collection.Decorators.Add(decorator);

            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnAdded = (items, item, index) =>
                {
                    added++;
                    item.ShouldEqual(decoratedItem);
                    decoratedIndex.ShouldEqual(index);
                }
            };

            collection.DecoratorListeners.Add(collectionListener);

            collection.Insert(0, item1);
            collection.Count.ShouldEqual(1);
            adding.ShouldEqual(1);
            added.ShouldEqual(0);

            canAdd = true;
            expectedIndex = 1;
            collection.Insert(1, item1);
            collection.Count.ShouldEqual(2);

            adding.ShouldEqual(2);
            added.ShouldEqual(1);

            expectedIndex = 2;
            expectedItem = item2;

            decoratedIndex = 0;
            decoratedItem = item1;
            collection.Insert(2, item2);
            collection.Count.ShouldEqual(3);
            adding.ShouldEqual(3);
            added.ShouldEqual(2);
        }

        [Fact]
        public void ReplaceNotificationDecoratorsTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var expectedIndex = 0;
            var expectedOldItem = item1;
            var expectedNewItem = item2;


            var decoratedIndex = 0;
            var decoratedOldItem = item1;
            var decoratedNewItem = item2;
            var canReplace = false;
            var replacing = 0;
            var replaced = 0;

            var collection = CreateCollection(item1, item2);

            var decorator = new TestObservableCollectionDecorator<CollectionItem>
            {
                OnReplaced = (ref CollectionItem oldItem, ref CollectionItem newItem, ref int index) =>
                {
                    replacing++;
                    expectedOldItem.ShouldEqual(oldItem);
                    expectedNewItem.ShouldEqual(newItem);
                    expectedIndex.ShouldEqual(index);

                    oldItem = decoratedOldItem;
                    newItem = decoratedNewItem;
                    index = decoratedIndex;

                    return canReplace;
                }
            };
            collection.Decorators.Add(decorator);

            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                OnReplaced = (items, oldItem, newItem, index) =>
                {
                    replaced++;
                    decoratedOldItem.ShouldEqual(oldItem);
                    decoratedNewItem.ShouldEqual(newItem);
                    decoratedIndex.ShouldEqual(index);
                }
            };
            collection.DecoratorListeners.Add(collectionListener);

            collection[0] = item2;
            replacing.ShouldEqual(1);
            replaced.ShouldEqual(0);

            canReplace = true;
            decoratedOldItem = item2;
            decoratedNewItem = item1;
            expectedOldItem = item2;
            expectedNewItem = item1;

            collection[0] = item1;
            replacing.ShouldEqual(2);
            replaced.ShouldEqual(1);

            expectedOldItem = item2;
            expectedNewItem = item1;
            expectedIndex = 1;

            decoratedOldItem = item1;
            decoratedNewItem = item1;
            decoratedIndex = int.MinValue;

            collection[1] = item1;
            collection[1].ShouldEqual(item1);
            replacing.ShouldEqual(3);
            replaced.ShouldEqual(2);
        }


        [Fact]
        public void MoveNotificationDecoratorsTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var expectedItem = item1;
            var expectedOldIndex = 0;
            var expectedNewIndex = 1;

            var decoratedOldIndex = 0;
            var decoratedNewIndex = 1;
            var decoratedItem = item1;
            var canMove = false;
            var moving = 0;
            var moved = 0;

            var collection = CreateCollection(item1, item2);
            var decorator = new TestObservableCollectionDecorator<CollectionItem>
            {
                OnMoved = (ref CollectionItem item, ref int oldIndex, ref int newIndex) =>
                {
                    moving++;
                    expectedItem.ShouldEqual(item);
                    expectedOldIndex.ShouldEqual(oldIndex);
                    expectedNewIndex.ShouldEqual(newIndex);

                    item = decoratedItem;
                    oldIndex = decoratedOldIndex;
                    newIndex = decoratedNewIndex;

                    return canMove;
                }
            };
            collection.Decorators.Add(decorator);

            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnMoved = (items, item, oldIndex, newIndex) =>
                {
                    moved++;
                    decoratedItem.ShouldEqual(item);
                    decoratedOldIndex.ShouldEqual(oldIndex);
                    decoratedNewIndex.ShouldEqual(newIndex);
                }
            };

            collection.DecoratorListeners.Add(collectionListener);

            collection.Move(0, 1);
            moving.ShouldEqual(1);
            moved.ShouldEqual(0);

            canMove = true;
            expectedItem = item2;
            decoratedItem = item2;
            collection.Move(0, 1);
            collection[1].ShouldEqual(item2);
            moving.ShouldEqual(2);
            moved.ShouldEqual(1);

            expectedOldIndex = 1;
            expectedNewIndex = 0;
            decoratedNewIndex = 0;
            decoratedOldIndex = 1;

            collection.Move(1, 0);
            collection[1].ShouldEqual(item1);
            moving.ShouldEqual(3);
            moved.ShouldEqual(2);
        }

        [Fact]
        public void RemoveNotificationDecoratorsTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var expectedIndex = 1;
            var expectedItem = item2;

            var decoratedIndex = 0;
            var decoratedItem = item1;
            var canRemove = false;
            var removing = 0;
            var removed = 0;

            var collection = CreateCollection(item1, item2);

            var decorator = new TestObservableCollectionDecorator<CollectionItem>
            {
                OnRemoved = (ref CollectionItem item, ref int index) =>
                {
                    removing++;
                    item.ShouldEqual(expectedItem);
                    expectedIndex.ShouldEqual(index);

                    index = decoratedIndex;
                    item = decoratedItem;

                    return canRemove;
                }
            };
            collection.Decorators.Add(decorator);

            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnRemoved = (items, item, index) =>
                {
                    removed++;
                    item.ShouldEqual(decoratedItem);
                    index.ShouldEqual(decoratedIndex);
                }
            };
            collection.DecoratorListeners.Add(collectionListener);

            collection.Remove(item2);
            collection.Count.ShouldEqual(1);
            removing.ShouldEqual(1);
            removed.ShouldEqual(0);

            canRemove = true;
            expectedItem = item1;
            expectedIndex = 0;

            decoratedIndex = int.MinValue;

            collection.Remove(item1);
            collection.Count.ShouldEqual(0);
            removing.ShouldEqual(2);
            removed.ShouldEqual(1);
        }

        [Fact]
        public void RemoveAtNotificationDecoratorsTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var expectedIndex = 1;
            var expectedItem = item2;

            var decoratedIndex = 0;
            var decoratedItem = item1;
            var canRemove = false;
            var removing = 0;
            var removed = 0;

            var collection = CreateCollection(item1, item2);

            var decorator = new TestObservableCollectionDecorator<CollectionItem>
            {
                OnRemoved = (ref CollectionItem item, ref int index) =>
                {
                    removing++;
                    item.ShouldEqual(expectedItem);
                    expectedIndex.ShouldEqual(index);

                    index = decoratedIndex;
                    item = decoratedItem;

                    return canRemove;
                }
            };
            collection.Decorators.Add(decorator);

            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnRemoved = (items, item, index) =>
                {
                    removed++;
                    item.ShouldEqual(decoratedItem);
                    index.ShouldEqual(decoratedIndex);
                }
            };
            collection.DecoratorListeners.Add(collectionListener);

            collection.RemoveAt(1);
            collection.Count.ShouldEqual(1);
            removing.ShouldEqual(1);
            removed.ShouldEqual(0);

            canRemove = true;
            expectedItem = item1;
            expectedIndex = 0;

            decoratedIndex = int.MinValue;

            collection.RemoveAt(0);
            collection.Count.ShouldEqual(0);
            removing.ShouldEqual(2);
            removed.ShouldEqual(1);
        }

        [Fact]
        public void ResetNotificationDecoratorsTest()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var resetItems = new[] { item1, item2 };

            var expectedItem = resetItems;
            var decoratedItem = resetItems;

            var canReset = false;
            var resetting = 0;
            var reset = 0;

            var collection = CreateCollection(new CollectionItem());

            var decorator = new TestObservableCollectionDecorator<CollectionItem>
            {
                OnReset = (ref IEnumerable<CollectionItem> items) =>
                {
                    resetting++;
                    items.ShouldEqual(expectedItem);

                    items = decoratedItem;
                    return canReset;
                }
            };
            collection.Decorators.Add(decorator);

            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnReset = (_, items) =>
                {
                    reset++;
                    items.ShouldEqual(decoratedItem);
                }
            };

            collection.DecoratorListeners.Add(collectionListener);

            collection.Reset(resetItems);
            collection.SequenceEqual(resetItems).ShouldBeTrue();
            resetting.ShouldEqual(1);
            reset.ShouldEqual(0);

            canReset = true;
            decoratedItem = new CollectionItem[0];

            collection.Reset(resetItems);
            collection.SequenceEqual(resetItems).ShouldBeTrue();
            resetting.ShouldEqual(2);
            reset.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ClearNotificationDecoratorsTest(bool canClear)
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var clearing = 0;
            var cleared = 0;

            var collection = CreateCollection(item1, item2);
            var decorator = new TestObservableCollectionDecorator<CollectionItem>
            {
                OnCleared = () =>
                {
                    clearing++;
                    return canClear;
                }
            };
            collection.Decorators.Add(decorator);

            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnCleared = _ => { cleared++; }
            };
            collection.DecoratorListeners.Add(collectionListener);

            collection.Clear();
            collection.Count.ShouldEqual(0);
            clearing.ShouldEqual(1);
            cleared.ShouldEqual(canClear ? 1 : 0);
        }

        [Fact]
        public void DecoratorShouldDecorateItems()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var decoratedItems = new[] { item1 };
            var collection = CreateCollection(item1, item2);
            var decorator = new TestObservableCollectionDecorator<CollectionItem>
            {
                DecorateItems = items =>
                {
                    items.SequenceEqual(new[] { item1, item2 }).ShouldBeTrue();
                    return decoratedItems;
                }
            };
            collection.Decorators.Add(decorator);
            collection.DecorateItems().SequenceEqual(decoratedItems).ShouldBeTrue();
        }

        [Fact]
        public void DecoratorShouldDecorateItemsMulti()
        {
            var item1 = new CollectionItem();
            var item2 = new CollectionItem();

            var original = new[] { item1, item2 };
            var decoratedItems1 = new[] { item2 };
            var decoratedItems2 = new[] { item1 };
            var collection = CreateCollection(item1, item2);
            var decorator1 = new TestObservableCollectionDecorator<CollectionItem>
            {
                DecorateItems = items =>
                {
                    items.SequenceEqual(original).ShouldBeTrue();
                    return decoratedItems1;
                }
            };
            collection.Decorators.Add(decorator1);

            var decorator2 = new TestObservableCollectionDecorator<CollectionItem>
            {
                DecorateItems = items =>
                {
                    items.SequenceEqual(decoratedItems1).ShouldBeTrue();
                    return items.Concat(decoratedItems2);
                }
            };
            collection.Decorators.Add(decorator2);

            collection.DecorateItems().SequenceEqual(decoratedItems1.Concat(decoratedItems2)).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void DecoratorShouldTrackItemsMulti(bool defaultComparer, bool filterFirst)
        {
            var comparer = defaultComparer ? Comparer<int>.Default : Comparer<int>.Create((i, i1) => i1.CompareTo(i));
            var observableCollection = CreateCollection<int>();
            var decorator1 = new OrderedObservableCollectionDecorator<int>(comparer);
            var decorator2 = new FilterObservableCollectionDecorator<int> { Filter = i => i % 2 == 0 };

            if (filterFirst)
            {
                observableCollection.Decorators.Add(decorator2);
                observableCollection.Decorators.Add(decorator1);
            }
            else
            {
                observableCollection.Decorators.Add(decorator1);
                observableCollection.Decorators.Add(decorator2);
            }

            ((IObservableCollectionDecorator<int>)decorator1).OnAttached((IObservableCollectionDecoratorManager<int>)observableCollection); //todo remove
            ((IObservableCollectionDecorator<int>)decorator2).OnAttached((IObservableCollectionDecoratorManager<int>)observableCollection);//todo remove

            var tracker = new ObservableCollectionTracker<int>();
            observableCollection.DecoratorListeners.Add(tracker);
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

            for (int i = 0; i < 100; i++)
            {
                observableCollection.Add(Guid.NewGuid().GetHashCode());
                tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
            }

            for (int i = 0; i < 10; i++)
            {
                observableCollection.Move(i, i + 1);
                tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
            }

            for (int i = 0; i < 10; i++)
            {
                observableCollection[i] = i + Guid.NewGuid().GetHashCode();
                tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
            }

            for (int i = 0; i < 100; i++)
            {
                observableCollection.RemoveAt(0);
                tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
            }

            observableCollection.Clear();
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
        }

        protected abstract IObservableCollection<T> CreateCollection<T>(params T[] items);

        #endregion
    }
}