using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Collections.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class GroupCollectionDecoratorTest : CollectionDecoratorTestBase
    {
        private readonly SynchronizedObservableCollection<object?> _collection;
        private readonly DecoratedCollectionChangeTracker<object> _tracker;
        private readonly Dictionary<int, object> _headers;
        private readonly List<object> _headersIndex;
        private Func<object?, Optional<int>> _getKey;
        private Func<int, object>? _getHeader;
        private GroupCollectionDecorator<object?, int, object> _decorator;

        public GroupCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object?>(ComponentCollectionManager);
            _headers = new Dictionary<int, object>();
            _headersIndex = new List<object>();
            _tracker = new DecoratedCollectionChangeTracker<object>();
            _collection.AddComponent(_tracker);
            _getKey = o =>
            {
                if (o is int i)
                    return i % 4;
                if (o == null)
                    return -1;
                return default;
            };
            _getHeader = v =>
            {
                var value = _headers.GetOrAdd(v, i => _headers.GetOrAdd(i, k => k.ToString()));
                if (!_headersIndex.Contains(value))
                    _headersIndex.Add(value);
                return value;
            };
            _decorator = new GroupCollectionDecorator<object?, int, object>(0, true, _getKey, _getHeader, (k, group, _, action, _, _) =>
            {
                if (action == CollectionGroupChangedAction.GroupRemoved)
                {
                    _headers.Remove(k);
                    _headersIndex.Remove(group);
                }
            }, null, null);
            _collection.AddComponent(_decorator);
            _tracker.Changed += Assert;
        }

        [Fact]
        public void ChangeShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            for (var i = 0; i < _collection.Count; i++)
            {
                _collection.RaiseItemChanged(_collection[i]);
                Assert();
                _tracker.ItemChangedCount.ShouldEqual(i + 1);
            }
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

            var decorator = (ICollectionDecorator) _collection.GetComponent<GroupCollectionDecorator<object, int, object>>();

            var i = 0;
            foreach (var o in _collection.DecoratedItems())
            {
                var indexes = new ItemOrListEditor<int>();
                decorator.TryGetIndexes(_collection, _collection, o!, false, ref indexes).ShouldBeTrue();
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
        public override void ResetShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            _headers.Clear();
            _headersIndex.Clear();
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
        public override void ShouldTrackChanges()
        {
            var collection = GetCollection();
            for (var i = 0; i < 2; i++)
            {
                collection.Add(1);
                Assert();

                collection.Insert(1, 2);
                Assert();

                collection.Move(0, 1);
                Assert();

                collection.Move(1, 0);
                Assert();

                collection.Remove(2);
                Assert();

                collection.RemoveAt(0);
                Assert();

                collection.Reset(new object[] {1, 2, 3, 4, 5, i});
                Assert();

                collection[0] = 200;
                Assert();

                collection[3] = 3;
                Assert();

                collection.Move(0, collection.Count - 1);
                Assert();

                collection.Move(0, collection.Count - 2);
                Assert();

                collection[i] = i;
                Assert();
            }

            collection.Clear();
            Assert();
        }

        [Fact]
        public void ShouldTrackHeaderChanges()
        {
            var eventArgs = NewId();
            var header = new List<int>();
            var changedItems = new List<int>();
            _collection.RemoveComponent(_decorator);
            _getKey = o =>
            {
                if ((int) o! % 2 == 0)
                    return 0;
                return default;
            };
            _getHeader = o =>
            {
                o.ShouldEqual(0);
                return header;
            };
            _decorator = new GroupCollectionDecorator<object?, int, object>(0, false, _getKey, _getHeader, (k, h, groupItems, action, item, args) =>
            {
                k.ShouldEqual(0);
                var ints = (List<int>) h;
                switch (action)
                {
                    case CollectionGroupChangedAction.ItemAdded:
                        ints.Add((int) item!);
                        break;
                    case CollectionGroupChangedAction.ItemRemoved:
                        ints.Remove((int) item!);
                        break;
                    case CollectionGroupChangedAction.ItemChanged:
                        args.ShouldEqual(eventArgs);
                        changedItems.Add((int) item!);
                        break;
                    case CollectionGroupChangedAction.Reset:
                        ints.Reset(groupItems.Cast<int>());
                        break;
                    case CollectionGroupChangedAction.GroupRemoved:
                        ints.Clear();
                        break;
                }

                if (action != CollectionGroupChangedAction.GroupRemoved)
                    groupItems.Cast<int>().ShouldEqualUnordered(ints);
            }, null, null);
            _collection.AddComponent(_decorator);

            var expectedItems = _collection.Where(o => _getKey(o).HasValue).Cast<int>();
            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                header.ShouldEqualUnordered(expectedItems);
                Assert();

                _collection.RaiseItemChanged(i, eventArgs);
                changedItems.ShouldEqualUnordered(expectedItems);
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Insert(i, i);
                header.ShouldEqualUnordered(expectedItems);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.RemoveAt(i);
                header.ShouldEqualUnordered(expectedItems);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Move(i, i + 1);
                header.ShouldEqualUnordered(expectedItems);
                Assert();
            }

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i, i * 2 + i);
                header.ShouldEqualUnordered(expectedItems);
                Assert();
            }

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i * 2 + i, i);
                header.ShouldEqualUnordered(expectedItems);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection[i] = i + 101;
                header.ShouldEqualUnordered(expectedItems);
                Assert();
            }

            _collection.Reset(new object[] {1, 2, 3, 4, 5});
            header.ShouldEqualUnordered(expectedItems);
            Assert();

            _collection.Clear();
            header.ShouldEqualUnordered(expectedItems);
            Assert();
        }

        [Fact]
        public void ShouldTrackUnstableKeys()
        {
            _collection.RemoveComponent(_decorator);
            var eventArgs = NewId();
            var header = new List<UnstableKey>();
            var changedItems = new List<UnstableKey>();
            _getKey = o =>
            {
                if (((UnstableKey) o!).Id % 2 == 0)
                    return 0;
                return default;
            };
            _getHeader = o =>
            {
                o.ShouldEqual(0);
                return header;
            };
            _decorator = new GroupCollectionDecorator<object?, int, object>(0, false, _getKey, _getHeader, (k, h, groupItems, action, item, args) =>
            {
                k.ShouldEqual(0);
                var list = (List<UnstableKey>) h;
                switch (action)
                {
                    case CollectionGroupChangedAction.ItemAdded:
                        list.Add((UnstableKey) item!);
                        break;
                    case CollectionGroupChangedAction.ItemRemoved:
                        list.Remove((UnstableKey) item!);
                        break;
                    case CollectionGroupChangedAction.ItemChanged:
                        args.ShouldEqual(eventArgs);
                        changedItems.Add((UnstableKey) item!);
                        break;
                    case CollectionGroupChangedAction.GroupRemoved:
                        list.Clear();
                        break;
                    case CollectionGroupChangedAction.Reset:
                        list.Reset(groupItems.Cast<UnstableKey>());
                        break;
                }

                if (action != CollectionGroupChangedAction.GroupRemoved)
                    groupItems.Cast<UnstableKey>().ShouldEqualUnordered(list);
            }, null, null);
            _collection.AddComponent(_decorator);

            var expectedItems = _collection.Where(o => _getKey(o).HasValue).Cast<UnstableKey>();
            for (var i = 0; i < 100; i++)
            {
                var unstableKey = new UnstableKey(i);
                _collection.Add(unstableKey);
                header.ShouldEqualUnordered(expectedItems);
                Assert();

                _collection.RaiseItemChanged(unstableKey, eventArgs);
                changedItems.ShouldEqualUnordered(expectedItems);
            }

            foreach (UnstableKey? key in _collection)
            {
                key!.Id++;
                _collection.RaiseItemChanged(key, eventArgs);
                header.ShouldEqualUnordered(expectedItems);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Insert(i, new UnstableKey(i));
                header.ShouldEqualUnordered(expectedItems);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.RemoveAt(i);
                header.ShouldEqualUnordered(expectedItems);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Move(i, i + 1);
                header.ShouldEqualUnordered(expectedItems);
                Assert();
            }

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i, i * 2 + i);
                header.ShouldEqualUnordered(expectedItems);
                Assert();
            }

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i * 2 + i, i);
                header.ShouldEqualUnordered(expectedItems);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection[i] = new UnstableKey(i + 101);
                header.ShouldEqualUnordered(expectedItems);
                Assert();
            }

            _collection.Reset(new object[] {new UnstableKey(1), new UnstableKey(2), new UnstableKey(3), new UnstableKey(4), new UnstableKey(5)});
            Assert();

            foreach (UnstableKey? key in _collection)
            {
                key!.Id++;
                _collection.RaiseItemChanged(key);
                Assert();
            }

            _collection.Clear();
            Assert();
        }

        protected override void Assert()
        {
            var decorator = _collection.GetComponentOptional<GroupCollectionDecorator<object, int, object>>();
            _tracker.ChangedItems.ShouldEqual(_collection.DecoratedItems());
            var items = Decorate(decorator == _decorator ? _getKey : null, decorator == _decorator ? _getHeader : null);
            _tracker.ChangedItems.ShouldEqual(items);
        }

        protected override IObservableCollection<object?> GetCollection() => _collection;

        private IEnumerable<object?> Decorate(Func<object?, Optional<int>>? getKey, Func<int, object>? getHeader)
        {
            HashSet<object>? headers = null;
            if (getKey != null)
            {
                foreach (var item in _collection)
                {
                    var key = getKey(item);
                    if (!key.HasValue)
                        continue;

                    headers ??= new HashSet<object>();
                    headers.Add(getHeader!(key.Value));
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

        private int GetHeaderIndex(object header) => _headersIndex.IndexOf(header);

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