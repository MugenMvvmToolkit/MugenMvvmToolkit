using System;
using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Tests.Collections;
using MugenMvvm.Tests.Internal;
using MugenMvvm.UnitTests.Collections.Components;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections
{
    [Collection(SharedContext)]
    public abstract class ReadOnlyObservableCollectionTestBase : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<object> _source;

        protected ReadOnlyObservableCollectionTestBase(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _source = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
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
            var source = new SynchronizedObservableCollection<TestCollectionItem>(ComponentCollectionManager);
            var target = GetCollection(source, false);

            for (var i = 0; i < listenersCount; i++)
            {
                target.AddComponent(new TestCollectionChangedListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnAdded = (c, item, index) =>
                    {
                        c.ShouldEqual(target);
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
                source.Add(expectedItem);
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
            var source = new SynchronizedObservableCollection<TestCollectionItem>(ComponentCollectionManager);
            var target = GetCollection(source, false);
            for (var i = 0; i < count; i++)
                source.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                target.AddComponent(new TestCollectionChangedListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnReset = (c, v) =>
                    {
                        c.ShouldEqual(target);
                        v.ShouldBeNull();
                        ++clear;
                    },
                    OnAdded = (_, _, _) => { }
                });
            }

            for (var i = 0; i < count; i++)
            {
                source.Clear();
                for (var j = 0; j < count; j++)
                    source.Add(new TestCollectionItem());
            }

            clear.ShouldEqual(count * listenersCount);
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
            var source = new SynchronizedObservableCollection<TestCollectionItem>(ComponentCollectionManager);
            var target = GetCollection(source, false);

            for (var i = 0; i < listenersCount; i++)
            {
                target.AddComponent(new TestCollectionChangedListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnAdded = (c, item, index) =>
                    {
                        c.ShouldEqual(target);
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
                source.Insert(0, expectedItem);
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
            var source = new SynchronizedObservableCollection<TestCollectionItem>(ComponentCollectionManager);
            var target = GetCollection(source, false);

            for (var i = 0; i < count + 1; i++)
                source.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                target.AddComponent(new TestCollectionChangedListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnMoved = (c, item, oldIndex, newIndex) =>
                    {
                        c.ShouldEqual(target);
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
                expectedItem = source[expectedOldIndex];
                source.Move(expectedOldIndex, expectedNewIndex);
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
            var source = new SynchronizedObservableCollection<TestCollectionItem>(ComponentCollectionManager);
            var target = GetCollection(source, false);
            for (var i = 0; i < count; i++)
                source.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                target.AddComponent(new TestCollectionChangedListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnRemoved = (c, item, index) =>
                    {
                        c.ShouldEqual(target);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++removed;
                    }
                });
            }

            for (var i = 0; i < count; i++)
            {
                expectedItem = source[0];
                expectedIndex = 0;
                source.RemoveAt(expectedIndex);
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
            var source = new SynchronizedObservableCollection<TestCollectionItem>(ComponentCollectionManager);
            var target = GetCollection(source, false);
            for (var i = 0; i < count; i++)
                source.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                target.AddComponent(new TestCollectionChangedListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnRemoved = (c, item, index) =>
                    {
                        c.ShouldEqual(target);
                        expectedIndex.ShouldEqual(index);
                        expectedItem.ShouldEqual(item);
                        ++removed;
                    }
                });
            }

            for (var i = 0; i < count; i++)
            {
                expectedItem = source[0];
                expectedIndex = 0;
                source.Remove(expectedItem);
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
            var source = new SynchronizedObservableCollection<TestCollectionItem>(ComponentCollectionManager);
            var target = GetCollection(source, false);
            for (var i = 0; i < count; i++)
                source.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                target.AddComponent(new TestCollectionChangedListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnReplaced = (c, oldItem, newItem, index) =>
                    {
                        c.ShouldEqual(target);
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
                expectedOldItem = source[i];
                expectedIndex = i;
                source[i] = expectedNewItem;
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
            var source = new SynchronizedObservableCollection<TestCollectionItem>(ComponentCollectionManager);
            var target = GetCollection(source, false);
            for (var i = 0; i < count; i++)
                source.Add(new TestCollectionItem());

            for (var i = 0; i < listenersCount; i++)
            {
                target.AddComponent(new TestCollectionChangedListener<TestCollectionItem>
                {
                    ThrowErrorNullDelegate = true,
                    OnReset = (c, enumerable) =>
                    {
                        c.ShouldEqual(target);
                        expectedItem.ShouldEqual(enumerable);
                        ++reset;
                    }
                });
            }

            for (var i = 0; i < count; i++)
            {
                expectedItem = new[] {new TestCollectionItem(), new TestCollectionItem()};
                source.Reset(expectedItem);
            }

            reset.ShouldEqual(count * listenersCount);
        }


        [Fact(Skip = ReleaseTest)]
        public void ShouldBeWeak()
        {
            var weakReference = WeakTest(_source);
            GcCollect();
            GcCollect();
            GcCollect();
            _source.Add(NewId());
            weakReference.IsAlive.ShouldBeFalse();
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(2, true)]
        public virtual void ShouldSynchronizeBatchUpdates(int batchUpdateType, bool supported)
        {
            var updateType = BatchUpdateType.Get(batchUpdateType);
            var batchCount = 0;
            var target = GetCollection(_source, false);
            target.AddComponent(new TestCollectionBatchUpdateListener
            {
                OnBeginBatchUpdate = (_, type) =>
                {
                    type.ShouldEqual(updateType);
                    ++batchCount;
                },
                OnEndBatchUpdate = (_, type) =>
                {
                    type.ShouldEqual(updateType);
                    --batchCount;
                }
            });

            var b1 = _source.BatchUpdate(updateType);
            batchCount.ShouldEqual(supported ? 1 : 0);

            var b2 = _source.BatchUpdate(updateType);
            batchCount.ShouldEqual(supported ? 1 : 0);

            b1.Dispose();
            batchCount.ShouldEqual(supported ? 1 : 0);

            b2.Dispose();
            batchCount.ShouldEqual(0);

            b1 = _source.BatchUpdate(updateType);
            b2 = _source.BatchUpdate(updateType);
            batchCount.ShouldEqual(supported ? 1 : 0);
            target.Dispose();
            batchCount.ShouldEqual(0);

            b1.Dispose();
            b2.Dispose();
            batchCount.ShouldEqual(0);
        }

        [Fact]
        public void ShouldSynchronizeItems()
        {
            var target = GetCollection(_source, false);
            for (var i = 0; i < 2; i++)
            {
                _source.Add(1);
                Assert(target, i == 0);

                _source.Insert(1, 2);
                Assert(target, i == 0);

                _source.Move(0, 1);
                Assert(target, i == 0);

                _source.Remove(2);
                Assert(target, i == 0);

                _source.RemoveAt(0);
                Assert(target, i == 0);

                _source.Reset(new object[] {1, 2, 3, 4, 5});
                Assert(target, i == 0);

                _source[0] = 200;
                Assert(target, i == 0);

                _source.Clear();
                Assert(target, i == 0);

                target.Dispose();
            }
        }

        [Fact]
        public void ShouldTrackDecoratedItems()
        {
            var source = new SynchronizedObservableCollection<TestCollectionItem>(ComponentCollectionManager);
            var target = GetCollection(source, false);
            var target1 = GetCollection(target, false);
            CollectionDecoratorManagerTest.ShouldTrackItemsMulti3(source, target1);
        }

        [Fact]
        public void SourceShouldDisposeTargetOnDispose()
        {
            var sourceDisposedCount = 0;
            var sourceDisposingCount = 0;
            var targetDisposedCount = 0;
            var targetDisposingCount = 0;

            var source = new SynchronizedObservableCollection<TestCollectionItem>(ComponentCollectionManager);
            var target = GetCollection(source, false);

            source.AddComponent(new TestDisposableComponent<IReadOnlyObservableCollection>
            {
                OnDisposed = (_, _) => ++sourceDisposedCount,
                OnDisposing = (_, _) => ++sourceDisposingCount
            });
            target.AddComponent(new TestDisposableComponent<IReadOnlyObservableCollection>
            {
                OnDisposed = (_, _) => ++targetDisposedCount,
                OnDisposing = (_, _) => ++targetDisposingCount
            });

            source.Dispose();
            source.IsDisposed.ShouldBeTrue();
            target.IsDisposed.ShouldBeTrue();
            targetDisposedCount.ShouldEqual(1);
            targetDisposingCount.ShouldEqual(1);
            sourceDisposedCount.ShouldEqual(1);
            sourceDisposingCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TargetShouldDisposeSourceOnDispose(bool dispose)
        {
            var sourceDisposedCount = 0;
            var sourceDisposingCount = 0;
            var targetDisposedCount = 0;
            var targetDisposingCount = 0;

            var source = new SynchronizedObservableCollection<TestCollectionItem>(ComponentCollectionManager);
            var target = GetCollection(source, dispose);

            source.AddComponent(new TestDisposableComponent<IReadOnlyObservableCollection>
            {
                OnDisposed = (_, _) => ++sourceDisposedCount,
                OnDisposing = (_, _) => ++sourceDisposingCount
            });
            target.AddComponent(new TestDisposableComponent<IReadOnlyObservableCollection>
            {
                OnDisposed = (_, _) => ++targetDisposedCount,
                OnDisposing = (_, _) => ++targetDisposingCount
            });

            target.Dispose();
            target.IsDisposed.ShouldBeTrue();
            source.IsDisposed.ShouldEqual(dispose);
            targetDisposedCount.ShouldEqual(1);
            targetDisposingCount.ShouldEqual(1);
            sourceDisposedCount.ShouldEqual(dispose ? 1 : 0);
            sourceDisposingCount.ShouldEqual(dispose ? 1 : 0);
        }

        protected abstract IReadOnlyObservableCollection<T> GetCollection<T>(IReadOnlyObservableCollection<T> source, bool disposeSource);

        private WeakReference WeakTest(SynchronizedObservableCollection<object> source)
        {
            var collection = GetCollection(source, false);
            source.Add(NewId());
            collection.ShouldEqual(source);
            return new WeakReference(collection);
        }

        private void Assert(IReadOnlyObservableCollection<object> target, bool hasListener)
        {
            if (hasListener)
                _source.ShouldEqual(target);
            else
                target.ShouldBeEmpty();
        }
    }
}