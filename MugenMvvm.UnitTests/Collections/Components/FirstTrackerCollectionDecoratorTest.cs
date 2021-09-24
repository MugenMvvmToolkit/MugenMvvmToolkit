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
    public class FirstTrackerCollectionDecoratorTest : CollectionDecoratorTestBase
    {
        private readonly SynchronizedObservableCollection<object?> _collection;
        private readonly DecoratedCollectionChangeTracker<object> _tracker;
        private object? _item;

        public FirstTrackerCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object?>(ComponentCollectionManager);
            _tracker = new DecoratedCollectionChangeTracker<object>();
            _collection.AddComponent(_tracker);
            var decorator = new FirstLastTrackerCollectionDecorator(0, o => _item = o, true);
            _collection.AddComponent(decorator);
            _tracker.Changed += Assert;
        }

        protected override IObservableCollection<object?> GetCollection() => _collection;

        protected override void Assert()
        {
            _tracker.ChangedItems.ShouldEqual(_collection.DecoratedItems());
            _item.ShouldEqual(_collection.FirstOrDefault());
        }
    }
}