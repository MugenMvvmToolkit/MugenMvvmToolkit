using System;
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
    public class FilterCollectionDecoratorTest : UnitTestBase
    {
        private readonly DecoratedCollectionChangeTracker<object> _tracker;
        private readonly SynchronizedObservableCollection<object> _collection;
        private readonly Func<TestCollectionItem, bool> _filter2;
        private Func<int, bool> _filter1;

        public FilterCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            _filter1 = i => i % 2 == 0;
            _filter2 = i => i.Id % 2 == 0;
            _collection.AddComponent(new FilterCollectionDecorator<int> { Filter = _filter1 });
            _collection.AddComponent(new FilterCollectionDecorator<TestCollectionItem> { Filter = _filter2 });

            _tracker = new DecoratedCollectionChangeTracker<object>();
            _collection.AddComponent(_tracker);
            _tracker.Changed += Assert;
        }

        [Fact]
        public void AddShouldTrackChanges()
        {
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

        [Fact]
        public void ChangeShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(new TestCollectionItem { Id = i });
            Assert();

            for (var i = 0; i < 100; i++)
            {
                ((TestCollectionItem)_collection[i]).Id = i == 0 ? 0 : Guid.NewGuid().GetHashCode();
                _collection.RaiseItemChanged(_collection[i], null);
                Assert();
            }
        }

        [Fact]
        public void ClearShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            _collection.Clear();
            Assert();
        }

        [Fact]
        public void MoveShouldTrackChanges1()
        {
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

        [Fact]
        public void MoveShouldTrackChanges2()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i, i * 2 + i);
                Assert();
            }

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i * 2 + i, i);
                Assert();
            }
        }

        [Fact]
        public void RemoveShouldTrackChanges()
        {
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

        [Fact]
        public void ReplaceShouldTrackChanges1()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            for (var i = 0; i < 10; i++)
            {
                _collection[i] = i + 101;
                Assert();
            }
        }

        [Fact]
        public void ReplaceShouldTrackChanges2()
        {
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

        [Fact]
        public void ResetShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            _collection.Reset(new object[] { 1, 2, 3, 4, 5 });
            Assert();
        }

        [Fact]
        public void ShouldTrackChanges()
        {
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

            _collection.Reset(new object[] { 1, 2, 3, 4, 5 });
            Assert();

            _collection[0] = 200;
            Assert();

            _collection.Clear();
            Assert();
        }

        [Fact]
        public void ShouldTrackChangesEmptyFilter()
        {
            _filter1 = i => true;
            _collection.RemoveComponents<FilterCollectionDecorator<int>>();

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

            _collection.Clear();
            Assert();
        }

        [Fact]
        public void ShouldTrackChangesSetFilter()
        {
            var filter = _filter1;
            _filter1 = i => true;
            _collection.RemoveComponents<FilterCollectionDecorator<int>>();
            _collection.Reset(new object[] { 1, 2, 3, 4, 5 });

            var decorator = new FilterCollectionDecorator<int>();
            _collection.AddComponent(decorator);

            _filter1 = filter;
            decorator.Filter = filter;
            Assert();
        }

        private void Assert()
        {
            _tracker.ChangedItems.ShouldEqual(_collection.Where(o => o is not int i || _filter1(i)).Where(o => o is not TestCollectionItem t || _filter2(t)));
            _tracker.ChangedItems.ShouldEqual(_collection.DecoratedItems());
        }
    }
}