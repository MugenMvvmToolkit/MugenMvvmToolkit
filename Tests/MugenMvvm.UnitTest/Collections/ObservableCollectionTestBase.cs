using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Models;
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

        [Fact]
        public void BeginEndBatchUpdateShouldCallListener()
        {
            int begin = 0;
            int end = 0;

            var collection = CreateCollection<CollectionItem>();
            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnBeginBatchUpdate = items => begin++,
                OnEndBatchUpdate = items => end++
            };
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
            int adding = 0;
            int added = 0;

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
            int adding = 0;
            int added = 0;

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
            int replacing = 0;
            int replaced = 0;

            var collection = CreateCollection<CollectionItem>(item1, item2);
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
            int moving = 0;
            int moved = 0;

            var collection = CreateCollection<CollectionItem>(item1, item2);
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
            int removing = 0;
            int removed = 0;

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
            int removing = 0;
            int removed = 0;

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
            int resetting = 0;
            int reset = 0;

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
            int clearing = 0;
            int cleared = 0;

            var collection = CreateCollection(item1, item2);
            var collectionListener = new CollectionListener<CollectionItem>(collection)
            {
                ThrowErrorNullDelegate = true,
                OnClearing = _ =>
                {
                    clearing++;
                    return canClear;
                },
                OnCleared = _ =>
                {
                    cleared++;
                }
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

        protected abstract IObservableCollection<T> CreateCollection<T>(params T[] items) where T : class;

        #endregion

        #region Nested types

        public class CollectionListener<T> : IObservableCollectionChangingListener<T>
        {
            #region Fields

            private readonly IObservableCollection<T> _collection;

            #endregion

            #region Constructors

            public CollectionListener(IObservableCollection<T> collection)
            {
                _collection = collection;
            }

            #endregion

            #region Properties

            public Func<IObservableCollection<T>, T, int, bool> OnAdding { get; set; }

            public Func<IObservableCollection<T>, T, T, int, bool> OnReplacing { get; set; }

            public Func<IObservableCollection<T>, T, int, int, bool> OnMoving { get; set; }

            public Func<IObservableCollection<T>, T, int, bool> OnRemoving { get; set; }

            public Func<IObservableCollection<T>, IEnumerable<T>, bool> OnResetting { get; set; }

            public Func<IObservableCollection<T>, bool> OnClearing { get; set; }

            public Action<IObservableCollection<T>> OnBeginBatchUpdate { get; set; }

            public Action<IObservableCollection<T>> OnEndBatchUpdate { get; set; }

            public Action<IObservableCollection<T>, T, int> OnAdded { get; set; }

            public Action<IObservableCollection<T>, T, T, int> OnReplaced { get; set; }

            public Action<IObservableCollection<T>, T, int, int> OnMoved { get; set; }

            public Action<IObservableCollection<T>, T, int> OnRemoved { get; set; }

            public Action<IObservableCollection<T>, IEnumerable<T>> OnReset { get; set; }

            public Action<IObservableCollection<T>> OnCleared { get; set; }

            public bool ThrowErrorNullDelegate { get; set; }

            #endregion

            #region Implementation of interfaces

            int IListener.GetPriority(object source)
            {
                return 0;
            }

            void IObservableCollectionChangedListener<T>.OnBeginBatchUpdate(IObservableCollection<T> collection)
            {
                _collection.ShouldEqual(collection);
                if (OnBeginBatchUpdate == null && ThrowErrorNullDelegate)
                    throw new NotSupportedException();
                OnBeginBatchUpdate?.Invoke(collection);
            }

            void IObservableCollectionChangedListener<T>.OnEndBatchUpdate(IObservableCollection<T> collection)
            {
                _collection.ShouldEqual(collection);
                if (OnEndBatchUpdate == null && ThrowErrorNullDelegate)
                    throw new NotSupportedException();
                OnEndBatchUpdate?.Invoke(collection);
            }

            void IObservableCollectionChangedListener<T>.OnAdded(IObservableCollection<T> collection, T item, int index)
            {
                _collection.ShouldEqual(collection);
                if (OnAdded == null && ThrowErrorNullDelegate)
                    throw new NotSupportedException();
                OnAdded?.Invoke(collection, item, index);
            }

            void IObservableCollectionChangedListener<T>.OnReplaced(IObservableCollection<T> collection, T oldItem, T newItem, int index)
            {
                _collection.ShouldEqual(collection);
                if (OnReplaced == null && ThrowErrorNullDelegate)
                    throw new NotSupportedException();
                OnReplaced?.Invoke(collection, oldItem, newItem, index);
            }

            void IObservableCollectionChangedListener<T>.OnMoved(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
            {
                _collection.ShouldEqual(collection);
                if (OnMoved == null && ThrowErrorNullDelegate)
                    throw new NotSupportedException();
                OnMoved?.Invoke(collection, item, oldIndex, newIndex);
            }

            void IObservableCollectionChangedListener<T>.OnRemoved(IObservableCollection<T> collection, T item, int index)
            {
                _collection.ShouldEqual(collection);
                if (OnRemoved == null && ThrowErrorNullDelegate)
                    throw new NotSupportedException();
                OnRemoved?.Invoke(collection, item, index);
            }

            void IObservableCollectionChangedListener<T>.OnReset(IObservableCollection<T> collection, IEnumerable<T> items)
            {
                _collection.ShouldEqual(collection);
                if (OnReset == null && ThrowErrorNullDelegate)
                    throw new NotSupportedException();
                OnReset?.Invoke(collection, items);
            }

            void IObservableCollectionChangedListener<T>.OnCleared(IObservableCollection<T> collection)
            {
                _collection.ShouldEqual(collection);
                if (OnCleared == null && ThrowErrorNullDelegate)
                    throw new NotSupportedException();
                OnCleared?.Invoke(collection);
            }

            bool IObservableCollectionChangingListener<T>.OnAdding(IObservableCollection<T> collection, T item, int index)
            {
                _collection.ShouldEqual(collection);
                if (OnAdding == null && ThrowErrorNullDelegate)
                    throw new NotSupportedException();
                return OnAdding?.Invoke(collection, item, index) ?? true;
            }

            bool IObservableCollectionChangingListener<T>.OnReplacing(IObservableCollection<T> collection, T oldItem, T newItem, int index)
            {
                _collection.ShouldEqual(collection);
                if (OnReplacing == null && ThrowErrorNullDelegate)
                    throw new NotSupportedException();
                return OnReplacing?.Invoke(collection, oldItem, newItem, index) ?? true;
            }

            bool IObservableCollectionChangingListener<T>.OnMoving(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
            {
                _collection.ShouldEqual(collection);
                if (OnMoving == null && ThrowErrorNullDelegate)
                    throw new NotSupportedException();
                return OnMoving?.Invoke(collection, item, oldIndex, newIndex) ?? true;
            }

            bool IObservableCollectionChangingListener<T>.OnRemoving(IObservableCollection<T> collection, T item, int index)
            {
                _collection.ShouldEqual(collection);
                if (OnRemoving == null && ThrowErrorNullDelegate)
                    throw new NotSupportedException();
                return OnRemoving?.Invoke(collection, item, index) ?? true;
            }

            bool IObservableCollectionChangingListener<T>.OnResetting(IObservableCollection<T> collection, IEnumerable<T> items)
            {
                _collection.ShouldEqual(collection);
                if (OnResetting == null && ThrowErrorNullDelegate)
                    throw new NotSupportedException();
                return OnResetting?.Invoke(collection, items) ?? true;
            }

            bool IObservableCollectionChangingListener<T>.OnClearing(IObservableCollection<T> collection)
            {
                _collection.ShouldEqual(collection);
                if (OnClearing == null && ThrowErrorNullDelegate)
                    throw new NotSupportedException();
                return OnClearing?.Invoke(collection) ?? true;
            }

            #endregion
        }

        [DebuggerDisplay("Id = {Id}")]
        public sealed class CollectionItem
        {
            #region Fields

            private static int _idGenerator = -1;

            #endregion

            #region Constructors

            public CollectionItem()
            {
                Id = Interlocked.Increment(ref _idGenerator);
            }

            #endregion

            #region Properties

            public bool Hidden { get; set; }

            public string Name { get; set; }

            public int Id { get; set; }

            #endregion

            #region Methods

            public override string ToString()
            {
                return $"Id: {Id}";
            }

            #endregion
        }

        #endregion
    }
}