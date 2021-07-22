using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.UnitTests.Collections.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class HeaderFooterCollectionDecoratorTest : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<object> _collection;
        private readonly HeaderFooterCollectionDecorator _decorator;
        private readonly DecoratorObservableCollectionTracker<object> _tracker;

        public HeaderFooterCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            _decorator = new HeaderFooterCollectionDecorator();
            _collection.AddComponent(_decorator);
            _tracker = new DecoratorObservableCollectionTracker<object>();
            _collection.AddComponent(_tracker);
            _tracker.Changed += Assert;
        }

        [Fact]
        public void IndexOfShouldBeValid()
        {
            var header1 = NewId();
            var header2 = NewId();
            var footer1 = NewId();
            var footer2 = NewId();
            var targetItem1 = 1;
            var targetItem2 = 2;

            _collection.Add(targetItem1);
            _collection.Add(targetItem2);
            _decorator.Header = new[] {header1, header2};
            _decorator.Footer = new[] {footer1, footer2};

            ICollectionDecorator decorator = _decorator;
            int i = 0;
            foreach (var o in _collection.Decorate())
            {
                if (o is string header)
                {
                    decorator.TryGetIndex(_collection, _collection, header, out var index).ShouldBeTrue();
                    index.ShouldEqual(i);
                }

                ++i;
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void AddShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

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

        [Theory]
        [MemberData(nameof(GetData))]
        public void ChangeShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                Assert();
            }

            for (var i = 0; i < _collection.Count; i++)
            {
                _collection.RaiseItemChanged(_collection[i], null);
                Assert();
                _tracker.ItemChangedCount.ShouldEqual(i + 1);
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ClearShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

            for (var i = 0; i < 100; i++)
                _collection.Add(i);

            Assert();

            _collection.Clear();
            Assert();
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void MoveShouldTrackChanges1(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

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

        [Theory]
        [MemberData(nameof(GetData))]
        public void MoveShouldTrackChanges2(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

            for (var i = 0; i < 100; i++)
                _collection.Add(i);

            Assert();

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i, i * 2 + 1);
                Assert();
            }

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i * 2 + 1, i);
                Assert();
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void RemoveShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

            for (var i = 0; i < 100; i++)
                _collection.Add(i);

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
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ReplaceShouldTrackChanges1(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

            for (var i = 0; i < 100; i++)
                _collection.Add(i);

            Assert();

            for (var i = 0; i < 10; i++)
            {
                _collection[i] = i + 101;
                Assert();
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ReplaceShouldTrackChanges2(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

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

        [Theory]
        [MemberData(nameof(GetData))]
        public void ResetShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            _collection.Reset(new object[] {1, 2, 3, 4, 5});
            Assert();
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
                    _collection.Add(i);

                Assert();
                _decorator.Header = header;
                Assert();
                _decorator.Footer = footer;
                Assert();
                _decorator.Header = default;
                Assert();
                _decorator.Footer = default;
                Assert();

                _decorator.Header = header;
                _decorator.Footer = footer;
                Assert();

                _collection.RemoveComponent(_decorator);
                Assert();
                _decorator.Header = default;
                _decorator.Footer = default;
            }

            for (var i = 0; i < 2; i++)
            {
                _collection.AddComponent(_decorator);
                if (i != 0)
                    _collection.Add(i);

                Assert();
                _decorator.Footer = footer;
                Assert();
                _decorator.Header = header;
                Assert();
                _decorator.Footer = default;
                Assert();
                _decorator.Header = default;
                Assert();

                _decorator.Footer = footer;
                _decorator.Header = header;
                Assert();

                _collection.RemoveComponent(_decorator);
                Assert();
                _decorator.Header = default;
                _decorator.Footer = default;
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.Header = header;
            _decorator.Footer = footer;

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

            _decorator.Footer = header;
            _decorator.Header = footer;
            Assert();
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

        private void Assert()
        {
            var header = _decorator.Header;
            var footer = _decorator.Footer;
            var decorator = _collection.GetComponentOptional<HeaderFooterCollectionDecorator>();
            if (decorator == null)
            {
                header = default;
                footer = default;
            }

            _tracker.ChangedItems.ShouldEqual(Decorate(header, footer));
            _tracker.ChangedItems.ShouldEqual(_collection.Decorate());
        }

        private IEnumerable<object> Decorate(ItemOrIReadOnlyList<object> header, ItemOrIReadOnlyList<object> footer)
        {
            if (!header.IsEmpty)
            {
                foreach (var item in header)
                    yield return item;
            }

            foreach (var item in _collection)
                yield return item;
            if (!footer.IsEmpty)
            {
                foreach (var item in footer)
                    yield return item;
            }
        }
    }
}