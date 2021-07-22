using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Collections;
using MugenMvvm.UnitTests.Collections.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Collections.Components
{
    [Collection(SharedContext)]
    public class FlattenCollectionDecoratorTest : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<int> _itemCollection1;
        private readonly SynchronizedObservableCollection<int> _itemCollection2;
        private readonly SynchronizedObservableCollection<object> _targetCollection;
        private readonly DecoratorObservableCollectionTracker<object> _tracker;

        public FlattenCollectionDecoratorTest()
        {
            _itemCollection1 = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            _itemCollection1.AddComponent(new FilterCollectionDecorator<int> {Filter = i => i % 2 == 0});
            _itemCollection2 = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            _itemCollection2.AddComponent(new SortingCollectionDecorator(SortingComparer<int>.Descending(i => i).Build()));

            _targetCollection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            _targetCollection.AddComponent(new FlattenCollectionDecorator<IEnumerable>(o => new FlattenItemInfo(o is string ? null : o, o != _itemCollection2)));
            _tracker = new DecoratorObservableCollectionTracker<object>();
            _targetCollection.AddComponent(_tracker);
            _targetCollection.Add(_itemCollection1);
            _targetCollection.Add(_itemCollection2);
            _tracker.Changed += Assert;
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
                _targetCollection.RaiseItemChanged(_targetCollection[i], null);
                _tracker.ItemChangedCount.ShouldEqual(0);
            }

            var raiseCount = 0;
            for (var i = 0; i < _itemCollection1.Count; i++)
            {
                _targetCollection.RaiseItemChanged(_targetCollection[i + offset], null);
                raiseCount += 1;
                _tracker.ItemChangedCount.ShouldEqual(raiseCount);

                _itemCollection1.RaiseItemChanged(_itemCollection1[i], null);
                raiseCount += _itemCollection1[i] % 2 == 0 ? 1 : 0;
                _tracker.ItemChangedCount.ShouldEqual(raiseCount);

                //ignore changes because we're listening source instead of decorators
                _itemCollection2.RaiseItemChanged(_itemCollection2[i], null);
                _tracker.ItemChangedCount.ShouldEqual(raiseCount);
            }

            Assert();
        }

        [Fact]
        public void ChangeShouldTrackUnstableItems()
        {
            var collection = new SynchronizedObservableCollection<UnstableCollection>(ComponentCollectionManager);
            collection.AddComponent(new FlattenCollectionDecorator<UnstableCollection>(c => new FlattenItemInfo(c.Items)));
            var tracker = new DecoratorObservableCollectionTracker<object>();
            var assert = new Action(() =>
            {
                collection.Decorate().ShouldEqual(tracker.ChangedItems);
                tracker.ChangedItems.ShouldEqual(Decorate(collection));
            });
            tracker.Changed += assert;
            collection.AddComponent(tracker);

            for (int i = 0; i < 20; i++)
                collection.Add(new UnstableCollection());

            int itemChangedCount = 0;
            for (int i = 0; i < collection.Count; i++)
            {
                if (i % 2 == 0)
                {
                    var items = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {i};
                    collection[i].Items = items;
                    collection.RaiseItemChanged(collection[i], null);
                    assert();
                    for (int j = 0; j < i; j++)
                    {
                        items.Add(j);
                        assert();
                    }
                }
                else
                {
                    collection.RaiseItemChanged(collection[i], null);
                    itemChangedCount++;
                }

                tracker.ItemChangedCount.ShouldEqual(itemChangedCount);
                assert();
            }

            for (int i = 0; i < collection.Count; i++)
            {
                collection.RaiseItemChanged(collection[i], null);
                if (collection[i].Items == null)
                    itemChangedCount++;
                tracker.ItemChangedCount.ShouldEqual(itemChangedCount);
            }

            for (int i = 0; i < collection.Count; i++)
            {
                if (i % 2 == 0)
                {
                    var items = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {i + 1000};
                    collection[i].Items = items;
                    collection.RaiseItemChanged(collection[i], null);
                    assert();
                    for (int j = 0; j < i; j++)
                    {
                        items.Add(j + 1000);
                        assert();
                    }
                }
                else
                {
                    collection.RaiseItemChanged(collection[i], null);
                    itemChangedCount++;
                }

                tracker.ItemChangedCount.ShouldEqual(itemChangedCount);
                assert();
            }

            for (int i = 0; i < collection.Count; i++)
            {
                if (i % 2 == 0)
                {
                    collection[i].Items = null;
                    collection.RaiseItemChanged(collection[i], null);
                    assert();
                }
                else
                {
                    collection.RaiseItemChanged(collection[i], null);
                    itemChangedCount++;
                }

                tracker.ItemChangedCount.ShouldEqual(itemChangedCount);
                assert();
            }
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
        public void IndexOfShouldBeValid()
        {
            _targetCollection.Clear();
            _targetCollection.RemoveComponents<FlattenCollectionDecorator>();
            _itemCollection1.RemoveComponents<SortingCollectionDecorator>();
            _itemCollection1.RemoveComponents<FilterCollectionDecorator<int>>();
            _itemCollection2.RemoveComponents<SortingCollectionDecorator>();
            _itemCollection2.RemoveComponents<FilterCollectionDecorator<int>>();
            _targetCollection.AddComponent(new FlattenCollectionDecorator<IEnumerable>(o => new FlattenItemInfo(o)));

            var targetItem1 = -100;
            var targetItem2 = -200;
            var source1Item1 = 100;
            var source1Item2 = 200;
            var source2Item1 = 300;
            var source2Item2 = 400;

            _targetCollection.Add(targetItem1);
            _targetCollection.Add(targetItem2);
            _targetCollection.Add(_itemCollection1);
            _targetCollection.Add(_itemCollection2);

            _itemCollection1.Add(source1Item1);
            _itemCollection1.Add(source1Item2);
            _itemCollection1.Add(source1Item2);

            _itemCollection2.Add(source2Item1);
            _itemCollection2.Add(source2Item2);
            _itemCollection2.Add(source2Item2);

            var decorator = (ICollectionDecorator) _targetCollection.GetComponent<FlattenCollectionDecorator>();
            var indexes = new ItemOrListEditor<int>();

            indexes.Clear();
            decorator.TryGetIndexes(_targetCollection, _targetCollection, source1Item1, ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(1);
            indexes[0].ShouldEqual(2);

            indexes.Clear();
            decorator.TryGetIndexes(_targetCollection, _targetCollection, source1Item2, ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(2);
            indexes[0].ShouldEqual(3);
            indexes[1].ShouldEqual(4);

            indexes.Clear();
            decorator.TryGetIndexes(_targetCollection, _targetCollection, source2Item1, ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(1);
            indexes[0].ShouldEqual(5);

            indexes.Clear();
            decorator.TryGetIndexes(_targetCollection, _targetCollection, source2Item2, ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(2);
            indexes[0].ShouldEqual(6);
            indexes[1].ShouldEqual(7);
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

            for (var i = 0; i < 10; i++)
            {
                _itemCollection1.Move(i, i + 1);
                Assert();
                _targetCollection.Move(i, i + 1);
                Assert();
                _itemCollection2.Move(i, i + 1);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _itemCollection1.Move(i + 1, i);
                Assert();
                _targetCollection.Move(i + 1, i);
                Assert();
                _itemCollection2.Move(i + 1, i);
                Assert();
            }
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

            for (var i = 0; i < 10; i++)
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

            for (var i = 0; i < 10; i++)
            for (var j = 10; j < 20; j++)
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
            root.AddComponent(new FlattenCollectionDecorator<IReadOnlyObservableCollection>(o => new FlattenItemInfo(o)));

            var tracker = new DecoratorObservableCollectionTracker<object>();
            tracker.Changed += () => tracker.ChangedItems.ShouldEqual(root.Decorate());
            root.AddComponent(tracker);

            var child1 = new SynchronizedObservableCollection<IReadOnlyObservableCollection>(ComponentCollectionManager);
            child1.AddComponent(new FlattenCollectionDecorator<IReadOnlyObservableCollection>(o => new FlattenItemInfo(o)));

            var child2 = new SynchronizedObservableCollection<SynchronizedObservableCollection<Guid>>(ComponentCollectionManager);
            child2.AddComponent(new FlattenCollectionDecorator<IReadOnlyObservableCollection>(o => new FlattenItemInfo(o)));

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

                    child.AddComponent(new FlattenCollectionDecorator<IReadOnlyObservableCollection>(o => new FlattenItemInfo(o)));
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
                    tracker.ChangedItems.ShouldEqual(root.Decorate());
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

        [Fact]
        public void ShouldHandleBatchUpdateFromChildDecorator()
        {
            var beginCount = 0;
            var endCount = 0;
            _targetCollection.AddComponent(new TestCollectionBatchUpdateListener
            {
                OnBeginBatchUpdate = (_, t) => beginCount += t == BatchUpdateType.Decorators ? 1 : 0,
                OnEndBatchUpdate = (_, t) => endCount += t == BatchUpdateType.Decorators ? 1 : 0
            });

            var t = _itemCollection1.GetComponent<ICollectionDecoratorManagerComponent>().BatchUpdate(_itemCollection1);
            beginCount.ShouldEqual(1);
            endCount.ShouldEqual(0);
            Assert();

            t.Dispose();
            beginCount.ShouldEqual(1);
            endCount.ShouldEqual(1);
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

            var t = _itemCollection1.BatchUpdate();
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

        private void Assert()
        {
            _targetCollection.Decorate().ShouldEqual(_tracker.ChangedItems);
            _tracker.ChangedItems.ShouldEqual(Decorate());
        }

        private void SetBatchLimit(int value)
        {
            foreach (var flattenCollectionDecorator in _targetCollection.GetComponents<FlattenCollectionDecorator>())
                flattenCollectionDecorator.BatchLimit = value;
            foreach (var flattenCollectionDecorator in _itemCollection1.GetComponents<FlattenCollectionDecorator>())
                flattenCollectionDecorator.BatchLimit = value;
            foreach (var flattenCollectionDecorator in _itemCollection2.GetComponents<FlattenCollectionDecorator>())
                flattenCollectionDecorator.BatchLimit = value;
        }

        private IEnumerable<object?> Decorate()
        {
            foreach (var item in _targetCollection)
            {
                var enumerable = item is string ? null : item as IEnumerable;
                if (enumerable != null)
                {
                    if (!ReferenceEquals(enumerable, _itemCollection2))
                        enumerable = enumerable.Decorate();

                    foreach (var nestedItem in enumerable)
                        yield return nestedItem;
                    continue;
                }

                yield return item;
            }
        }

        private IEnumerable<object?> Decorate(SynchronizedObservableCollection<UnstableCollection> collection)
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

        private sealed class UnstableCollection
        {
            public IList<object>? Items { get; set; }
        }
    }
}