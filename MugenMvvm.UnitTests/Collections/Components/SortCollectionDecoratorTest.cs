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
    public class SortCollectionDecoratorTest : UnitTestBase, IComparer<object?>
    {
        private readonly SynchronizedObservableCollection<object> _collection;
        private readonly DecoratedCollectionChangeTracker<object> _tracker;
        private readonly SortCollectionDecorator _decorator;
        private bool? _defaultComparer;

        public SortCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            _decorator = new SortCollectionDecorator();
            _tracker = new DecoratedCollectionChangeTracker<object>();
            _collection.AddComponent(_decorator);
            _collection.AddComponent(_tracker);
            _tracker.Changed += Assert;
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
                _collection.Add(new SortItem(i));
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Insert(i, new SortItem(i));
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
                _collection.Add(new SortItem(Guid.NewGuid().GetHashCode()));
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Insert(i, new SortItem(Guid.NewGuid().GetHashCode()));
                Assert();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ChangeShouldTrackChanges1(bool defaultComparer)
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

            for (var i = 0; i < _collection.Count; i++)
            {
                var newId = i == 0 ? 0 : Guid.NewGuid().GetHashCode();
                ((TestCollectionItem)_collection[i]).Id = newId;
                _collection.RaiseItemChanged(_collection[i], null);
                Assert();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ChangeShouldTrackChanges2(bool defaultComparer)
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

            for (var i = 0; i < _collection.Count; i++)
            {
                var newId = i % 2;
                ((TestCollectionItem)_collection[i]).Id = newId;
                _collection.RaiseItemChanged(_collection[i], null);
                Assert();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ChangeShouldTrackChanges3(bool defaultComparer)
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

            for (var i = 0; i < _collection.Count; i++)
            {
                var newId = 1;
                ((TestCollectionItem)_collection[i]).Id = newId;
                _collection.RaiseItemChanged(_collection[i], null);
                Assert();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ChangeShouldTrackChanges4(bool defaultComparer)
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

            for (var i = 0; i < _collection.Count; i++)
            {
                var newId = -1;
                ((TestCollectionItem)_collection[i]).Id = newId;
                _collection.RaiseItemChanged(_collection[i], null);
                Assert();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void ClearShouldTrackChanges(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
            for (var i = 0; i < 100; i++)
                _collection.Add(new SortItem(i));
            Assert();

            _collection.Clear();
            Assert();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void MoveShouldTrackChanges1(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
            for (var i = 0; i < 100; i++)
                _collection.Add(new SortItem(i));
            Assert();

            for (var i = 0; i < _collection.Count - 1; i++)
            {
                _collection.Move(i, i + 1);
                Assert();
            }

            for (var i = 0; i < _collection.Count - 1; i++)
            {
                _collection.Move(i + 1, i);
                Assert();
            }

            _collection.Move(0, _collection.Count - 1);
            Assert();

            _collection.Move(_collection.Count - 1, 0);
            Assert();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void MoveShouldTrackChanges2(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
            for (var i = 0; i < 100; i++)
                _collection.Add(new SortItem(i));
            Assert();

            for (var i = 1; i < _collection.Count - 1; i++)
            {
                _collection.Move(i, Math.Min(i * 2 + i, _collection.Count - 1));
                Assert();
            }

            for (var i = 1; i < _collection.Count - 1; i++)
            {
                _collection.Move(Math.Min(i * 2 + i, _collection.Count - 1), i);
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
                _collection.Add(new SortItem(i));
            Assert();

            for (var i = 0; i < 20; i++)
            {
                _collection.Remove(_collection[0]);
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
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void RemoveShouldTrackChanges2(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
            for (var i = 0; i < 100; i++)
                _collection.Add(new SortItem(Guid.NewGuid().GetHashCode()));
            Assert();

            for (var i = 0; i < _collection.Count; i++)
            {
                _collection.RemoveAt(0);
                Assert();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void ReorderShouldTrackChanges1(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
            _collection.Reset(new[] { new SortItem(1), new SortItem(2), new SortItem(3), new SortItem(3), new SortItem(4), new SortItem(5), new SortItem(6), new SortItem(7) });

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
        public void ReplaceShouldTrackChanges1(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;
            for (var i = 0; i < 100; i++)
                _collection.Add(new SortItem(i));
            Assert();

            for (var i = 0; i < _collection.Count; i++)
            {
                _collection[i] = new SortItem(i + Guid.NewGuid().GetHashCode());
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection[i + 10] = new SortItem(i + 10);
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
                _collection.Add(new SortItem(i));
            Assert();

            for (var i = 0; i < _collection.Count / 2; i++)
            for (var j = _collection.Count / 2; j < _collection.Count; j++)
            {
                _collection[i] = _collection[j];
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
                _collection.Add(new SortItem(i));
            Assert();

            _collection.Reset(new object[] { new SortItem(1), new SortItem(2), new SortItem(3), new SortItem(4), new SortItem(5) });
            Assert();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(null)]
        public void ShouldTrackChanges1(bool? defaultComparer)
        {
            DefaultComparer = defaultComparer;

            _collection.Add(new SortItem(1));
            Assert();

            _collection.Insert(1, new SortItem(2));
            Assert();

            _collection.Remove(_collection[1]);
            Assert();

            _collection.RemoveAt(0);
            Assert();

            _collection.Reset(new object[] { new SortItem(1), new SortItem(2), new SortItem(3), new SortItem(4), new SortItem(5) });
            Assert();

            _collection[0] = new SortItem(200);
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

            _collection.Add(new SortItem(Guid.NewGuid().GetHashCode()));
            Assert();

            _collection.Insert(1, new SortItem(2));
            Assert();

            _collection.Remove(_collection[1]);
            Assert();

            _collection.RemoveAt(0);
            Assert();

            _collection.Reset(new[]
            {
                new SortItem(Guid.NewGuid().GetHashCode()), new SortItem(Guid.NewGuid().GetHashCode()), new SortItem(Guid.NewGuid().GetHashCode()), new SortItem(4), new SortItem(5)
            });
            Assert();

            _collection[0] = new SortItem(200);
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

        private bool? DefaultComparer
        {
            get => _defaultComparer;
            set
            {
                _defaultComparer = value;
                _decorator.Comparer = value == null ? null : this;
            }
        }

        private void Assert()
        {
            if (DefaultComparer == null)
                _collection.ShouldEqual(_tracker.ChangedItems);
            else
                _collection.OrderBy(o => o, _decorator.Comparer).ShouldEqual(_tracker.ChangedItems);
            _collection.DecoratedItems().ShouldEqual(_tracker.ChangedItems);
        }

        int IComparer<object?>.Compare(object? x1, object? x2)
        {
            var x = (SortItem)x1!;
            var y = (SortItem)x2!;
            if (DefaultComparer.GetValueOrDefault())
                return ThenBy(Comparer<int>.Default.Compare(x.Value, y.Value), x1, x2);
            return ThenBy(y.Value.CompareTo(x.Value), x1, x2);
        }

        private int ThenBy(int result, object? x1, object? x2)
        {
            if (result != 0)
                return result;
            return _collection.IndexOf(x1).CompareTo(_collection.IndexOf(x2));
        }

        private sealed class SortItem
        {
            public readonly int Value;

            public SortItem(int value)
            {
                Value = value;
            }

            public override string ToString() => Value.ToString();
        }
    }
}