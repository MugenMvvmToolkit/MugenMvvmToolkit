using System;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.UnitTests.Collections.Internal;
using MugenMvvm.UnitTests.Models.Internal;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class FilterCollectionDecoratorTest : CollectionDecoratorTestBase
    {
        private readonly DecoratedCollectionChangeTracker<object> _tracker;
        private readonly SynchronizedObservableCollection<object?> _collection;
        private readonly Func<TestCollectionItem, int, bool> _filter2;
        private Func<int, int, bool> _filter1;

        public FilterCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object?>(ComponentCollectionManager);
            _filter1 = (i, _) => i % 2 == 0;
            _filter2 = (i, _) => i.Id % 2 == 0;
            _collection.AddComponent(new FilterCollectionDecorator<int>(0) {Filter = _filter1});
            _collection.AddComponent(new FilterCollectionDecorator<TestCollectionItem>(-1) {Filter = _filter2});

            _tracker = new DecoratedCollectionChangeTracker<object>();
            _collection.AddComponent(_tracker);
            _tracker.Changed += Assert;
        }

        [Fact]
        public void ChangeShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(new TestCollectionItem {Id = i});
            Assert();

            for (var i = 0; i < _collection.Count; i++)
            {
                ((TestCollectionItem) _collection[i]!).Id = i == 0 ? 0 : Guid.NewGuid().GetHashCode();
                _collection.RaiseItemChanged(_collection[i]);
                Assert();
            }
        }

        [Fact]
        public void ShouldTrackChangesEmptyFilter()
        {
            _filter1 = (_, _) => true;
            _collection.RemoveComponents<FilterCollectionDecorator<int>>();

            _collection.Add(1);
            Assert();

            _collection.Insert(1, 2);
            Assert();

            _collection.Remove(2);
            Assert();

            _collection.RemoveAt(0);
            Assert();

            _collection.Reset(new object[] {1, 2, 3, 4, 5});
            Assert();

            _collection[0] = 200;
            Assert();

            _collection.Move(1, 2);
            Assert();

            _collection.Clear();
            Assert();
        }

        [Fact]
        public void ShouldTrackChangesSetFilter()
        {
            var filter = _filter1;
            _filter1 = (_, _) => true;
            _collection.RemoveComponents<FilterCollectionDecorator<int>>();
            _collection.Reset(new object[] {1, 2, 3, 4, 5});

            var decorator = new FilterCollectionDecorator<int>(0);
            _collection.AddComponent(decorator);

            _filter1 = filter;
            decorator.Filter = filter;
            Assert();
        }

        protected override IObservableCollection<object?> GetCollection() => _collection;

        protected override void Assert()
        {
            _tracker.ChangedItems.ShouldEqual(_collection.Where((o, index) => o is not int i || _filter1(i, index))
                                                         .Where((o, index) => o is not TestCollectionItem t || _filter2(t, index)));
            _tracker.ChangedItems.ShouldEqual(_collection.DecoratedItems());
        }
    }
}