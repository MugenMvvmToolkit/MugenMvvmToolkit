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
        private readonly SynchronizedObservableCollection<object?> _collection;
        private readonly HeaderFooterCollectionDecorator _decorator;
        private readonly DecoratedCollectionChangeTracker<object> _tracker;

        public HeaderFooterCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object?>(ComponentCollectionManager);
            _decorator = new HeaderFooterCollectionDecorator(0);
            _collection.AddComponent(_decorator);
            _tracker = new DecoratedCollectionChangeTracker<object>();
            _collection.AddComponent(_tracker);
            _tracker.Changed += Assert;
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void AddShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.SetHeader(header).SetFooter(footer);
            CollectionDecoratorTestBase.AddShouldTrackChangesImpl(_collection, Assert);
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ChangeShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.SetHeader(header).SetFooter(footer);
            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                Assert();
            }

            for (var i = 0; i < _collection.Count; i++)
            {
                _collection.RaiseItemChanged(_collection[i]);
                Assert();
                _tracker.ItemChangedCount.ShouldEqual(i + 1);
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ClearShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.SetHeader(header).SetFooter(footer);
            CollectionDecoratorTestBase.ClearShouldTrackChangesImpl(_collection, Assert);
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
            _decorator.SetHeader(new[] {header1, header2, header2}).SetFooter(new[] {footer1, footer2, footer2});

            ICollectionDecorator decorator = _decorator;
            var indexes = new ItemOrListEditor<int>();

            indexes.Clear();
            decorator.TryGetIndexes(_collection, _collection, header1, false, ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(1);
            indexes[0].ShouldEqual(0);

            indexes.Clear();
            decorator.TryGetIndexes(_collection, _collection, header2, false, ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(2);
            indexes[0].ShouldEqual(1);
            indexes[1].ShouldEqual(2);

            indexes.Clear();
            decorator.TryGetIndexes(_collection, _collection, footer1, false, ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(1);
            indexes[0].ShouldEqual(5);

            indexes.Clear();
            decorator.TryGetIndexes(_collection, _collection, footer2, false, ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(2);
            indexes[0].ShouldEqual(6);
            indexes[1].ShouldEqual(7);
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void MoveShouldTrackChanges1(string[]? header, string[]? footer)
        {
            _decorator.SetHeader(header).SetFooter(footer);
            CollectionDecoratorTestBase.MoveShouldTrackChanges1Impl(_collection, Assert);
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void MoveShouldTrackChanges2(string[]? header, string[]? footer)
        {
            _decorator.SetHeader(header).SetFooter(footer);
            CollectionDecoratorTestBase.MoveShouldTrackChanges2Impl(_collection, Assert);
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void RemoveShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.SetHeader(header).SetFooter(footer);
            CollectionDecoratorTestBase.RemoveShouldTrackChangesImpl(_collection, Assert);
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ReplaceShouldTrackChanges1(string[]? header, string[]? footer)
        {
            _decorator.SetHeader(header).SetFooter(footer);
            CollectionDecoratorTestBase.ReplaceShouldTrackChanges1Impl(_collection, Assert);
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ReplaceShouldTrackChanges2(string[]? header, string[]? footer)
        {
            _decorator.SetHeader(header).SetFooter(footer);
            CollectionDecoratorTestBase.ReplaceShouldTrackChanges2Impl(_collection, Assert);
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ResetShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.SetHeader(header).SetFooter(footer);
            CollectionDecoratorTestBase.ResetShouldTrackChangesImpl(_collection, Assert);
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
                _decorator.SetHeader(header);
                Assert();
                _decorator.SetFooter(footer);
                Assert();
                _decorator.SetHeader(default);
                Assert();
                _decorator.SetFooter(default);
                Assert();

                _decorator.SetHeader(header);
                _decorator.SetFooter(footer);
                Assert();

                _collection.RemoveComponent(_decorator);
                Assert();
                _decorator.SetHeader(default);
                _decorator.SetFooter(default);
            }

            for (var i = 0; i < 2; i++)
            {
                _collection.AddComponent(_decorator);
                if (i != 0)
                    _collection.Add(i);

                Assert();
                _decorator.SetFooter(footer);
                Assert();
                _decorator.SetHeader(header);
                Assert();
                _decorator.SetFooter(default);
                Assert();
                _decorator.SetHeader(default);
                Assert();

                _decorator.SetFooter(footer);
                _decorator.SetHeader(header);
                Assert();

                _collection.RemoveComponent(_decorator);
                Assert();
                _decorator.SetHeader(default);
                _decorator.SetFooter(default);
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ShouldTrackChanges(string[]? header, string[]? footer)
        {
            _decorator.SetHeader(header).SetFooter(footer);
            CollectionDecoratorTestBase.ShouldTrackChangesImpl(_collection, Assert);

            _decorator.SetFooter(header).SetHeader(footer);
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

            _tracker.ChangedItems.ShouldEqual(Decorate(ItemOrIReadOnlyList.FromList(header), ItemOrIReadOnlyList.FromList(footer)));
            _tracker.ChangedItems.ShouldEqual(_collection.DecoratedItems());
        }

        private IEnumerable<object?> Decorate(ItemOrIReadOnlyList<object> header, ItemOrIReadOnlyList<object> footer)
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