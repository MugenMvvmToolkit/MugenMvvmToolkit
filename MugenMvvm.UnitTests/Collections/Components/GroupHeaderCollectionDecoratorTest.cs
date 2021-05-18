using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Collections.Internal;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class GroupHeaderCollectionDecoratorTest : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<object> _collection;
        private readonly List<object> _items;
        private readonly DecoratorObservableCollectionTracker<object> _tracker;
        private readonly Dictionary<int, object> _headers;
        private readonly Func<object?, object> _getHeader;
        private readonly GroupHeaderCollectionDecorator _decorator;

        public GroupHeaderCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            _headers = new Dictionary<int, object>();
            _items = new List<object>();
            _tracker = new DecoratorObservableCollectionTracker<object>();
            _collection.AddComponent(_tracker);
            _getHeader = o => _headers.GetOrAdd((int) o! % 4, i => _headers.GetOrAdd(i, k => k.ToString()));
            _decorator = new GroupHeaderCollectionDecorator(_getHeader);
            _collection.AddComponent(_decorator);
        }

        [Fact]
        public void AddShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                _items.Add(i);
                AssertChanges(_getHeader);
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Insert(i, i);
                _items.Insert(i, i);
                AssertChanges(_getHeader);
            }
        }

        [Fact]
        public void ClearShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                _items.Add(i);
            }

            AssertChanges(_getHeader);

            _collection.Clear();
            _items.Clear();
            AssertChanges(_getHeader);
        }

        [Fact]
        public void MoveShouldTrackChanges1()
        {
            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                _items.Add(i);
            }

            AssertChanges(_getHeader);

            for (var i = 0; i < 10; i++)
            {
                _collection.Move(i, i + 1);
                Move(i, i + 1);
                AssertChanges(_getHeader);
            }
        }

        [Fact]
        public void MoveShouldTrackChanges2()
        {
            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                _items.Add(i);
            }

            AssertChanges(_getHeader);

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i, i * 2 + i);
                Move(i, i * 2 + i);
                AssertChanges(_getHeader);
            }
        }

        [Fact]
        public void RemoveShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                _items.Add(i);
            }

            for (var i = 0; i < 20; i++)
            {
                _collection.Remove(i);
                _items.Remove(i);
                AssertChanges(_getHeader);
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.RemoveAt(i);
                _items.RemoveAt(i);
                AssertChanges(_getHeader);
            }

            var count = _collection.Count;
            for (var i = 0; i < count; i++)
            {
                _collection.RemoveAt(0);
                _items.RemoveAt(0);
                AssertChanges(_getHeader);
            }
        }

        [Fact]
        public void ReplaceShouldTrackChanges1()
        {
            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                _items.Add(i);
            }

            AssertChanges(_getHeader);

            for (var i = 0; i < 10; i++)
            {
                _collection[i] = i + 101;
                _items[i] = i + 101;
                AssertChanges(_getHeader);
            }
        }

        [Fact]
        public void ReplaceShouldTrackChanges2()
        {
            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                _items.Add(i);
            }

            AssertChanges(_getHeader);

            for (var i = 0; i < 10; i++)
            for (var j = 10; j < 20; j++)
            {
                _collection[i] = _collection[j];
                _items[i] = _items[j];
                AssertChanges(_getHeader);
            }
        }

        [Fact]
        public void ResetShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                _items.Add(i);
            }

            AssertChanges(_getHeader);

            _headers.Clear();
            _collection.Reset(new object[] {1, 2, 3, 4, 5});
            _items.Clear();
            _items.AddRange(_collection);
            AssertChanges(_getHeader);
        }

        [Fact]
        public void ShouldAttachDetach()
        {
            _collection.RemoveComponent(_decorator);

            for (var i = 0; i < 2; i++)
            {
                _collection.AddComponent(_decorator);
                if (i != 0)
                {
                    _collection.Add(i);
                    _items.Add(i);
                }

                AssertChanges(_getHeader);
                _collection.RemoveComponent(_decorator);
                AssertChanges(null);
            }
        }

        [Fact]
        public void ShouldTrackChanges()
        {
            _collection.Add(1);
            _items.Add(1);
            AssertChanges(_getHeader);

            _collection.Insert(1, 2);
            _items.Insert(1, 2);
            AssertChanges(_getHeader);

            _collection.Move(0, 1);
            Move(0, 1);
            AssertChanges(_getHeader);

            _collection.Remove(2);
            _items.Remove(2);
            AssertChanges(_getHeader);

            _collection.RemoveAt(0);
            _items.RemoveAt(0);
            AssertChanges(_getHeader);

            _collection.Reset(new object[] {1, 2, 3, 4, 5});
            _items.Clear();
            _items.AddRange(_collection);
            AssertChanges(_getHeader);

            _collection[0] = 200;
            _items[0] = 200;
            AssertChanges(_getHeader);

            _collection.Clear();
            _items.Clear();
            AssertChanges(_getHeader);
        }

        private void AssertChanges(Func<object?, object>? getHeader)
        {
            _tracker.ChangedItems.ShouldEqual(Decorate(getHeader));
            _tracker.ChangedItems.ShouldEqual(_collection.Decorate());
        }

        private IEnumerable<object> Decorate(Func<object?, object>? getHeader)
        {
            HashSet<object>? headers = null;
            if (getHeader != null)
            {
                foreach (var item in _items)
                {
                    var header = getHeader(item);
                    headers ??= new HashSet<object>();
                    headers.Add(header);
                }
            }

            if (headers != null)
            {
                foreach (var header in headers.OrderBy(GetHeaderIndex))
                    yield return header;
            }

            foreach (var item in _items)
                yield return item;
        }

        private int GetHeaderIndex(object header)
        {
            int index = 0;
            foreach (var h in _headers)
            {
                if (h.Value == header)
                    return index;
                ++index;
            }

            return -1;
        }

        private void Move(int oldIndex, int newIndex)
        {
            var obj = _items[oldIndex];
            _items.RemoveAt(oldIndex);
            _items.Insert(newIndex, obj);
        }
    }
}