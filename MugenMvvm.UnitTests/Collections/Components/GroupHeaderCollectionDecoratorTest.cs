using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.UnitTests.Collections.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class GroupHeaderCollectionDecoratorTest : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<object> _collection;
        private readonly DecoratorObservableCollectionTracker<object> _tracker;
        private readonly Dictionary<int, object> _headers;
        private Func<object?, object?>? _getHeader;
        private GroupHeaderCollectionDecorator<object, object> _decorator;

        public GroupHeaderCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            _headers = new Dictionary<int, object>();
            _tracker = new DecoratorObservableCollectionTracker<object>();
            _collection.AddComponent(_tracker);
            _getHeader = o => _headers.GetOrAdd((int) o! % 4, i => _headers.GetOrAdd(i, k => k.ToString()));
            _decorator = new GroupHeaderCollectionDecorator<object, object>(_getHeader);
            _collection.AddComponent(_decorator);
            _tracker.Changed += Assert;
        }

        [Fact]
        public void AddShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Insert(i, i);
                Assert();
            }
        }

        [Fact]
        public void ChangeShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            for (var i = 0; i < _collection.Count; i++)
            {
                _collection.RaiseItemChanged(_collection[i], null);
                Assert();
                _tracker.ItemChangedCount.ShouldEqual(i + 1);
            }
        }

        [Fact]
        public void ClearShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            _collection.Clear();
            Assert();
        }

        [Fact]
        public void IndexOfShouldBeValid()
        {
            var targetItem1 = 1;
            var targetItem2 = 2;
            var targetItem3 = 3;
            var targetItem4 = 4;

            _collection.Add(targetItem1);
            _collection.Add(targetItem2);
            _collection.Add(targetItem3);
            _collection.Add(targetItem4);

            var decorator = (ICollectionDecorator) _collection.GetComponent<GroupHeaderCollectionDecorator<object, object>>();

            var i = 0;
            foreach (var o in _collection.Decorate())
            {
                var indexes = new ItemOrListEditor<int>();
                decorator.TryGetIndexes(_collection, _collection, o!, ref indexes).ShouldBeTrue();
                if (o is string)
                {
                    indexes.Count.ShouldEqual(1);
                    indexes[0].ShouldEqual(i);
                }
                else
                    indexes.Count.ShouldEqual(0);

                ++i;
            }
        }

        [Fact]
        public void MoveShouldTrackChanges1()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            for (var i = 0; i < 10; i++)
            {
                _collection.Move(i, i + 1);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Move(i + 1, i);
                Assert();
            }
        }

        [Fact]
        public void MoveShouldTrackChanges2()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i, i * 2 + i);
                Assert();
            }

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i * 2 + i, i);
                Assert();
            }
        }

        [Fact]
        public void RemoveShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            for (var i = 0; i < 20; i++)
            {
                _collection.Remove(i);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.RemoveAt(i);
                Assert();
            }

            var count = _collection.Count;
            for (var i = 0; i < count; i++)
            {
                _collection.RemoveAt(0);
                Assert();
            }
        }

        [Fact]
        public void ReplaceShouldTrackChanges1()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            for (var i = 0; i < 10; i++)
            {
                _collection[i] = i + 101;
                Assert();
            }
        }

        [Fact]
        public void ReplaceShouldTrackChanges2()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            for (var i = 0; i < 10; i++)
            for (var j = 10; j < 20; j++)
            {
                _collection[i] = _collection[j];
                Assert();
            }
        }

        [Fact]
        public void ResetShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            _headers.Clear();
            _collection.Reset(new object[] {1, 2, 3, 4, 5});
            Assert();
        }

        [Fact]
        public void ShouldAttachDetach()
        {
            _collection.RemoveComponent(_decorator);

            for (var i = 0; i < 2; i++)
            {
                _collection.AddComponent(_decorator);
                if (i != 0)
                    _collection.Add(i);

                Assert();
                _collection.RemoveComponent(_decorator);
                Assert();
            }
        }

        [Fact]
        public void ShouldTrackChanges()
        {
            _collection.Add(1);
            Assert();

            _collection.Insert(1, 2);
            Assert();

            _collection.Move(0, 1);
            Assert();

            _collection.Remove(2);
            Assert();

            _collection.RemoveAt(0);
            Assert();

            _collection.Reset(new object[] {1, 2, 3, 4, 5});
            Assert();

            _collection[0] = 200;
            Assert();

            _collection.Clear();
            Assert();
        }

        [Fact]
        public void ShouldTrackHeaderChanges()
        {
            var eventArgs = NewId();
            var header = new List<int>();
            var changedItems = new List<int>();
            _collection.RemoveComponent(_decorator);
            _getHeader = o => (int) o! % 2 == 0 ? header : null;
            _decorator = new GroupHeaderCollectionDecorator<object, object>(_getHeader, (h, action, item, args) =>
            {
                var ints = (List<int>) h;
                if (action == GroupHeaderChangedAction.ItemAdded)
                    ints.Add((int) item!);
                else if (action == GroupHeaderChangedAction.ItemRemoved)
                    ints.Remove((int) item!);
                else if (action == GroupHeaderChangedAction.ItemChanged)
                {
                    args.ShouldEqual(eventArgs);
                    changedItems.Add((int) item!);
                }
                else if (action == GroupHeaderChangedAction.Clear)
                    ints.Clear();
            });
            _collection.AddComponent(_decorator);

            var expectedItems = _collection.Where(o => _getHeader(o) != null).Cast<int>().OrderBy(i => i);
            var items = header.OrderBy(i => i);
            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                items.ShouldEqual(expectedItems);
                Assert();

                _collection.RaiseItemChanged(i, eventArgs);
                changedItems.OrderBy(i => i).ShouldEqual(expectedItems);
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Insert(i, i);
                items.ShouldEqual(expectedItems);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.RemoveAt(i);
                items.ShouldEqual(expectedItems);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Move(i, i + 1);
                items.ShouldEqual(expectedItems);
                Assert();
            }

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i, i * 2 + i);
                items.ShouldEqual(expectedItems);
                Assert();
            }

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i * 2 + i, i);
                items.ShouldEqual(expectedItems);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection[i] = i + 101;
                items.ShouldEqual(expectedItems);
                Assert();
            }

            _collection.Reset(new object[] {1, 2, 3, 4, 5});
            items.ShouldEqual(expectedItems);
            Assert();

            _collection.Clear();
            items.ShouldEqual(expectedItems);
            Assert();
        }

        [Fact]
        public void ShouldTrackUnstableKeys()
        {
            _collection.RemoveComponent(_decorator);
            var eventArgs = NewId();
            var header = new List<UnstableKey>();
            var changedItems = new List<UnstableKey>();
            _getHeader = o => ((UnstableKey) o!).Id! % 2 == 0 ? header : null;
            _decorator = new GroupHeaderCollectionDecorator<object, object>(_getHeader, (h, action, item, args) =>
            {
                var ints = (List<UnstableKey>) h;
                if (action == GroupHeaderChangedAction.ItemAdded)
                    ints.Add((UnstableKey) item!);
                else if (action == GroupHeaderChangedAction.ItemRemoved)
                    ints.Remove((UnstableKey) item!);
                else if (action == GroupHeaderChangedAction.ItemChanged)
                {
                    args.ShouldEqual(eventArgs);
                    changedItems.Add((UnstableKey) item!);
                }
                else if (action == GroupHeaderChangedAction.Clear)
                    ints.Clear();
            }, null, false);
            _collection.AddComponent(_decorator);

            var expectedItems = _collection.Where(o => _getHeader(o) != null).Cast<UnstableKey>().OrderBy(i => i.Id).ThenBy(key => key.GetHashCode());
            var items = header.OrderBy(i => i.Id).ThenBy(key => key.GetHashCode());
            for (var i = 0; i < 100; i++)
            {
                var unstableKey = new UnstableKey(i);
                _collection.Add(unstableKey);
                items.ShouldEqual(expectedItems);
                Assert();

                _collection.RaiseItemChanged(unstableKey, eventArgs);
                changedItems.OrderBy(i => i.Id).ShouldEqual(expectedItems);
            }

            foreach (UnstableKey key in _collection)
            {
                key.Id++;
                _collection.RaiseItemChanged(key, eventArgs);
                items.ShouldEqual(expectedItems);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Insert(i, new UnstableKey(i));
                items.ShouldEqual(expectedItems);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.RemoveAt(i);
                items.ShouldEqual(expectedItems);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Move(i, i + 1);
                items.ShouldEqual(expectedItems);
                Assert();
            }

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i, i * 2 + i);
                items.ShouldEqual(expectedItems);
                Assert();
            }

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i * 2 + i, i);
                items.ShouldEqual(expectedItems);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection[i] = new UnstableKey(i + 101);
                items.ShouldEqual(expectedItems);
                Assert();
            }

            _collection.Reset(new object[] {new UnstableKey(1), new UnstableKey(2), new UnstableKey(3), new UnstableKey(4), new UnstableKey(5)});
            Assert();

            foreach (UnstableKey key in _collection)
            {
                key.Id++;
                _collection.RaiseItemChanged(key, null);
                Assert();
            }

            _collection.Clear();
            Assert();
        }

        private void Assert()
        {
            _tracker.ChangedItems.ShouldEqual(Decorate(_collection.GetComponentOptional<GroupHeaderCollectionDecorator<object, object>>() == _decorator ? _getHeader : null));
            _tracker.ChangedItems.ShouldEqual(_collection.Decorate());
        }

        private IEnumerable<object> Decorate(Func<object?, object?>? getHeader)
        {
            HashSet<object>? headers = null;
            if (getHeader != null)
            {
                foreach (var item in _collection)
                {
                    var header = getHeader(item);
                    if (header == null)
                        continue;

                    headers ??= new HashSet<object>();
                    headers.Add(header);
                }
            }

            if (headers != null)
            {
                foreach (var header in headers.OrderBy(GetHeaderIndex))
                    yield return header;
            }

            foreach (var item in _collection)
                yield return item;
        }

        private int GetHeaderIndex(object header)
        {
            var index = 0;
            foreach (var h in _headers)
            {
                if (h.Value == header)
                    return index;
                ++index;
            }

            return -1;
        }

        private sealed class UnstableKey
        {
            public UnstableKey(int id)
            {
                Id = id;
            }

            public int Id { get; set; }

            public override string ToString() => Id.ToString();
        }
    }
}