﻿using System;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.UnitTests.Collections.Internal;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class LastTrackerCollectionDecoratorTest : CollectionDecoratorTestBase
    {
        private readonly Func<int, bool>? _condition;
        private readonly SynchronizedObservableCollection<object?> _collection;
        private readonly DecoratedCollectionChangeTracker<object> _tracker;
        private int _item;
        private bool _hasItem;

        public LastTrackerCollectionDecoratorTest(Func<int, bool>? condition = null, ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _condition = condition;
            _collection = new SynchronizedObservableCollection<object?>(ComponentCollectionManager);
            _tracker = new DecoratedCollectionChangeTracker<object>();
            _collection.AddComponent(_tracker);
            var decorator = new FirstLastTrackerCollectionDecorator<int>(0, false, false, (o, hv) =>
            {
                _item = o;
                _hasItem = hv;
            }, condition);
            _collection.AddComponent(decorator);
            _tracker.Changed += Assert;
        }

        [Fact]
        public void ShouldTrackChanges1()
        {
            TestCollectionItem? item = null;
            bool hasItem = false;
            Action assert = () =>
            {
                var items = _collection.OfType<TestCollectionItem>().Where(collectionItem => collectionItem.Id % 2 == 0);
                item.ShouldEqual(items.LastOrDefault());
                hasItem.ShouldEqual(items.Any());
            };
            _collection.RemoveComponent(_tracker);
            _collection.AddComponent(new FirstLastTrackerCollectionDecorator<TestCollectionItem>(0, false, false, (collectionItem, hv) =>
            {
                item = collectionItem;
                hasItem = hv;
                assert();
            }, collectionItem => collectionItem.Id % 2 == 0));

            for (var i = 0; i < DefaultCount; i++)
            {
                _collection.Add(new TestCollectionItem
                {
                    Id = i
                });
                assert();

                _collection.Add(i);
                assert();
            }

            for (var i = 0; i < _collection.Count; i++)
            {
                if (_collection[i] is TestCollectionItem testCollectionItem)
                {
                    testCollectionItem.Id += 1;
                    _collection.RaiseItemChanged(testCollectionItem);
                    assert();
                }
            }

            for (var i = _collection.Count - 1; i >= 0; i--)
            {
                if (_collection[i] is TestCollectionItem testCollectionItem)
                {
                    testCollectionItem.Id -= 1;
                    _collection.RaiseItemChanged(testCollectionItem);
                    assert();
                }
            }
        }

        protected override IObservableCollection<object?> GetCollection() => _collection;

        protected override void Assert()
        {
            _tracker.ChangedItems.ShouldEqual(_collection.DecoratedItems());
            var items = _collection.OfType<int>().Where(_condition ?? (_ => true));
            _item.ShouldEqual(items.LastOrDefault());
            _hasItem.ShouldEqual(items.Any());
        }
    }

    public class LastTrackerCollectionDecoratorConditionTest : LastTrackerCollectionDecoratorTest
    {
        public LastTrackerCollectionDecoratorConditionTest(ITestOutputHelper? outputHelper = null) : base(i => i > 10 && i < 20, outputHelper)
        {
        }
    }
}