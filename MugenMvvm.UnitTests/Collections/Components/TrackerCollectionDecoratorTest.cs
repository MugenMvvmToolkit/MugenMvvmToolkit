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
    public class TrackerCollectionDecoratorTest : CollectionDecoratorTestBase
    {
        private readonly SynchronizedObservableCollection<object> _collection;
        private readonly Dictionary<int, int> _items;
        private int _sum;
        private int _resetCount;
        private bool? _isReset;

        public TrackerCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _items = new Dictionary<int, int>();
            _collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            var tracker = new DecoratedCollectionChangeTracker<object>();
            tracker.Changed += Assert;
            _collection.AddComponent(tracker);
            _collection.AddComponent(new TrackerCollectionDecorator<int, int>((items, item, state, count, isReset) =>
            {
                if (_isReset.HasValue)
                    _isReset.Value.ShouldEqual(isReset);
                AssertItems(_items, items, item, false, false);
                state.ShouldEqual(count - 1);
                _sum += item;
                _items[item] = count;
                return count;
            }, (items, item, state, count, isReset) =>
            {
                if (_isReset.HasValue)
                    _isReset.Value.ShouldEqual(isReset);
                AssertItems(_items, items, item, count == 0, false);
                state.ShouldEqual(count + 1);
                _sum -= item;
                if (count == 0)
                    _items.Remove(item);
                else
                    _items[item] = count;
                return count;
            }, null, items =>
            {
                AssertItems(_items, items);
                ++_resetCount;
            }));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        public void ChangeShouldTrackChanges(int duplicateCount)
        {
            var trackedItems = new Dictionary<ChangeModel, int>();
            var collection = new SynchronizedObservableCollection<ChangeModel>(ComponentCollectionManager);
            var tracker = new DecoratedCollectionChangeTracker<object>();
            collection.AddComponent(tracker);
            collection.AddComponent(new TrackerCollectionDecorator<ChangeModel, int>((items, item, state, count, isReset) =>
            {
                isReset.ShouldBeFalse();
                AssertItems(trackedItems, items, item, false, false);
                var value = count == 1 ? item.Value : state;
                _sum += value;
                trackedItems[item] = count;
                return value;
            }, (items, item, state, count, isReset) =>
            {
                isReset.ShouldBeFalse();
                AssertItems(trackedItems, items, item, count == 0, false);
                _sum -= state;
                if (count == 0)
                    trackedItems.Remove(item);
                else
                    trackedItems[item] = count;
                return state;
            }, (items, item, state, count, isReset, args) =>
            {
                isReset.ShouldBeFalse();
                AssertItems(trackedItems, items, item, false, true);
                args.ShouldEqual(this);
                count.ShouldEqual(items[item].count);
                var value = item.Value;
                if (state != value)
                {
                    for (var i = 0; i < count; i++)
                        _sum = _sum - state + value;
                }

                return value;
            }, null));
            Action assert = () =>
            {
                collection.Sum(i => i.Value).ShouldEqual(_sum);
                foreach (var grouping in collection.GroupBy(i => i))
                    trackedItems[grouping.Key].ShouldEqual(grouping.Count());
            };
            tracker.Changed += assert;

            for (var i = 0; i < DefaultCount; i++)
            {
                var changeModel = new ChangeModel { Value = i };
                for (var j = 0; j < duplicateCount; j++)
                {
                    collection.Add(changeModel);
                    assert();
                }
            }

            var random = new Random(Guid.NewGuid().GetHashCode());
            foreach (var c in collection)
            {
                c.Value = random.Next(-1000, 1000);
                collection.RaiseItemChanged(c, this);
                assert();
            }
        }

        [Fact]
        public void ResetShouldTrackChanges1()
        {
            _isReset = true;
            _resetCount = 0;

            _collection.Reset(new object[] { 1, 2, 2, 3, 3, 3, 4, 4, 4, 4 });
            Assert();
            _resetCount.ShouldEqual(1);

            _collection.Reset(new object[] { 1, 1, 1, 2, 3, 3, 4, 4, 4, 5, 5, 5 });
            Assert();
            _resetCount.ShouldEqual(2);

            _collection.Reset(new object[] { 1, 3, 4, 5, 5, 5, 5 });
            Assert();
            _resetCount.ShouldEqual(3);

            _collection.Clear();
            Assert();
            _resetCount.ShouldEqual(4);
        }

        public override void ClearShouldTrackChanges()
        {
            base.ClearShouldTrackChanges();
            _items.Count.ShouldEqual(0);
        }

        public override void RemoveShouldTrackChanges()
        {
            base.RemoveShouldTrackChanges();
            _items.Count.ShouldEqual(0);
        }

        protected override IObservableCollection<object> GetCollection() => _collection;

        protected override void Assert()
        {
            _collection.OfType<int>().Sum(i => i).ShouldEqual(_sum);
            int count = 0;
            foreach (var grouping in _collection.OfType<int>().GroupBy(i => i))
            {
                _items[grouping.Key].ShouldEqual(grouping.Count());
                ++count;
            }

            _items.Count.ShouldEqual(count);
        }

        private static void AssertItems<T>(Dictionary<T, int> currentItems, IReadOnlyDictionary<T, (int, int count)> items, T current, bool isRemove, bool isChange)
            where T : notnull
        {
            if (isRemove)
                items.ContainsKey(current).ShouldBeFalse();
            if (isChange)
                currentItems.Count.ShouldEqual(items.Count);
            foreach (var item in currentItems)
            {
                if (EqualityComparer<T>.Default.Equals(item.Key, current))
                    continue;

                items[item.Key].count.ShouldEqual(item.Value);
            }
        }

        private static void AssertItems<T>(Dictionary<T, int> currentItems, IReadOnlyDictionary<T, (int, int count)> items) where T : notnull
        {
            currentItems.Count.ShouldEqual(items.Count);
            foreach (var item in currentItems)
                items[item.Key].count.ShouldEqual(item.Value);
        }

        private class ChangeModel
        {
            public int Value { get; set; }
        }
    }
}