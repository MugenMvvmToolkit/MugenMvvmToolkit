﻿using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Collections.Internal;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class ItemHeaderFooterCollectionDecoratorTest : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<object> _collection;
        private readonly DecoratorObservableCollectionTracker<object> _tracker;
        private readonly Func<object?, bool?> _isHeaderOrFooter;
        private readonly IComparer<object?> _headerComparer;
        private readonly IComparer<object?> _footerComparer;

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
            _headerComparer = SortingComparer<object?>.Descending(o => (int) o!).Build();
            _footerComparer = SortingComparer<object?>.Ascending(o => (int) o!).Build();
            _collection.AddComponent(new ItemHeaderFooterCollectionDecorator(_isHeaderOrFooter, _headerComparer, _footerComparer));
        }

        [Fact]
        public void AddShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                AssertChanges();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Insert(i, i);
                AssertChanges();
            }
        }

        [Fact]
        public void ClearShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            AssertChanges();

            _collection.Clear();
            AssertChanges();
        }

        [Fact]
        public void MoveShouldTrackChanges1()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            AssertChanges();

            for (var i = 0; i < 10; i++)
            {
                _collection.Move(i, i + 1);
                AssertChanges();
            }
        }

        [Fact]
        public void MoveShouldTrackChanges2()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            AssertChanges();

            for (var i = 0; i < 10; i++)
            {
                _collection.Move(i, i * 2 + 1);
                AssertChanges();
            }
        }

        [Fact]
        public void RemoveShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            AssertChanges();

            for (var i = 0; i < 20; i++)
            {
                _collection.Remove(i);
                AssertChanges();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.RemoveAt(i);
                AssertChanges();
            }

            var count = _collection.Count;
            for (var i = 0; i < count; i++)
            {
                _collection.RemoveAt(0);
                AssertChanges();
            }
        }

        [Fact]
        public void ReplaceShouldTrackChanges1()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            AssertChanges();

            for (var i = 0; i < 10; i++)
            {
                _collection[i] = i + 101;
                AssertChanges();
            }
        }

        [Fact]
        public void ReplaceShouldTrackChanges2()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            AssertChanges();

            for (var i = 0; i < 10; i++)
            for (var j = 10; j < 20; j++)
            {
                _collection[i] = _collection[j];
                AssertChanges();
            }
        }

        [Fact]
        public void ResetShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            AssertChanges();

            _collection.Reset(new object[] {1, 2, 3, 4, 5});
            AssertChanges();
        }

        [Fact]
        public void ShouldTrackChanges()
        {
            for (int i = 0; i < 4; i++)
            {
                _collection.Add(1);
                AssertChanges();

                _collection.Insert(1, 2);
                AssertChanges();

                _collection.Move(0, 1);
                AssertChanges();

                _collection.Move(1, 0);
                AssertChanges();

                _collection.Remove(2);
                AssertChanges();

                _collection.RemoveAt(0);
                AssertChanges();

                _collection.Reset(new object[] {1, 2, 3, 4, 5, i});
                AssertChanges();

                _collection[0] = 200;
                AssertChanges();

                _collection[3] = 3;
                AssertChanges();

                _collection.Move(0, _collection.Count - 1);
                AssertChanges();

                _collection.Move(0, _collection.Count - 2);
                AssertChanges();

                _collection[i] = i;
                AssertChanges();
            }

            _collection.Clear();
            AssertChanges();
        }

        private void AssertChanges()
        {
            _tracker.ChangedItems.ShouldEqual(_collection.Decorate());
            _tracker.ChangedItems.ShouldEqual(Decorate().ToArray());
        }

        private IEnumerable<object> Decorate()
        {
            foreach (var item in _collection.Where(o => _isHeaderOrFooter(o) == true).OrderBy(o => o, _headerComparer))
                yield return item;

            foreach (var item in _collection.Where(o => _isHeaderOrFooter(o) == null))
                yield return item;

            foreach (var item in _collection.Where(o => _isHeaderOrFooter(o) == false).OrderBy(o => o, _footerComparer))
                yield return item;
        }
    }
}