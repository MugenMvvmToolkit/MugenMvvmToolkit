using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Collections.Internal;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class HeaderFooterCollectionDecoratorTest : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<object> _collection;
        private readonly HeaderFooterCollectionDecorator _decorator;
        private readonly List<object> _items;
        private readonly DecoratorObservableCollectionTracker<object> _tracker;

        public HeaderFooterCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            _decorator = new HeaderFooterCollectionDecorator();
            _collection.AddComponent(_decorator);
            _items = new List<object>();
            _tracker = new DecoratorObservableCollectionTracker<object>();
            _collection.AddComponent(_tracker);
        }

        private static IEnumerable<object?[]> GetData() =>
            new[]
            {
                new object?[] {null, null},
                new object?[] {new[] {"header"}, null},
                new object?[] {new[] {"header1", "header2"}, null},
                new object?[] {null, new[] {"footer"}},
                new object?[] {null, new[] {"footer1", "footer2"}},
                new object?[] {new[] {"header"}, new[] {"footer"}},
                new object?[] {new[] {"header1", "header2"}, new[] {"footer"}},
                new object?[] {new[] {"header"}, new[] {"footer1", "footer2"}},
                new object?[] {new[] {"header1", "header2"}, new[] {"footer1", "footer2"}}
            };

        [Theory]
        [MemberData(nameof(GetData))]
        public void AddShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                _items.Add(i);
                AssertChanges(header, footer);
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Insert(i, i);
                _items.Insert(i, i);
                AssertChanges(header, footer);
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ClearShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                _items.Add(i);
            }

            AssertChanges(header, footer);

            _collection.Clear();
            _items.Clear();
            AssertChanges(header, footer);
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void MoveShouldTrackChanges1(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                _items.Add(i);
            }

            AssertChanges(header, footer);

            for (var i = 0; i < 10; i++)
            {
                _collection.Move(i, i + 1);
                Move(i, i + 1);
                AssertChanges(header, footer);
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void MoveShouldTrackChanges2(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                _items.Add(i);
            }

            AssertChanges(header, footer);

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i, i + i);
                Move(i, i + i);
                AssertChanges(header, footer);
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void RemoveShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                _items.Add(i);
            }

            for (var i = 0; i < 20; i++)
            {
                _collection.Remove(i);
                _items.Remove(i);
                AssertChanges(header, footer);
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.RemoveAt(i);
                _items.RemoveAt(i);
                AssertChanges(header, footer);
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ReplaceShouldTrackChanges1(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                _items.Add(i);
            }

            AssertChanges(header, footer);

            for (var i = 0; i < 10; i++)
            {
                _collection[i] = i + 101;
                _items[i] = i + 101;
                AssertChanges(header, footer);
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ReplaceShouldTrackChanges2(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                _items.Add(i);
            }

            AssertChanges(header, footer);

            for (var i = 0; i < 10; i++)
            for (var j = 10; j < 20; j++)
            {
                _collection[i] = _collection[j];
                _items[i] = _items[j];
                AssertChanges(header, footer);
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ResetShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                _items.Add(i);
            }

            AssertChanges(header, footer);

            _collection.Reset(new object[] {1, 2, 3, 4, 5});
            _items.Clear();
            _items.AddRange(_collection);
            AssertChanges(header, footer);
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

            _collection.Add(1);
            _items.Add(1);
            AssertChanges(header, footer);

            _collection.Insert(1, 2);
            _items.Insert(1, 2);
            AssertChanges(header, footer);

            _collection.Move(0, 1);
            Move(0, 1);
            AssertChanges(header, footer);

            _collection.Remove(2);
            _items.Remove(2);
            AssertChanges(header, footer);

            _collection.RemoveAt(0);
            _items.RemoveAt(0);
            AssertChanges(header, footer);

            _collection.Reset(new object[] {1, 2, 3, 4, 5});
            _items.Clear();
            _items.AddRange(_collection);
            AssertChanges(header, footer);

            _collection[0] = 200;
            _items[0] = 200;
            AssertChanges(header, footer);

            _collection.Clear();
            _items.Clear();
            AssertChanges(header, footer);

            _decorator.Footer = header;
            _decorator.Header = footer;
            AssertChanges(footer, header);
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ShouldAttachDetach(string[]? header, string[]? footer)
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

                AssertChanges(null, null);
                _decorator.Header = header;
                AssertChanges(header, null);
                _decorator.Footer = footer;
                AssertChanges(header, footer);
                _decorator.Header = null;
                AssertChanges(null, footer);
                _decorator.Footer = null;
                AssertChanges(null, null);

                _decorator.Header = header;
                _decorator.Footer = footer;
                AssertChanges(header, footer);

                _collection.RemoveComponent(_decorator);
                AssertChanges(null, null);
                _decorator.Header = null;
                _decorator.Footer = null;
            }

            for (var i = 0; i < 2; i++)
            {
                _collection.AddComponent(_decorator);
                if (i != 0)
                {
                    _collection.Add(i);
                    _items.Add(i);
                }

                AssertChanges(null, null);
                _decorator.Footer = footer;
                AssertChanges(null, footer);
                _decorator.Header = header;
                AssertChanges(header, footer);
                _decorator.Footer = null;
                AssertChanges(header, null);
                _decorator.Header = null;
                AssertChanges(null, null);

                _decorator.Footer = footer;
                _decorator.Header = header;
                AssertChanges(header, footer);

                _collection.RemoveComponent(_decorator);
                AssertChanges(null, null);
                _decorator.Header = null;
                _decorator.Footer = null;
            }
        }

        private void AssertChanges(string[]? header, string[]? footer)
        {
            _tracker.ChangedItems.ShouldEqual(Decorate(header, footer));
            _tracker.ChangedItems.ShouldEqual(_collection.DecorateItems());
        }

        private IEnumerable<object> Decorate(string[]? header, string[]? footer)
        {
            if (header != null)
            {
                foreach (var item in header)
                    yield return item;
            }

            foreach (var item in _items)
                yield return item;
            if (footer != null)
            {
                foreach (var item in footer)
                    yield return item;
            }
        }

        private void Move(int oldIndex, int newIndex)
        {
            var obj = _items[oldIndex];
            _items.RemoveAt(oldIndex);
            _items.Insert(newIndex, obj);
        }
    }
}