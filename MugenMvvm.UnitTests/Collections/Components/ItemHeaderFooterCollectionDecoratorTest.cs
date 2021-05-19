﻿using System;
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
                _headerComparer = SortingComparer<object?>.Descending(o => (int) o!).Build();
                _footerComparer = SortingComparer<object?>.Ascending(o => (int) o!).Build();
            }

            _collection.AddComponent(new ItemHeaderFooterCollectionDecorator(_isHeaderOrFooter, _headerComparer, _footerComparer));
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
    }
}