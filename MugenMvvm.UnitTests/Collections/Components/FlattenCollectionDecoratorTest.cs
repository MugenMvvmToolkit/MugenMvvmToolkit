﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Collections.Internal;
using Xunit;

namespace MugenMvvm.UnitTests.Collections.Components
{
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
            _itemCollection2.AddComponent(new SortingCollectionDecorator(SortingComparer<object?>.Descending(i => (int) i!).Build()));

            _targetCollection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            _targetCollection.AddComponent(new FlattenCollectionDecorator(o => o is string ? null : o as IEnumerable));
            _tracker = new DecoratorObservableCollectionTracker<object>();
            _targetCollection.AddComponent(_tracker);
            _targetCollection.Add(_itemCollection1);
            _targetCollection.Add(_itemCollection2);
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

            for (var i = 0; i < 10; i++)
            {
                _itemCollection1.Move(i, i + 1);
                Assert();
                _targetCollection.Move(i, i + 1);
                Assert();
                _itemCollection2.Move(i, i + 1);
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
            _targetCollection.Reset(new[] {1, 2, 3, 4, 5});
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
            _targetCollection.Reset(new[] {1, 2, 3, 4, 5});
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

        [Fact]
        public void ShouldTrackChanges2()
        {
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

        [Fact(Skip = "DEBUG ONLY")]
        // [Fact]
        public void ShouldBeThreadSafe()
        {
            var random = new Random();
            Task.Run(async () =>
            {
                int index = 0;
                while (true)
                {
                    await Task.Delay(5);
                    _itemCollection1.Add(++index);
                }
            });
            Task.Run(async () =>
            {
                int index = 0;
                while (true)
                {
                    await Task.Delay(10);
                    _itemCollection2.Add(++index);
                }
            });
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(10);
                    _targetCollection[0] = random.Next() % 2 == 0 ? "" : _itemCollection1;
                    _targetCollection[1] = random.Next() % 2 == 0 ? "" : _itemCollection2;
                }
            });
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(3000);
                    _targetCollection.Add(_itemCollection1);
                    _targetCollection.Add(_itemCollection2);
                }
            });
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(2000);
                    var target = _targetCollection[_targetCollection.Count - 1];
                    _targetCollection.Remove(target);
                    var i = _itemCollection2[_itemCollection1.Count - 1];
                    _itemCollection2.Remove(i);
                }
            });
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    _itemCollection1.Reset(new[] {1, 2, 3});
                }
            });
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(100);
                    if (_itemCollection1.Count > 2)
                        _itemCollection1.Move(0, 1);
                    if (_targetCollection.Count > 4)
                        _targetCollection.Move(2, 3);
                }
            });
            Task.Run(async () =>
            {
                int index = 0;
                while (true)
                {
                    await Task.Delay(50);
                    _targetCollection.Add(++index);
                    using var l1 = _targetCollection.TryLock();
                    using var l2 = _itemCollection1.TryLock();
                    using var l3 = _itemCollection2.TryLock();
                    Assert();
                }
            }).Wait();
        }

        [Fact]
        public void ShouldTrackChanges3()
        {
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
        [InlineData(1)]
        [InlineData(2)]
        public void ShouldTrackAddChanges(int count)
        {
            _targetCollection.Clear();
            var t1 = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {"t1-T1", "t1-T2"};
            var t2 = new SynchronizedObservableCollection<object>(ComponentCollectionManager);

            for (int i = 0; i < count; i++)
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

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void ShouldTrackRemoveChanges(int count)
        {
            _targetCollection.Clear();
            var t1 = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {"t1-T1"};
            var t2 = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {"t2-T1", "t2-T2"};

            for (int i = 0; i < count; i++)
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
        [InlineData(1)]
        [InlineData(2)]
        public void ShouldTrackMoveChanges1(int count)
        {
            _targetCollection.Clear();
            var t1 = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {"t1-T1"};
            var t2 = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {"t2-T1", "t2-T2"};

            for (int i = 0; i < count; i++)
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
            _targetCollection.Clear();
            var t1 = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            var t2 = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            for (int i = 0; i < t1Count; i++)
                t1.Add("t1-T" + i);
            for (int i = 0; i < t2Count; i++)
                t2.Add("t2-T" + i);

            for (int i = 0; i < count; i++)
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
            _targetCollection.Clear();
            var t1 = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            var t2 = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            for (int i = 0; i < t1Count; i++)
                t1.Add("t1-T" + i);
            for (int i = 0; i < t2Count; i++)
                t2.Add("t2-T" + i);
            var t3 = new SynchronizedObservableCollection<object>(ComponentCollectionManager) {"t3-T1", "t3-T2", "t3-T3"};

            for (int i = 0; i < count; i++)
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

        private void Assert()
        {
            _targetCollection.Decorate().ShouldEqual(_tracker.ChangedItems);
            _tracker.ChangedItems.ShouldEqual(Decorate());
        }

        private IEnumerable<object?> Decorate()
        {
            foreach (var item in _targetCollection)
            {
                var enumerable = item is string ? null : item as IEnumerable;
                if (enumerable != null)
                {
                    foreach (var nestedItem in enumerable.Decorate())
                        yield return nestedItem;
                    continue;
                }

                yield return item;
            }
        }
    }
}