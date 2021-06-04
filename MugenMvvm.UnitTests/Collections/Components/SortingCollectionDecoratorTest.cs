using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Collections.Internal;
using MugenMvvm.UnitTests.Models.Internal;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class SortingCollectionDecoratorTest : UnitTestBase, IComparer<object?>
    {
        private readonly SynchronizedObservableCollection<object> _collection;
        private readonly DecoratorObservableCollectionTracker<object> _tracker;
        private readonly SortingCollectionDecorator _decorator;
        private bool? _defaultComparer;

        public SortingCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            _decorator = new SortingCollectionDecorator();
            _tracker = new DecoratorObservableCollectionTracker<object>();
            _collection.AddComponent(_decorator);
            _collection.AddComponent(_tracker);
            _tracker.Changed += Assert;
        }

        private bool? DefaultComparer
        {
            get => _defaultComparer;
            set
            {
                _defaultComparer = value;
                _decorator.Comparer = value == null ? null : this;
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void ReorderShouldTrackChanges1(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
            _collection.Reset(new object[] { 1, 2, 3, 4, 5, 6, 6 });

            _decorator.Reorder();
            Assert();

            DefaultComparer = !defaultComparer;
            _decorator.Reorder();
            Assert();

            DefaultComparer = null;
            _decorator.Reorder();
            Assert();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void AddShouldTrackChanges1(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
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
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void AddShouldTrackChanges2(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
            for (var i = 0; i < 100; i++)
            {
                _collection.Add(Guid.NewGuid().GetHashCode());
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Insert(i, Guid.NewGuid().GetHashCode());
                Assert();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void ReplaceShouldTrackChanges1(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            for (var i = 0; i < 10; i++)
            {
                _collection[i] = i + Guid.NewGuid().GetHashCode();
                Assert();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void ReplaceShouldTrackChanges2(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
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
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void MoveShouldTrackChanges1(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
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
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void MoveShouldTrackChanges2(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
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
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void RemoveShouldTrackChanges1(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
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
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void RemoveShouldTrackChanges2(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
            for (var i = 0; i < 100; i++)
                _collection.Add(Guid.NewGuid().GetHashCode());
            Assert();

            for (var i = 0; i < 100; i++)
            {
                _collection.RemoveAt(0);
                Assert();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void ResetShouldTrackChanges(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            _collection.Reset(new object[] { 1, 2, 3, 4, 5 });
            Assert();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void ClearShouldTrackChanges(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            _collection.Clear();
            Assert();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ChangeShouldTrackChanges(bool defaultComparer)
        {
            DefaultComparer = defaultComparer;
            _decorator.Comparer = Comparer<object?>.Create((x1, x2) =>
            {
                var item = (TestCollectionItem)x1!;
                var collectionItem = (TestCollectionItem)x2!;
                if (defaultComparer)
                    return item.Id.CompareTo(collectionItem.Id);
                return collectionItem.Id.CompareTo(item.Id);
            });

            for (var i = 0; i < 100; i++)
                _collection.Add(new TestCollectionItem { Id = i });

            for (var i = 0; i < 100; i++)
            {
                ((TestCollectionItem)_collection[i]).Id = i == 0 ? 0 : Guid.NewGuid().GetHashCode();
                _collection.RaiseItemChanged(_collection[i], null);
                Assert();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void ShouldTrackChanges1(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;

            _collection.Add(1);
            Assert();

            _collection.Insert(1, 2);
            Assert();

            _collection.Remove(2);
            Assert();

            _collection.RemoveAt(0);
            Assert();

            _collection.Reset(new object[] { 1, 2, 3, 4, 5 });
            Assert();

            _collection[0] = 200;
            Assert();

            _collection.Move(1, 2);
            Assert();

            DefaultComparer = null;
            Assert();

            DefaultComparer = defaultComparer;
            Assert();

            _collection.Clear();
            Assert();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldTrackChanges2(bool defaultComparer)
        {
            DefaultComparer = defaultComparer;

            _collection.Add(Guid.NewGuid().GetHashCode());
            Assert();

            _collection.Insert(1, 2);
            Assert();

            _collection.Remove(2);
            Assert();

            _collection.RemoveAt(0);
            Assert();

            _collection.Reset(new object[] { Guid.NewGuid().GetHashCode(), Guid.NewGuid().GetHashCode(), Guid.NewGuid().GetHashCode(), 4, 5 });
            Assert();

            _collection[0] = 200;
            Assert();

            _collection.Move(1, 2);
            Assert();

            DefaultComparer = null;
            Assert();

            DefaultComparer = defaultComparer;
            Assert();

            _collection.Clear();
            Assert();
        }

        private void Assert()
        {
            if (DefaultComparer == null)
                _collection.ShouldEqual(_tracker.ChangedItems);
            else
                _collection.OrderBy(o => o, _decorator.Comparer).ShouldEqual(_tracker.ChangedItems);
            _collection.Decorate().ShouldEqual(_tracker.ChangedItems);
        }

        int IComparer<object?>.Compare(object? x1, object? x2)
        {
            var x = (int)x1!;
            var y = (int)x2!;
            if (DefaultComparer.GetValueOrDefault())
                return Comparer<int>.Default.Compare(x, y);
            return y.CompareTo(x);
        }
    }
}