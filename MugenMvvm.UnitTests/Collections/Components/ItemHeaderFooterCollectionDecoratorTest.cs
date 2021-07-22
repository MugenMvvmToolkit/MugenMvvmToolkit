using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Collections.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class ItemHeaderFooterCollectionDecoratorTest : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<object> _collection;
        private readonly DecoratorObservableCollectionTracker<object> _tracker;
        private readonly Func<object?, bool?> _isHeaderOrFooter;
        private IComparer<object?>? _headerComparer;
        private IComparer<object?>? _footerComparer;

        public ItemHeaderFooterCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            _tracker = new DecoratorObservableCollectionTracker<object>();
            _collection.AddComponent(_tracker);
            _isHeaderOrFooter = o =>
            {
                var i = (int) o!;
                if (i % 2 == 0)
                    return true;
                if (i % 3 == 0)
                    return false;
                return null;
            };
            _tracker.Changed += Assert;
        }

        [Fact]
        public void ChangeShouldTrackUnstableItems()
        {
            var item1 = "Item1";
            var item2 = "Item2";
            var item3 = "Item3";
            var item4 = "Item4";
            var headers = new List<string>();
            var footers = new List<string>();
            var collection = new SynchronizedObservableCollection<string>(ComponentCollectionManager);
            var isHeaderOrFooter = new Func<string, bool?>(s =>
            {
                if (headers.Contains(s))
                    return true;
                if (footers.Contains(s))
                    return false;
                return null;
            });
            collection.AddComponent(new ItemHeaderFooterCollectionDecorator<string>(isHeaderOrFooter));

            var tracker = new DecoratorObservableCollectionTracker<object>();
            var assert = new Action(() =>
            {
                collection.Decorate().ShouldEqual(tracker.ChangedItems);
                tracker.ChangedItems.ShouldEqual(Decorate(collection, isHeaderOrFooter));
            });
            tracker.Changed += assert;
            collection.AddComponent(tracker);
            collection.Reset(new[] {item1, item2, item3, item4});

            int itemChangedCount = 0;

            headers.Add(item2);
            collection.RaiseItemChanged(item2, null);
            assert();
            tracker.ItemChangedCount.ShouldEqual(itemChangedCount);

            headers.Remove(item2);
            footers.Add(item2);
            collection.RaiseItemChanged(item2, null);
            assert();
            tracker.ItemChangedCount.ShouldEqual(itemChangedCount);

            footers.Remove(item2);
            collection.RaiseItemChanged(item2, null);
            assert();
            tracker.ItemChangedCount.ShouldEqual(itemChangedCount);

            headers.Add(item1);
            collection.RaiseItemChanged(item1, null);
            ++itemChangedCount;
            assert();
            tracker.ItemChangedCount.ShouldEqual(itemChangedCount);

            footers.Add(item4);
            collection.RaiseItemChanged(item4, null);
            ++itemChangedCount;
            assert();
            tracker.ItemChangedCount.ShouldEqual(itemChangedCount);

            headers.Remove(item1);
            collection.RaiseItemChanged(item1, null);
            ++itemChangedCount;
            assert();
            tracker.ItemChangedCount.ShouldEqual(itemChangedCount);

            footers.Remove(item4);
            collection.RaiseItemChanged(item4, null);
            ++itemChangedCount;
            assert();
            tracker.ItemChangedCount.ShouldEqual(itemChangedCount);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddShouldTrackChanges(bool defaultComparer)
        {
            AddDecorator(defaultComparer);

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
        [InlineData(true)]
        [InlineData(false)]
        public void ChangeShouldTrackChanges(bool defaultComparer)
        {
            AddDecorator(defaultComparer);

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
        [InlineData(true)]
        [InlineData(false)]
        public void ClearShouldTrackChanges(bool defaultComparer)
        {
            AddDecorator(defaultComparer);

            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            _collection.Clear();
            Assert();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MoveShouldTrackChanges1(bool defaultComparer)
        {
            AddDecorator(defaultComparer);

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
        [InlineData(true)]
        [InlineData(false)]
        public void MoveShouldTrackChanges2(bool defaultComparer)
        {
            AddDecorator(defaultComparer);

            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            for (var i = 0; i < 10; i++)
            {
                _collection.Move(i, i * 2 + 1);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Move(i * 2 + 1, i);
                Assert();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RemoveShouldTrackChanges(bool defaultComparer)
        {
            AddDecorator(defaultComparer);

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReplaceShouldTrackChanges1(bool defaultComparer)
        {
            AddDecorator(defaultComparer);

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
        [InlineData(true)]
        [InlineData(false)]
        public void ReplaceShouldTrackChanges2(bool defaultComparer)
        {
            AddDecorator(defaultComparer);

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
        [InlineData(true)]
        [InlineData(false)]
        public void ResetShouldTrackChanges(bool defaultComparer)
        {
            AddDecorator(defaultComparer);

            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            _collection.Reset(new object[] {1, 2, 3, 4, 5});
            Assert();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldTrackChanges(bool defaultComparer)
        {
            AddDecorator(defaultComparer);

            for (var i = 0; i < 4; i++)
            {
                _collection.Add(1);
                Assert();

                _collection.Insert(1, 2);
                Assert();

                _collection.Move(0, 1);
                Assert();

                _collection.Move(1, 0);
                Assert();

                _collection.Remove(2);
                Assert();

                _collection.RemoveAt(0);
                Assert();

                _collection.Reset(new object[] {1, 2, 3, 4, 5, i});
                Assert();

                _collection[0] = 200;
                Assert();

                _collection[3] = 3;
                Assert();

                _collection.Move(0, _collection.Count - 1);
                Assert();

                _collection.Move(0, _collection.Count - 2);
                Assert();

                _collection[i] = i;
                Assert();
            }

            _collection.Clear();
            Assert();
        }

        private void AddDecorator(bool defaultComparer)
        {
            if (!defaultComparer)
            {
                _headerComparer = SortingComparer<int>.Descending(o => o).Build();
                _footerComparer = SortingComparer<int>.Ascending(o => o).Build();
            }

            _collection.AddComponent(new ItemHeaderFooterCollectionDecorator<object>(_isHeaderOrFooter, _headerComparer, _footerComparer));
        }

        private void Assert()
        {
            _tracker.ChangedItems.ShouldEqual(_collection.Decorate());
            _tracker.ChangedItems.ShouldEqual(Decorate().ToArray());
        }

        private IEnumerable<object> Decorate()
        {
            var enumerable = _collection.Where(o => _isHeaderOrFooter(o) == true);
            if (_headerComparer != null)
                enumerable = enumerable.OrderBy(o => o, _headerComparer);

            foreach (var item in enumerable)
                yield return item;

            foreach (var item in _collection.Where(o => _isHeaderOrFooter(o) == null))
                yield return item;

            enumerable = _collection.Where(o => _isHeaderOrFooter(o) == false);
            if (_footerComparer != null)
                enumerable = enumerable.OrderBy(o => o, _footerComparer);
            foreach (var item in enumerable)
                yield return item;
        }

        private static IEnumerable<object> Decorate(SynchronizedObservableCollection<string> collection, Func<string, bool?> isHeaderOrFooter)
        {
            var enumerable = collection.Where(o => isHeaderOrFooter(o) == true);
            foreach (var item in enumerable)
                yield return item;
            foreach (var item in collection.Where(o => isHeaderOrFooter(o) == null))
                yield return item;
            enumerable = collection.Where(o => isHeaderOrFooter(o) == false);
            foreach (var item in enumerable)
                yield return item;
        }
    }
}