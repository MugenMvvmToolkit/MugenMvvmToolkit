using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Internal;
using MugenMvvm.Models;
using MugenMvvm.Tests.Collections;
using MugenMvvm.UnitTests.Collections.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Collections.Components
{
    [Collection(SharedContext)]
    public class FlattenCollectionDecoratorTest : UnitTestBase
    {
        private static readonly int[] NullValue = {int.MaxValue, int.MinValue};

        private readonly SynchronizedObservableCollection<int> _itemCollection1;
        private readonly SynchronizedObservableCollection<int> _itemCollection2;
        private readonly SynchronizedObservableCollection<object> _targetCollection;
        private readonly DecoratedCollectionChangeTracker<object> _tracker;

        public FlattenCollectionDecoratorTest()
        {
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
            _itemCollection1 = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            _itemCollection1.AddComponent(new FilterCollectionDecorator<int>(0, false) {Filter = (i, _) => i % 2 == 0});
            _itemCollection2 = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            _itemCollection2.AddComponent(new SortCollectionDecorator<object>(0, o => o, SortingComparerBuilder.Get<int>().Descending(i => i).Build().AsObjectComparer()));

            _targetCollection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            _targetCollection.AddComponent(new FlattenCollectionDecorator<IEnumerable?>(0, true,
                (o, currentInfo) =>
                {
                    var result = o == null ? new FlattenItemInfo(NullValue, false) : new FlattenItemInfo(o is string ? null : o, o != _itemCollection2);
                    if (!currentInfo.IsEmpty(null))
                        result.ShouldEqual(currentInfo);
                    return result;
                },
                (item, items) =>
                {
                    if (item == null)
                        items.ShouldEqual(NullValue, Default.ReferenceEqualityComparer);
                    else
                        item.ShouldEqual(items, Default.ReferenceEqualityComparer);
                }));
            _tracker = new DecoratedCollectionChangeTracker<object>();
            _tracker.Changed += Assert;
            _targetCollection.AddComponent(_tracker);
            _targetCollection.Add(_itemCollection1);
            _targetCollection.Add(_itemCollection2);
            _targetCollection.Add(null!);
        }

        [Fact]
        public void AddShouldTrackChanges()
        {
            for (var i = 0; i < 10; i++)
            {
                _itemCollection1.Add(i);
                Assert();
                _targetCollection.Add(i);
                Assert();
                _itemCollection2.Add(i);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _itemCollection1.Insert(i, i);
                Assert();
                _targetCollection.Insert(i, i);
                Assert();
                _itemCollection2.Insert(i, i);
                Assert();
            }
        }

        [Fact]
        public void ChangeShouldTrackChanges()
        {
            var offset = _targetCollection.Count;
            for (var i = 0; i < 100; i++)
            {
                _itemCollection1.Add(i);
                _targetCollection.Add(i);
                _itemCollection2.Add(i);
            }

            Assert();

            for (var i = 0; i < offset; i++)
            {
                _targetCollection.RaiseItemChanged(_targetCollection[i]);
                _tracker.ItemChangedCount.ShouldEqual(0);
            }

            var raiseCount = 0;
            for (var i = 0; i < _itemCollection1.Count; i++)
            {
                _targetCollection.RaiseItemChanged(_targetCollection[i + offset]);
                raiseCount += 1;
                _tracker.ItemChangedCount.ShouldEqual(raiseCount);

                _itemCollection1.RaiseItemChanged(_itemCollection1[i]);
                raiseCount += _itemCollection1[i] % 2 == 0 ? 1 : 0;
                Assert();
                _tracker.ItemChangedCount.ShouldEqual(raiseCount);

                //ignore changes because we're listening source instead of decorators
                _itemCollection2.RaiseItemChanged(_itemCollection2[i]);
                _tracker.ItemChangedCount.ShouldEqual(raiseCount);
            }

            Assert();
        }

        [Fact]
        public void ChangeShouldTrackUnstableItems1()
        {
            var collection = new SynchronizedObservableCollection<UnstableCollection>(ComponentCollectionManager);
            collection.AddComponent(new FlattenCollectionDecorator<UnstableCollection>(0, false, (c, _) => new FlattenItemInfo(c.Items, true), null));
            var tracker = new DecoratedCollectionChangeTracker<object>();
            var assert = new Action(() =>
            {
                collection.DecoratedItems().ShouldEqual(tracker.ChangedItems);
                tracker.ChangedItems.ShouldEqual(Decorate(collection));
            });
            tracker.Changed += assert;
            collection.AddComponent(tracker);

            for (var i = 0; i < 20; i++)
                collection.Add(new UnstableCollection());

            var itemChangedCount = 0;
            for (var i = 0; i < collection.Count; i++)
            {
                if (i % 2 == 0)
                {
                    var items = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {i};
                    collection[i].Items = items;
                    collection.RaiseItemChanged(collection[i]);
                    assert();
                    for (var j = 0; j < i; j++)
                    {
                        items.Add(j);
                        assert();
                    }
                }
                else
                {
                    collection.RaiseItemChanged(collection[i]);
                    itemChangedCount++;
                }

                tracker.ItemChangedCount.ShouldEqual(itemChangedCount);
                assert();
            }

            for (var i = 0; i < collection.Count; i++)
            {
                collection.RaiseItemChanged(collection[i]);
                if (collection[i].Items == null)
                    itemChangedCount++;
                tracker.ItemChangedCount.ShouldEqual(itemChangedCount);
            }

            for (var i = 0; i < collection.Count; i++)
            {
                if (i % 2 == 0)
                {
                    var items = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {i + 1000};
                    collection[i].Items = items;
                    collection.RaiseItemChanged(collection[i]);
                    assert();
                    for (var j = 0; j < i; j++)
                    {
                        items.Add(j + 1000);
                        assert();
                    }
                }
                else
                {
                    collection.RaiseItemChanged(collection[i]);
                    itemChangedCount++;
                }

                tracker.ItemChangedCount.ShouldEqual(itemChangedCount);
                assert();
            }

            for (var i = 0; i < collection.Count; i++)
            {
                if (i % 2 == 0)
                {
                    collection[i].Items = null;
                    collection.RaiseItemChanged(collection[i]);
                    assert();
                }
                else
                {
                    collection.RaiseItemChanged(collection[i]);
                    itemChangedCount++;
                }

                tracker.ItemChangedCount.ShouldEqual(itemChangedCount);
                assert();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ChangeShouldTrackUnstableItems2(bool decoratedItems)
        {
            var collection = new SynchronizedObservableCollection<UnstableCollection>(ComponentCollectionManager);
            collection.AddComponent(new FlattenCollectionDecorator<UnstableCollection>(0, false, (c, _) => new FlattenItemInfo(c.Items, decoratedItems), null));
            var tracker = new DecoratedCollectionChangeTracker<object>();
            var assert = new Action(() =>
            {
                collection.DecoratedItems().ShouldEqual(tracker.ChangedItems);
                tracker.ChangedItems.ShouldEqual(Decorate(collection));
            });
            tracker.Changed += assert;
            collection.AddComponent(tracker);
            var unstableCollection = new UnstableCollection();
            collection.Add(unstableCollection);

            var items1 = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            for (int i = 0; i < 10; i++)
                items1.Add(i.ToString());

            unstableCollection.Items = items1;
            collection.RaiseItemChanged(unstableCollection);
            assert();

            unstableCollection.Items = Array.Empty<object>();
            collection.RaiseItemChanged(unstableCollection);
            assert();

            var t = items1.BatchUpdate();
            items1.Clear();
            unstableCollection.Items = items1;
            collection.RaiseItemChanged(unstableCollection);

            t.Dispose();
            assert();
        }

        [Fact]
        public void ClearShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
            {
                _itemCollection1.Add(i);
                _targetCollection.Insert(0, i);
                _itemCollection2.Add(i);
            }

            Assert();

            _itemCollection1.Clear();
            Assert();
            _itemCollection2.Clear();
            Assert();
            _targetCollection.Clear();
            Assert();
        }

        [Fact]
        public void MoveShouldTrackChanges1()
        {
            for (var i = 0; i < 100; i++)
            {
                _itemCollection1.Add(i);
                _targetCollection.Insert(0, i);
                _itemCollection2.Add(i);
                _targetCollection.Add(i);
            }

            Assert();

            for (var i = 0; i < _itemCollection1.Count - 1; i++)
            {
                _itemCollection1.Move(i, i + 1);
                Assert();
                _targetCollection.Move(i, i + 1);
                Assert();
                _itemCollection2.Move(i, i + 1);
                Assert();
            }

            for (var i = 0; i < _itemCollection1.Count - 1; i++)
            {
                _itemCollection1.Move(i + 1, i);
                Assert();
                _targetCollection.Move(i + 1, i);
                Assert();
                _itemCollection2.Move(i + 1, i);
                Assert();
            }

            _itemCollection1.Move(0, _itemCollection1.Count - 1);
            Assert();

            _itemCollection2.Move(0, _itemCollection2.Count - 1);
            Assert();

            _targetCollection.Move(0, _targetCollection.Count - 1);
            Assert();

            _itemCollection1.Move(_itemCollection1.Count - 1, 0);
            Assert();

            _itemCollection2.Move(_itemCollection2.Count - 1, 0);
            Assert();

            _targetCollection.Move(_targetCollection.Count - 1, 0);
            Assert();
        }

        [Fact]
        public void MoveShouldTrackChanges2()
        {
            for (var i = 0; i < 100; i++)
            {
                _itemCollection1.Add(i);
                _targetCollection.Insert(0, i);
                _itemCollection2.Add(i);
                _targetCollection.Add(i);
            }

            Assert();

            for (var i = 0; i < 10; i++)
            {
                _itemCollection1.Move(i, i * 2 + 1);
                Assert();
                _targetCollection.Move(i, i * 2 + 1);
                Assert();
                _itemCollection2.Move(i, i * 2 + 1);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _itemCollection1.Move(i * 2 + 1, i);
                Assert();
                _targetCollection.Move(i * 2 + 1, i);
                Assert();
                _itemCollection2.Move(i * 2 + 1, i);
                Assert();
            }
        }

        [Fact]
        public void RemoveShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
            {
                _itemCollection1.Add(i);
                _targetCollection.Add(i);
                _itemCollection2.Add(i);
            }

            Assert();

            for (var i = 0; i < 20; i++)
            {
                _itemCollection1.Remove(i);
                Assert();
                _targetCollection.Remove(i);
                Assert();
                _itemCollection2.Remove(i);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _itemCollection1.RemoveAt(i);
                Assert();
                _itemCollection2.RemoveAt(i);
                Assert();
                _targetCollection.RemoveAt(i);
                Assert();
            }

            var count = _itemCollection1.Count;
            for (var i = 0; i < count; i++)
            {
                _itemCollection1.RemoveAt(0);
                Assert();
                _itemCollection2.RemoveAt(0);
                Assert();
                _targetCollection.RemoveAt(0);
                Assert();
            }
        }

        [Fact]
        public void ReplaceShouldTrackChanges1()
        {
            for (var i = 0; i < 100; i++)
            {
                _itemCollection1.Add(i);
                _targetCollection.Add(i);
                _itemCollection2.Add(i);
            }

            Assert();

            for (var i = 0; i < _itemCollection1.Count; i++)
            {
                _itemCollection1[i] = i + 101;
                Assert();
                _targetCollection[i] = i + 101;
                Assert();
                _itemCollection2[i] = i + 101;
                Assert();
            }
        }

        [Fact]
        public void ReplaceShouldTrackChanges2()
        {
            for (var i = 0; i < 100; i++)
            {
                _itemCollection1.Add(i);
                _targetCollection.Add(i);
                _itemCollection2.Add(i);
            }

            Assert();

            for (var i = 0; i < _itemCollection1.Count / 2; i++)
            for (var j = _itemCollection1.Count / 2; j < _itemCollection1.Count; j++)
            {
                _itemCollection1[i] = _itemCollection1[j];
                Assert();
                _targetCollection[i] = _targetCollection[j];
                Assert();
                _itemCollection2[i] = _itemCollection2[j];
                Assert();
            }
        }

        [Fact]
        public void ResetShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
            {
                _itemCollection1.Add(i);
                _targetCollection.Add(i);
                _itemCollection2.Add(i);
            }

            Assert();

            _itemCollection1.Reset(new[] {1, 2, 3, 4, 5});
            Assert();
            _itemCollection2.Reset(new[] {1, 2, 3, 4, 5});
            Assert();
            _targetCollection.Reset(new object[] {1, 2, 3, 4, 5});
            Assert();
        }

        [Theory]
        [InlineData(LongRunningTimeout)]
        public async Task ShouldBeThreadSafe1(int timeout)
        {
            var cts = new CancellationTokenSource();
            var root = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            root.AddComponent(new FlattenCollectionDecorator<IReadOnlyObservableCollection>(0, false, (o, _) => new FlattenItemInfo(o, true), null));

            var tracker = new DecoratedCollectionChangeTracker<object>();
            tracker.Changed += () => tracker.ChangedItems.ShouldEqual(root.DecoratedItems());
            root.AddComponent(tracker);

            var child1 = new SynchronizedObservableCollection<IReadOnlyObservableCollection>(ComponentCollectionManager);
            child1.AddComponent(new FlattenCollectionDecorator<IReadOnlyObservableCollection>(0, false, (o, _) => new FlattenItemInfo(o, true), null));

            var child2 = new SynchronizedObservableCollection<SynchronizedObservableCollection<Guid>>(ComponentCollectionManager);
            child2.AddComponent(new FlattenCollectionDecorator<IReadOnlyObservableCollection>(0, false, (o, _) => new FlattenItemInfo(o, true), null));

            var nestedChild = new SynchronizedObservableCollection<Guid>(ComponentCollectionManager);
            child1.Add(nestedChild);
            child2.Add(nestedChild);

            DateTime startDate = default;

            bool IsCancellationRequested()
            {
                if (DateTime.Now - startDate > TimeSpan.FromSeconds(timeout))
                    cts.Cancel();
                return cts.IsCancellationRequested;
            }

            var t1 = Task.Run(() =>
            {
                while (true)
                {
                    if (IsCancellationRequested())
                        return;
                    Thread.Sleep(10);
                    nestedChild.Add(Guid.NewGuid());
                }
            });

            var t2 = Task.Run(() =>
            {
                while (true)
                {
                    if (IsCancellationRequested())
                        return;
                    Thread.Sleep(10);
                    if (root.Contains(child1))
                        root.Remove(child1);
                    else
                        root.Add(child1);
                }
            });

            var t3 = Task.Run(() =>
            {
                while (true)
                {
                    if (IsCancellationRequested())
                        return;
                    Thread.Sleep(10);
                    if (root.Contains(child2))
                        root.Remove(child2);
                    else
                        root.Add(child2);
                }
            });

            var t4 = Task.Run(() =>
            {
                var index = 0;
                while (true)
                {
                    if (IsCancellationRequested())
                        return;

                    if (index == 1000)
                        return;

                    Thread.Sleep(100);

                    var child = new SynchronizedObservableCollection<SynchronizedObservableCollection<Guid>>(ComponentCollectionManager);
                    var nestedChild1 = new SynchronizedObservableCollection<Guid>(ComponentCollectionManager);

                    Task.Run(() =>
                    {
                        while (true)
                        {
                            if (IsCancellationRequested())
                                return;
                            nestedChild1.Add(Guid.NewGuid());
                            Thread.Sleep(10);
                        }
                    });

                    child.AddComponent(new FlattenCollectionDecorator<IReadOnlyObservableCollection>(0, false, (o, _) => new FlattenItemInfo(o, true), null));
                    child.Add(nestedChild1);

                    if (index % 2 == 0)
                        Thread.Sleep(100);

                    root.Add(child);

                    ++index;
                }
            });

            var t5 = Task.Run(() =>
            {
                while (true)
                {
                    if (IsCancellationRequested())
                        return;

                    Thread.Sleep(1000);
                    using var l = root.Lock();
                    tracker.ChangedItems.ShouldEqual(root.DecoratedItems());
                }
            });
            cts.CancelAfter(TimeSpan.FromSeconds(timeout));
            startDate = DateTime.Now;
            await Task.WhenAll(t1, t2, t3, t4, t5);
            Assert();
        }

        [Theory]
        [InlineData(LongRunningTimeout)]
        public async Task ShouldBeThreadSafe2(int timeout)
        {
            var cts = new CancellationTokenSource();
            _targetCollection.Add("T");
            var random = new Random();

            DateTime startDate = default;

            bool IsCancellationRequested()
            {
                if (DateTime.Now - startDate > TimeSpan.FromSeconds(timeout))
                    cts.Cancel();
                return cts.IsCancellationRequested;
            }

            var t1 = Task.Run(() =>
            {
                var index = 0;
                while (true)
                {
                    if (IsCancellationRequested())
                        return;
                    Thread.Sleep(5);
                    _itemCollection1.Add(++index);
                }
            });
            var t2 = Task.Run(() =>
            {
                var index = 0;
                while (true)
                {
                    if (IsCancellationRequested())
                        return;
                    Thread.Sleep(10);
                    using var t = _targetCollection.BatchUpdate(BatchUpdateType.Decorators);
                    _itemCollection2.Add(++index);
                }
            });
            var t3 = Task.Run(() =>
            {
                while (true)
                {
                    if (IsCancellationRequested())
                        return;
                    Thread.Sleep(10);
                    _targetCollection[0] = random.Next() % 2 == 0 ? "" : _itemCollection1;
                    _targetCollection[1] = random.Next() % 2 == 0 ? "" : _itemCollection2;
                    if (_targetCollection.Count > 2)
                        _targetCollection[2] = random.Next() % 2 == 0 ? "" : new SynchronizedObservableCollection<int>(ComponentCollectionManager) {int.MaxValue, int.MinValue};
                }
            });
            var t4 = Task.Run(() =>
            {
                while (true)
                {
                    if (IsCancellationRequested())
                        return;
                    Thread.Sleep(3000);
                    _targetCollection.Add(_itemCollection1);
                    _targetCollection.Add(_itemCollection2);
                }
            });
            var t5 = Task.Run(() =>
            {
                while (true)
                {
                    if (IsCancellationRequested())
                        return;
                    Thread.Sleep(2000);
                    var target = _targetCollection[_targetCollection.Count - 1];
                    _targetCollection.Remove(target);
                    using var l = _itemCollection2.Lock();
                    using var t = _targetCollection.BatchUpdate(BatchUpdateType.Decorators);
                    var i = _itemCollection2[_itemCollection2.Count - 1];
                    _itemCollection2.Remove(i);
                }
            });
            var t6 = Task.Run(() =>
            {
                while (true)
                {
                    if (IsCancellationRequested())
                        return;
                    Thread.Sleep(1000);
                    _itemCollection1.Reset(new[] {1, 2, 3});
                }
            });
            var t7 = Task.Run(() =>
            {
                while (true)
                {
                    if (IsCancellationRequested())
                        return;
                    Thread.Sleep(100);
                    if (_itemCollection1.Count > 2)
                        _itemCollection1.Move(0, 1);
                    if (_targetCollection.Count > 4)
                        _targetCollection.Move(2, 3);
                }
            });
            var t8 = Task.Run(() =>
            {
                var index = 0;
                while (true)
                {
                    if (IsCancellationRequested())
                        return;
                    Thread.Sleep(50);
                    _targetCollection.Add(++index);
                    using var l1 = _targetCollection.Lock();
                    using var l2 = _itemCollection1.Lock();
                    using var l3 = _itemCollection2.Lock();
                    Assert();
                }
            });
            cts.CancelAfter(TimeSpan.FromSeconds(timeout));
            startDate = DateTime.Now;
            await Task.WhenAll(t1, t2, t3, t4, t5, t6, t7, t8);
            Assert();
        }

        [Theory]
        [InlineData(LongRunningTimeout, true)]
        [InlineData(LongRunningTimeout, false)]
        public async Task ShouldBeThreadSafe3(int timeout, bool decorator)
        {
            var cts = new CancellationTokenSource();
            var targetCollection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            var tracker = new DecoratedCollectionChangeTracker<object>();
            targetCollection.AddComponent(tracker);
            targetCollection.ConfigureDecorators<ThreadSafeItem>()
                            .AutoRefreshOnPropertyChanged(new[]
                            {
                                nameof(ThreadSafeItem.Items),
                                nameof(ThreadSafeItem.IsVisible)
                            })
                            .Where(item => item.IsVisible)
                            .SelectMany(item => item.Items);
            DateTime startDate = default;

            bool IsCancellationRequested()
            {
                if (DateTime.Now - startDate > TimeSpan.FromSeconds(timeout))
                    cts.Cancel();
                return cts.IsCancellationRequested;
            }

            const int poolCount = 100000;
            var items = new List<ThreadSafeItem>();
            for (int i = 0; i < poolCount; i++)
                items.Add(new ThreadSafeItem());

            var t1 = Task.Run(() =>
            {
                var index = 0;
                while (index < poolCount)
                {
                    if (IsCancellationRequested())
                        return;
                    Thread.Sleep(5);
                    if (decorator)
                        targetCollection.AddComponent(new HeaderFooterCollectionDecorator(index + 1000).SetHeader(items[index]));
                    else
                        targetCollection.Add(items[index]);
                    ++index;
                }
            });
            var t2 = Task.Run(() =>
            {
                var index = 0;
                while (index < poolCount)
                {
                    if (IsCancellationRequested())
                        return;
                    Thread.Sleep(10);
                    var item = items[index];
                    item.Items = new SynchronizedObservableCollection<object>(ComponentCollectionManager)
                                 .ConfigureDecorators()
                                 .Count(i => item.IsVisible = i > 0)
                                 .CastCollectionToSynchronized();
                    item.Items.Add(index);
                    ++index;
                }
            });

            cts.CancelAfter(TimeSpan.FromSeconds(timeout));
            startDate = DateTime.Now;
            await Task.WhenAll(t1, t2);
            targetCollection.DecoratedItems().ShouldEqual(tracker.ChangedItems);
        }

        [Fact(Skip = ReleaseTest)]
        public void ShouldBeWeak()
        {
            var collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            var weakReference = WeakTest(collection);
            GcCollect();
            GcCollect();
            GcCollect();
            collection.Add(NewId());
            weakReference.IsAlive.ShouldBeFalse();
        }

        [Fact]
        public void ShouldHandleBatchUpdateFromChildDecorator1()
        {
            var beginCount = 0;
            var endCount = 0;
            _targetCollection.AddComponent(new TestCollectionBatchUpdateListener
            {
                OnBeginBatchUpdate = (_, t) => beginCount += t == BatchUpdateType.Decorators ? 1 : 0,
                OnEndBatchUpdate = (_, t) => endCount += t == BatchUpdateType.Decorators ? 1 : 0
            });

            var t = _itemCollection1.BatchUpdate(BatchUpdateType.Decorators);
            beginCount.ShouldEqual(1);
            endCount.ShouldEqual(0);
            Assert();

            t.Dispose();
            beginCount.ShouldEqual(1);
            endCount.ShouldEqual(1);
            Assert();
        }

        [Fact]
        public void ShouldHandleBatchUpdateFromChildDecorator2()
        {
            _targetCollection.Clear();
            _targetCollection.Add(_itemCollection2);
            var t = _itemCollection2.BatchUpdate();
            Assert();

            _itemCollection2.Add(0);
            _itemCollection2.Insert(0, 1);
            _itemCollection2.Insert(1, 2);
            _itemCollection2.Remove(2);
            _itemCollection2[0] = -1;
            _itemCollection2.Move(0, 1);
            _tracker.ChangedItems.ShouldBeEmpty();
            _targetCollection.DecoratedItems().ShouldBeEmpty();

            t.Dispose();
            Assert();
        }

        [Fact]
        public void ShouldNotHandleBatchUpdateFromChildSource()
        {
            var beginCount = 0;
            var endCount = 0;
            _targetCollection.AddComponent(new TestCollectionBatchUpdateListener
            {
                OnBeginBatchUpdate = (_, t) => beginCount += t == BatchUpdateType.Decorators ? 1 : 0,
                OnEndBatchUpdate = (_, t) => endCount += t == BatchUpdateType.Decorators ? 1 : 0
            });

            var t = _itemCollection1.BatchUpdate(BatchUpdateType.Source);
            beginCount.ShouldEqual(1);
            endCount.ShouldEqual(0);
            Assert();

            t.Dispose();
            beginCount.ShouldEqual(1);
            endCount.ShouldEqual(1);
            Assert();
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(1, int.MaxValue)]
        [InlineData(2, int.MaxValue)]
        public void ShouldTrackAddChanges(int count, int batchLimit)
        {
            SetBatchLimit(batchLimit);
            _targetCollection.Clear();
            var t1 = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {"t1-T1", "t1-T2"};
            var t2 = new SynchronizedObservableCollection<object>(ComponentCollectionManager);

            for (var i = 0; i < count; i++)
            {
                _targetCollection.Add("T" + i);
                Assert();
                _targetCollection.Add(t1);
                Assert();
                _targetCollection.Add(t2);
                Assert();
            }

            t2.Add("t1-T2");
            Assert();
            t2.Insert(0, "t1-T2");
            Assert();

            _targetCollection.Insert(0, "T3");
            Assert();

            t1.Insert(0, "t1-T3");
            Assert();

            t1.Add("t1-T4");
            Assert();
        }

        [Fact]
        public void ShouldTrackChanges1()
        {
            _itemCollection1.Add(1);
            Assert();
            _targetCollection.Add(1);
            Assert();

            _itemCollection1.Insert(1, 2);
            Assert();
            _targetCollection.Insert(1, 2);
            Assert();

            _itemCollection1.Move(0, 1);
            Assert();
            _targetCollection.Move(0, 1);
            Assert();

            _itemCollection1.Remove(2);
            Assert();
            _targetCollection.Remove(2);
            Assert();

            _itemCollection1.RemoveAt(0);
            Assert();
            _targetCollection.RemoveAt(0);
            Assert();

            _itemCollection1.Reset(new[] {1, 2, 3, 4, 5});
            Assert();
            _targetCollection.Reset(new object[] {1, 2, 3, 4, 5});
            Assert();

            _itemCollection1[0] = 200;
            Assert();
            _targetCollection[0] = 200;
            Assert();

            _itemCollection1.Clear();
            Assert();
            _targetCollection.Clear();
            Assert();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void ShouldTrackChanges2(int batchLimit)
        {
            SetBatchLimit(batchLimit);

            _itemCollection1.Add(1);
            Assert();
            _targetCollection.Add(1);
            Assert();

            _itemCollection1.Insert(1, 2);
            Assert();
            _targetCollection.Insert(1, 2);
            Assert();

            _itemCollection1.Remove(2);
            Assert();
            _targetCollection.Remove(2);
            Assert();

            _itemCollection1.RemoveAt(0);
            Assert();
            _targetCollection.RemoveAt(0);
            Assert();

            _itemCollection1.Reset(new[] {1, 2, 3, 4, 5});
            Assert();
            _targetCollection.Reset(new object[] {1, 2, 3, 4, 5});
            Assert();

            _itemCollection1[0] = 200;
            Assert();
            _targetCollection[0] = 200;
            Assert();

            _itemCollection1.Move(1, 2);
            Assert();
            _targetCollection.Move(1, 2);
            Assert();

            _itemCollection1.Clear();
            Assert();
            _targetCollection.Clear();
            Assert();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void ShouldTrackChanges3(int batchLimit)
        {
            SetBatchLimit(batchLimit);

            _targetCollection.Clear();
            var t1 = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            var t2 = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            var t3 = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {"t3-T1", "t3-T2"};

            _targetCollection.Add("T1");
            _targetCollection.Add(t1);
            _targetCollection.Add(t2);
            _targetCollection.Add("T2");
            _targetCollection.Add(t1);
            _targetCollection.Add(t2);
            Assert();

            t1.Add("t1-T1");
            Assert();

            t1.Add("t1-T2");
            Assert();

            t1.Insert(0, "t1-T0");
            Assert();

            t2.Add("t2-T1");
            Assert();

            t2.Add("t2-T2");
            Assert();

            t1.RemoveAt(1);
            Assert();

            t2.Add("t2-T3");
            Assert();

            t1[0] = "t1-T1'";
            Assert();

            t1.Move(0, 1);
            Assert();

            t1.Clear();
            Assert();

            t1.Reset(new[] {"T1-T1"});
            Assert();

            _targetCollection.Clear();
            _targetCollection.Add(t1);
            _targetCollection.Add(t2);
            Assert();

            _targetCollection[0] = t3;
            Assert();

            _targetCollection[1] = t3;
            Assert();

            _targetCollection[0] = t1;
            Assert();

            _targetCollection[1] = t2;
            Assert();

            _targetCollection[0] = "T1";
            Assert();

            _targetCollection[1] = "T2";
            Assert();

            _targetCollection[0] = t1;
            Assert();

            _targetCollection[1] = t2;
            Assert();

            _targetCollection.Add("T1");
            _targetCollection.Add("T2");
            _targetCollection.Insert(1, new[] {"c0-T1", "c0-T2"});
            Assert();

            _targetCollection.RemoveAt(1);
            Assert();

            _targetCollection.Add("T3");
            Assert();

            _targetCollection.Insert(1, new[] {"c0-T1", "c0-T2"});
            Assert();

            _targetCollection.Insert(2, new[] {"c1-T1", "c1-T2"});
            Assert();

            _targetCollection[2] = "T4";
            Assert();

            _targetCollection[2] = new[] {"c1-T1", "c1-T2"};
            Assert();

            _targetCollection[2] = new[] {"c2-T1", "c2-T2"};
            Assert();

            _targetCollection.Reset(new object[] {"T1", new[] {"c0-T1"}, new[] {"c1-T1"}, new string[0], "T2", new[] {"c2-T1", "c2-T2"}});
            Assert();

            _targetCollection.Move(5, 0);
            Assert();

            _targetCollection[1] = "T2'";
            Assert();
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(1, int.MaxValue)]
        [InlineData(2, int.MaxValue)]
        public void ShouldTrackMoveChanges1(int count, int batchLimit)
        {
            SetBatchLimit(batchLimit);
            _targetCollection.Clear();
            var t1 = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {"t1-T1"};
            var t2 = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {"t2-T1", "t2-T2"};

            for (var i = 0; i < count; i++)
            {
                _targetCollection.Add("T1");
                _targetCollection.Add(t1);
                _targetCollection.Add("T3");
                _targetCollection.Add(t2);
                _targetCollection.Add("T2");
                Assert();
            }

            _targetCollection.Move(1, 3);
            Assert();

            _targetCollection.Move(3, 1);
            Assert();

            _targetCollection.Move(0, 2);
            Assert();

            _targetCollection.Move(2, 0);
            Assert();

            _targetCollection.Move(0, 1);
            Assert();

            _targetCollection.Move(1, 0);
            Assert();

            t2.Move(0, 1);
            Assert();
        }

        [Theory]
        [InlineData(1, 0, 0)]
        [InlineData(1, 0, 1)]
        [InlineData(1, 1, 1)]
        [InlineData(1, 1, 2)]
        [InlineData(1, 1, 3)]
        [InlineData(1, 2, 1)]
        [InlineData(1, 3, 1)]
        [InlineData(1, 3, 0)]
        [InlineData(2, 0, 1)]
        [InlineData(2, 1, 1)]
        [InlineData(2, 1, 2)]
        [InlineData(2, 1, 3)]
        [InlineData(2, 2, 1)]
        [InlineData(2, 3, 1)]
        [InlineData(2, 3, 0)]
        [InlineData(1, 3, 3)]
        [InlineData(1, 0, 3)]
        [InlineData(1, 3, 2)]
        [InlineData(1, 2, 3)]
        public void ShouldTrackMoveChanges2(int count, int t1Count, int t2Count)
        {
            foreach (var l in new[] {1, int.MaxValue})
            {
                SetBatchLimit(l);
                _targetCollection.Clear();
                var t1 = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
                var t2 = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
                for (var i = 0; i < t1Count; i++)
                    t1.Add("t1-T" + i);
                for (var i = 0; i < t2Count; i++)
                    t2.Add("t2-T" + i);

                for (var i = 0; i < count; i++)
                {
                    _targetCollection.Add("T1");
                    _targetCollection.Add(t1);
                    _targetCollection.Add("T3");
                    _targetCollection.Add(t2);
                    _targetCollection.Add("T2");
                    Assert();
                }

                _targetCollection.Move(1, 3);
                Assert();

                _targetCollection.Move(3, 1);
                Assert();

                _targetCollection.Move(0, 2);
                Assert();

                _targetCollection.Move(2, 0);
                Assert();

                _targetCollection.Move(0, 1);
                Assert();

                _targetCollection.Move(1, 0);
                Assert();

                _targetCollection.Move(3, 0);
                Assert();

                _targetCollection.Move(0, 3);
                Assert();

                _targetCollection.Move(1, 2);
                Assert();

                _targetCollection.Move(2, 1);
                Assert();

                _targetCollection.Move(3, 4);
                Assert();

                _targetCollection.Move(4, 3);
                Assert();

                _targetCollection.Move(4, 3);
                Assert();

                _targetCollection.Move(3, 4);
                Assert();
            }
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(1, int.MaxValue)]
        [InlineData(2, int.MaxValue)]
        public void ShouldTrackRemoveChanges(int count, int batchLimit)
        {
            SetBatchLimit(batchLimit);
            _targetCollection.Clear();
            var t1 = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {"t1-T1"};
            var t2 = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {"t2-T1", "t2-T2"};

            for (var i = 0; i < count; i++)
            {
                _targetCollection.Add("T1");
                _targetCollection.Add(t1);
                _targetCollection.Add("T3");
                _targetCollection.Add(t2);
                _targetCollection.Add("T2");
                Assert();
            }

            t2.RemoveAt(0);
            Assert();

            t1.RemoveAt(0);
            Assert();

            _targetCollection.RemoveAt(0);
            Assert();

            _targetCollection.RemoveAt(_targetCollection.Count - 1);
            Assert();

            t2.RemoveAt(0);
            Assert();
        }

        [Theory]
        [InlineData(1, 0, 0)]
        [InlineData(1, 0, 1)]
        [InlineData(1, 1, 1)]
        [InlineData(1, 1, 2)]
        [InlineData(1, 1, 3)]
        [InlineData(1, 2, 1)]
        [InlineData(1, 3, 1)]
        [InlineData(1, 3, 0)]
        [InlineData(2, 0, 1)]
        [InlineData(2, 1, 1)]
        [InlineData(2, 1, 2)]
        [InlineData(2, 1, 3)]
        [InlineData(2, 2, 1)]
        [InlineData(2, 3, 1)]
        [InlineData(2, 3, 0)]
        public void ShouldTrackReplaceChanges(int count, int t1Count, int t2Count)
        {
            foreach (var l in new[] {1, int.MaxValue})
            {
                SetBatchLimit(l);
                _targetCollection.Clear();
                var t1 = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
                var t2 = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
                for (var i = 0; i < t1Count; i++)
                    t1.Add("t1-T" + i);
                for (var i = 0; i < t2Count; i++)
                    t2.Add("t2-T" + i);
                var t3 = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {"t3-T1", "t3-T2", "t3-T3"};

                for (var i = 0; i < count; i++)
                {
                    _targetCollection.Add("T1");
                    _targetCollection.Add(t1);
                    _targetCollection.Add("T3");
                    _targetCollection.Add(t2);
                    _targetCollection.Add("T2");
                    Assert();
                }

                _targetCollection[0] = "T1'";
                Assert();

                _targetCollection[4] = "T2'";
                Assert();

                _targetCollection[1] = t3;
                Assert();

                _targetCollection[3] = t3;
                Assert();

                _targetCollection[1] = t1;
                Assert();

                _targetCollection[3] = t2;
                Assert();

                _targetCollection[0] = t3;
                Assert();

                _targetCollection[4] = t3;
                Assert();

                _targetCollection[0] = "T1";
                Assert();

                _targetCollection[4] = "T2";
                Assert();
            }
        }

        private WeakReference WeakTest(SynchronizedObservableCollection<object> target)
        {
            var collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            collection.AddComponent(new FlattenCollectionDecorator<SynchronizedObservableCollection<object>>(0, false, (o, _) => new FlattenItemInfo(o, true), null));
            collection.Add(target);

            target.Add(NewId());
            collection.DecoratedItems().ShouldEqual(target);
            return new WeakReference(collection);
        }

        private void Assert()
        {
            _targetCollection.Components.Get<IHasPendingNotifications>().Raise(null);
            _itemCollection1.Components.Get<IHasPendingNotifications>().Raise(null);
            _itemCollection2.Components.Get<IHasPendingNotifications>().Raise(null);
            _targetCollection.DecoratedItems().ShouldEqual(_tracker.ChangedItems);
            _tracker.ChangedItems.ShouldEqual(Decorate());
        }

        private void SetBatchLimit(int value)
        {
            foreach (var flattenCollectionDecorator in _targetCollection.GetComponents<FlattenCollectionDecorator>())
                flattenCollectionDecorator.BatchThreshold = value;
            foreach (var flattenCollectionDecorator in _itemCollection1.GetComponents<FlattenCollectionDecorator>())
                flattenCollectionDecorator.BatchThreshold = value;
            foreach (var flattenCollectionDecorator in _itemCollection2.GetComponents<FlattenCollectionDecorator>())
                flattenCollectionDecorator.BatchThreshold = value;
        }

        private IEnumerable<object?> Decorate()
        {
            foreach (var item in _targetCollection)
            {
                var enumerable = item is string ? null : item as IEnumerable;
                if (enumerable == null && item == null)
                    enumerable = NullValue;
                if (enumerable != null)
                {
                    if (!ReferenceEquals(enumerable, _itemCollection2))
                        enumerable = enumerable.DecoratedItems();

                    foreach (var nestedItem in enumerable)
                        yield return nestedItem;
                    continue;
                }

                yield return item;
            }
        }

        private static IEnumerable<object?> Decorate(SynchronizedObservableCollection<UnstableCollection> collection)
        {
            foreach (var item in collection)
            {
                if (item.Items != null)
                {
                    foreach (var nestedItem in item.Items)
                        yield return nestedItem;
                    continue;
                }

                yield return item;
            }
        }

        private sealed class ThreadSafeItem : NotifyPropertyChangedBase
        {
            private bool _isVisible;
            private IList<object>? _items;

            public bool IsVisible
            {
                get => _isVisible;
                set
                {
                    if (value == _isVisible) return;
                    _isVisible = value;
                    OnPropertyChanged();
                }
            }

            public IList<object>? Items
            {
                get => _items;
                set
                {
                    if (Equals(value, _items)) return;
                    _items = value;
                    OnPropertyChanged();
                }
            }
        }

        private sealed class UnstableCollection
        {
            public IList<object>? Items { get; set; }
        }
    }
}