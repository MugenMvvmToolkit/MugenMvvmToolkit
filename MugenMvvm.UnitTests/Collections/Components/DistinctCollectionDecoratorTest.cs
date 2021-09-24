using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.UnitTests.Collections.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class DistinctCollectionDecoratorTest : CollectionDecoratorTestBase
    {
        private readonly SynchronizedObservableCollection<object?> _collection;
        private readonly DecoratedCollectionChangeTracker<object> _tracker;

        public DistinctCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object?>(ComponentCollectionManager);
            _tracker = new DecoratedCollectionChangeTracker<object>();
            _collection.AddComponent(_tracker);
            var decorator = new DistinctCollectionDecorator<DistinctItem, int>(0, i => i.Index);
            _collection.AddComponent(decorator);
            _tracker.Changed += Assert;
            _tracker.PendingChanged += AssertPending;
        }

        [Fact]
        public void ChangeShouldTrackChanges()
        {
            base.InitializeDefaultData(_collection, DefaultCount, GetData);

            for (var i = 0; i < _collection.Count; i++)
            {
                _collection.RaiseItemChanged(_collection[i]);
                Assert();
                _tracker.ItemChangedCount.ShouldEqual(i + 1);
            }
        }

        [Fact]
        public void ChangeShouldTrackChanges1()
        {
            InitializeDefaultData(_collection, DefaultCount, GetData);

            for (var i = 0; i < _collection.Count; i++)
            {
                if (_collection[i] is DistinctItem distinctItem)
                {
                    distinctItem.Index = i == 0 ? 0 : Guid.NewGuid().GetHashCode();
                    _collection.RaiseItemChanged(distinctItem);
                }

                Assert();
            }
        }

        [Fact]
        public void ChangeShouldTrackChanges2()
        {
            InitializeDefaultData(_collection, DefaultCount, GetData);

            for (var i = 0; i < _collection.Count; i++)
            {
                if (_collection[i] is DistinctItem distinctItem)
                {
                    distinctItem.Index = i % 2;
                    _collection.RaiseItemChanged(distinctItem);
                }

                Assert();
            }
        }

        [Fact]
        public void ChangeShouldTrackChanges3()
        {
            InitializeDefaultData(_collection, DefaultCount, GetData);

            for (var i = 0; i < _collection.Count; i++)
            {
                if (_collection[i] is DistinctItem distinctItem)
                {
                    distinctItem.Index = 1;
                    _collection.RaiseItemChanged(distinctItem);
                }

                Assert();
            }
        }

        [Fact]
        public void ChangeShouldTrackChanges4()
        {
            InitializeDefaultData(_collection, DefaultCount, GetData);

            for (var i = 0; i < _collection.Count; i++)
            {
                if (_collection[i] is DistinctItem distinctItem)
                {
                    distinctItem.Index -= 1;
                    _collection.RaiseItemChanged(distinctItem);
                }

                Assert();
            }
        }

        private void AssertPending()
        {
            var hashSet = new HashSet<DistinctItem>(IndexEqualityComparer.Instance);
            var decoratedItems = _collection.DecoratedItems().OfType<DistinctItem>();
            foreach (var item in decoratedItems)
                hashSet.Add(item).ShouldBeTrue();
        }

        protected override IObservableCollection<object?> GetCollection() => _collection;

        protected override void Assert()
        {
            _tracker.ChangedItems.ShouldEqual(_collection.DecoratedItems());
            var distinct = Distinct();
            _collection.DecoratedItems().ShouldEqual(distinct);
        }

        private IEnumerable<object?> Distinct()
        {
            var hashSet = new HashSet<DistinctItem>(IndexEqualityComparer.Instance);
            foreach (var o in _collection)
            {
                if (o is DistinctItem distinctItem)
                {
                    if (hashSet.Add(distinctItem))
                        yield return o;
                    continue;
                }

                yield return o;
            }
        }

        protected override void InitializeDefaultData(IList<object?> collection, int minCount, Func<int, object?> getData)
        {
            base.InitializeDefaultData(collection, minCount, getData);
            base.InitializeDefaultData(collection, minCount, getData);
        }

        protected override object GetData(int index) => new DistinctItem {Index = index};

        private sealed class DistinctItem
        {
            public int Index;

            public override string ToString() => $"{Index.ToString()}-{GetHashCode()}";
        }

        private sealed class IndexEqualityComparer : IEqualityComparer<DistinctItem?>
        {
            public static readonly IEqualityComparer<DistinctItem> Instance = new IndexEqualityComparer();

            public bool Equals(DistinctItem? x, DistinctItem? y) => x!.Index == y!.Index;

            public int GetHashCode(DistinctItem? obj) => obj!.Index;
        }
    }
}