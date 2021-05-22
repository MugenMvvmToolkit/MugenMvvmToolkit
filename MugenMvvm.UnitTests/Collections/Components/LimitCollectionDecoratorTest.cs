using System.Collections.Generic;
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
    public class LimitCollectionDecoratorTest : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<object> _collection;
        private readonly LimitCollectionDecorator<int> _decorator;
        private readonly DecoratorObservableCollectionTracker<object> _tracker;

        public LimitCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            _decorator = new LimitCollectionDecorator<int>(null, i => i % 2 == 0);
            _collection.AddComponent(_decorator);
            _tracker = new DecoratorObservableCollectionTracker<object>();
            _collection.AddComponent(_tracker);
            _tracker.Changed += Assert;
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(null)]
        public void AddShouldTrackChanges(int? limit)
        {
            _decorator.Limit = limit;
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

            _decorator.Limit = null;
            Assert();
            _decorator.Limit = limit;
            Assert();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(null)]
        public void ChangeShouldTrackChanges1(int? limit)
        {
            var ignoreIds = new HashSet<int>();
            _decorator.Limit = limit;
            _decorator.Condition = i => !ignoreIds.Contains(i);

            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            for (var i = 0; i < _collection.Count; i++)
            {
                ignoreIds.Add(i);
                _collection.RaiseItemChanged(_collection[i], null);
                Assert();

                ignoreIds.Remove(i);
                _collection.RaiseItemChanged(_collection[i], null);
                Assert();

                if (limit == null)
                    _tracker.ItemChangedCount.ShouldEqual((i + 1) * 2);
                else
                    _tracker.ItemChangedCount.ShouldEqual(0);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(null)]
        public void ChangeShouldTrackChanges2(int? limit)
        {
            _decorator.Limit = limit;

            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            int countLimit = 0;
            int count = 0;
            for (var i = 0; i < _collection.Count; i++)
            {
                if (IsSatisfied(i))
                {
                    if (countLimit < limit.GetValueOrDefault(int.MaxValue))
                        ++countLimit;
                }
                else
                    ++count;

                _collection.RaiseItemChanged(_collection[i], null);
                Assert();
                _tracker.ItemChangedCount.ShouldEqual(count + countLimit);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(null)]
        public void ClearShouldTrackChanges(int? limit)
        {
            _decorator.Limit = limit;

            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            _collection.Clear();
            Assert();

            _decorator.Limit = null;
            Assert();
            _decorator.Limit = limit;
            Assert();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(null)]
        public void MoveShouldTrackChanges1(int? limit)
        {
            _decorator.Limit = limit;

            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            for (var i = 0; i < 10; i++)
            {
                _collection.Move(i, i + 1);
                Assert();
            }

            _decorator.Limit = null;
            Assert();
            _decorator.Limit = limit;
            Assert();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(null)]
        public void MoveShouldTrackChanges2(int? limit)
        {
            _decorator.Limit = limit;

            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            for (var i = 1; i < 10; i++)
            {
                _collection.Move(i, i * 2 + i);
                Assert();
            }

            _decorator.Limit = null;
            Assert();
            _decorator.Limit = limit;
            Assert();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(null)]
        public void RemoveShouldTrackChanges(int? limit)
        {
            _decorator.Limit = limit;

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

            _decorator.Limit = null;
            Assert();
            _decorator.Limit = limit;
            Assert();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(null)]
        public void ReplaceShouldTrackChanges1(int? limit)
        {
            _decorator.Limit = limit;

            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            for (var i = 0; i < 10; i++)
            {
                _collection[i] = i + 101;
                Assert();
            }

            _decorator.Limit = null;
            Assert();
            _decorator.Limit = limit;
            Assert();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(null)]
        public void ReplaceShouldTrackChanges2(int? limit)
        {
            _decorator.Limit = limit;

            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            for (var i = 0; i < 10; i++)
            for (var j = 10; j < 20; j++)
            {
                _collection[i] = _collection[j];
                Assert();
            }

            _decorator.Limit = null;
            Assert();
            _decorator.Limit = limit;
            Assert();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(null)]
        public void ResetShouldTrackChanges(int? limit)
        {
            _decorator.Limit = limit;

            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            _collection.Reset(new object[] {1, 2, 3, 4, 5});
            Assert();

            _decorator.Limit = null;
            Assert();
            _decorator.Limit = limit;
            Assert();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(null)]
        public void ShouldAttachDetach(int? limit)
        {
            _decorator.Limit = limit;

            _collection.RemoveComponent(_decorator);

            for (var i = 0; i < 2; i++)
            {
                _collection.AddComponent(_decorator);
                if (i != 0)
                    _collection.Add(i);

                Assert();
                _collection.RemoveComponent(_decorator);
                Assert();
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(null)]
        public void ShouldTrackChanges(int? limit)
        {
            _decorator.Limit = limit;

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
        }

        private void Assert()
        {
            _tracker.ChangedItems.ShouldEqual(_collection.Decorate());
            _tracker.ChangedItems.ShouldEqual(_decorator.Limit == null ? _collection : Decorate(_decorator.Limit.Value));
        }

        private IEnumerable<object?> Decorate(int limit)
        {
            var count = 0;
            foreach (var item in _collection)
            {
                if (IsSatisfied(item) && ++count > limit)
                    continue;

                yield return item;
            }
        }

        private bool IsSatisfied(object? item) => item is int i && (_decorator.Condition == null || _decorator.Condition(i));
    }
}