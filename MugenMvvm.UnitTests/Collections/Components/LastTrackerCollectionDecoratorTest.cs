using System;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.UnitTests.Collections.Internal;
using Should;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class LastTrackerCollectionDecoratorTest : CollectionDecoratorTestBase
    {
        private readonly Func<int, bool>? _condition;
        private readonly SynchronizedObservableCollection<object?> _collection;
        private readonly DecoratedCollectionChangeTracker<object> _tracker;
        private int _item;

        public LastTrackerCollectionDecoratorTest(Func<int, bool>? condition = null, ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _condition = condition;
            _collection = new SynchronizedObservableCollection<object?>(ComponentCollectionManager);
            _tracker = new DecoratedCollectionChangeTracker<object>();
            _collection.AddComponent(_tracker);
            var decorator = new FirstLastTrackerCollectionDecorator<int>(0, false, o => _item = o, condition);
            _collection.AddComponent(decorator);
            _tracker.Changed += Assert;
        }

        protected override IObservableCollection<object?> GetCollection() => _collection;

        protected override void Assert()
        {
            _tracker.ChangedItems.ShouldEqual(_collection.DecoratedItems());
            _item.ShouldEqual(_collection.OfType<int>().LastOrDefault(_condition ?? (_ => true)));
        }
    }

    public class LastTrackerCollectionDecoratorConditionTest : LastTrackerCollectionDecoratorTest
    {
        public LastTrackerCollectionDecoratorConditionTest(ITestOutputHelper? outputHelper = null) : base(i => i > 10 && i < 20, outputHelper)
        {
        }
    }
}