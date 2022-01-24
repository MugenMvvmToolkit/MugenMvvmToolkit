using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.UnitTests.Collections.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class ConvertImmutableCollectionDecoratorTest : CollectionDecoratorTestBase
    {
        private readonly ObservableList<object?> _collection;
        private readonly DecoratedCollectionChangeTracker<object> _tracker;
        private ConvertImmutableCollectionDecorator<object, object> _decorator;

        public ConvertImmutableCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new ObservableList<object?>(ComponentCollectionManager);
            _tracker = new DecoratedCollectionChangeTracker<object>();
            _collection.AddComponent(_tracker);
            _decorator = new ConvertImmutableCollectionDecorator<object, object>(0, true, o => "Item: " + o);
            _collection.AddComponent(_decorator);
            _tracker.Changed += Assert;
        }

        [Fact]
        public void ChangeShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                Assert();
            }

            for (var i = 0; i < _collection.Count; i++)
            {
                _collection.RaiseItemChanged(_collection[i]);
                Assert();
                _tracker.ItemChangedCount.ShouldEqual(i + 1);
            }
        }

        [Fact]
        public void IndexOfShouldBeValid()
        {
            _collection.RemoveComponent(_decorator);
            _decorator = new ConvertImmutableCollectionDecorator<object, object>(0, true, o =>
            {
                if (o is int)
                    return "Item: " + o;
                return o;
            });
            ICollectionDecorator decorator = new ConvertImmutableCollectionDecorator<int, string>(0, false, o => "Item: " + o);
            _collection.AddComponent(decorator);
            _collection.Add("Test1");
            _collection.Add(1);
            _collection.Add(2);
            _collection.Add(2);
            _collection.Add("Test2");

            var indexes = new ItemOrListEditor<int>();

            indexes.Clear();
            decorator.TryGetIndexes(_collection, _collection, _collection[0], false, ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(0);

            indexes.Clear();
            decorator.TryGetIndexes(_collection, _collection, "Item: 1", false, ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(1);
            indexes[0].ShouldEqual(1);

            indexes.Clear();
            decorator.TryGetIndexes(_collection, _collection, "Item: 2", false, ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(2);
            indexes[0].ShouldEqual(2);
            indexes[1].ShouldEqual(3);
        }

        protected override IObservableList<object?> GetCollection() => _collection;

        protected override void Assert()
        {
            _tracker.ChangedItems.ShouldEqual(_collection.DecoratedItems());
            _tracker.ChangedItems.ShouldEqual(Decorate().ToArray());
        }

        private IEnumerable<object?> Decorate() => _collection.Select(_decorator.Converter!);
    }
}