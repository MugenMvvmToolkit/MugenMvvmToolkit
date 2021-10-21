using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Collections.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class LimitCollectionDecoratorTest : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<object?> _collection;
        private readonly LimitCollectionDecorator<int> _decorator;
        private readonly DecoratedCollectionChangeTracker<object> _tracker;

        public LimitCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object?>(ComponentCollectionManager);
            _decorator = new LimitCollectionDecorator<int>(0, false, null, i => i % 2 == 0);
            _collection.AddComponent(_decorator);
            _tracker = new DecoratedCollectionChangeTracker<object>();
            _collection.AddComponent(_tracker);
            _tracker.Changed += Assert;
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void AddShouldTrackChanges(int? limit, bool hasCondition)
        {
            _decorator.Limit = limit;
            if (!hasCondition)
                _decorator.Condition = null;
            CollectionDecoratorTestBase.AddShouldTrackChangesImpl(_collection, Assert);

            _decorator.Limit = null;
            Assert();
            _decorator.Limit = limit;
            Assert();
        }

        [Theory]
        [MemberData(nameof(GetDataDefaultCondition))]
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
                _collection.RaiseItemChanged(_collection[i]);
                Assert();

                ignoreIds.Remove(i);
                _collection.RaiseItemChanged(_collection[i]);
                Assert();

                if (limit == null)
                    _tracker.ItemChangedCount.ShouldEqual((i + 1) * 2);
                else
                    _tracker.ItemChangedCount.ShouldEqual(0);
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ChangeShouldTrackChanges2(int? limit, bool hasCondition)
        {
            _decorator.Limit = limit;
            if (!hasCondition)
                _decorator.Condition = null;

            for (var i = 0; i < 100; i++)
                _collection.Add(i);
            Assert();

            var countLimit = 0;
            var count = 0;
            for (var i = 0; i < _collection.Count; i++)
            {
                if (IsSatisfied(i))
                {
                    if (countLimit < limit.GetValueOrDefault(int.MaxValue))
                        ++countLimit;
                }
                else
                    ++count;

                _collection.RaiseItemChanged(_collection[i]);
                Assert();
                _tracker.ItemChangedCount.ShouldEqual(count + countLimit);
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ClearShouldTrackChanges(int? limit, bool hasCondition)
        {
            _decorator.Limit = limit;
            if (!hasCondition)
                _decorator.Condition = null;
            CollectionDecoratorTestBase.ClearShouldTrackChangesImpl(_collection, Assert);

            _decorator.Limit = null;
            Assert();
            _decorator.Limit = limit;
            Assert();
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void MoveShouldTrackChanges1(int? limit, bool hasCondition)
        {
            _decorator.Limit = limit;
            if (!hasCondition)
                _decorator.Condition = null;
            CollectionDecoratorTestBase.MoveShouldTrackChanges1Impl(_collection, Assert);

            _decorator.Limit = null;
            Assert();
            _decorator.Limit = limit;
            Assert();
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void MoveShouldTrackChanges2(int? limit, bool hasCondition)
        {
            _decorator.Limit = limit;
            if (!hasCondition)
                _decorator.Condition = null;
            CollectionDecoratorTestBase.MoveShouldTrackChanges2Impl(_collection, Assert);

            _decorator.Limit = null;
            Assert();
            _decorator.Limit = limit;
            Assert();
        }

        [Fact]
        public void MoveShouldTrackChanges3()
        {
            _decorator.Limit = 1;
            _decorator.Condition = null;

            _collection.Add(1);
            _collection.Add(2);
            _collection.Add(3);
            _collection.Add(4);
            _collection.Add("st_1");
            _collection.Move(4, 1);
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void RemoveShouldTrackChanges(int? limit, bool hasCondition)
        {
            _decorator.Limit = limit;
            if (!hasCondition)
                _decorator.Condition = null;
            CollectionDecoratorTestBase.RemoveShouldTrackChangesImpl(_collection, Assert);

            _decorator.Limit = null;
            Assert();
            _decorator.Limit = limit;
            Assert();
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ReplaceShouldTrackChanges1(int? limit, bool hasCondition)
        {
            _decorator.Limit = limit;
            if (!hasCondition)
                _decorator.Condition = null;
            CollectionDecoratorTestBase.ReplaceShouldTrackChanges1Impl(_collection, Assert);

            _decorator.Limit = null;
            Assert();
            _decorator.Limit = limit;
            Assert();
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ReplaceShouldTrackChanges2(int? limit, bool hasCondition)
        {
            _decorator.Limit = limit;
            if (!hasCondition)
                _decorator.Condition = null;
            CollectionDecoratorTestBase.ReplaceShouldTrackChanges2Impl(_collection, Assert);

            _decorator.Limit = null;
            Assert();
            _decorator.Limit = limit;
            Assert();
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ResetShouldTrackChanges(int? limit, bool hasCondition)
        {
            _decorator.Limit = limit;
            if (!hasCondition)
                _decorator.Condition = null;
            CollectionDecoratorTestBase.ResetShouldTrackChangesImpl(_collection, Assert);

            _decorator.Limit = null;
            Assert();
            _decorator.Limit = limit;
            Assert();
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void ShouldAttachDetach(int? limit, bool hasCondition)
        {
            _decorator.Limit = limit;
            if (!hasCondition)
                _decorator.Condition = null;

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
        [MemberData(nameof(GetData))]
        public void ShouldTrackChanges(int? limit, bool hasCondition)
        {
            _decorator.Limit = limit;
            if (!hasCondition)
                _decorator.Condition = null;
            CollectionDecoratorTestBase.ShouldTrackChangesImpl(_collection, Assert);
        }

        public static IEnumerable<object?[]> GetData() => new[]
        {
            new object?[] {0, true},
            new object?[] {1, true},
            new object?[] {2, true},
            new object?[] {3, true},
            new object?[] {10, true},
            new object?[] {100, true},
            new object?[] {int.MaxValue - 1, true},
            new object?[] {null, true},
            new object?[] {0, false},
            new object?[] {1, false},
            new object?[] {2, false},
            new object?[] {3, false},
            new object?[] {10, false},
            new object?[] {100, false},
            new object?[] {int.MaxValue - 1, false},
            new object?[] {null, false},
        };

        public static IEnumerable<object?[]> GetDataDefaultCondition() => new[]
        {
            new object?[] {0},
            new object?[] {1},
            new object?[] {2},
            new object?[] {3},
            new object?[] {10},
            new object?[] {100},
            new object?[] {int.MaxValue - 1},
            new object?[] {null},
        };

        private void Assert()
        {
            _tracker.ChangedItems.ShouldEqual(_collection.DecoratedItems());
            _tracker.ChangedItems.ShouldEqual(_collection.GetComponentOptional<LimitCollectionDecorator<int>>() == null || _decorator.Limit == null
                ? _collection
                : Decorate(_decorator.Limit.Value));
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