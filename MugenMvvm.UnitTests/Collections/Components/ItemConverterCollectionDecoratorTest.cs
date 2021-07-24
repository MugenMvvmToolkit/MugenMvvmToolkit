using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.UnitTests.Collections.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class ItemConverterCollectionDecoratorTest : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<object> _collection;
        private readonly DecoratorObservableCollectionTracker<object> _tracker;
        private ItemConverterCollectionDecorator<object, object> _decorator;

        public ItemConverterCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            _tracker = new DecoratorObservableCollectionTracker<object>();
            _collection.AddComponent(_tracker);
            _decorator = new ItemConverterCollectionDecorator<object, object>((o, c) => "Item: " + o);
            _collection.AddComponent(_decorator);
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
            {
                _collection.Add(i);
                Assert();
            }

            for (var i = 0; i < _collection.Count; i++)
            {
                _collection.RaiseItemChanged(_collection[i], null);
                Assert();
                _tracker.ItemChangedCount.ShouldEqual(i + 1);
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
        public void IndexOfShouldBeValid()
        {
            _collection.RemoveComponent(_decorator);
            _decorator = new ItemConverterCollectionDecorator<object, object>((o, c) =>
            {
                if (o is int)
                    return "Item: " + o;
                return o;
            });
            ICollectionDecorator decorator = new ImmutableItemConverterCollectionDecorator<int, string>(o => "Item: " + o);
            _collection.AddComponent(decorator);
            _collection.Add("Test1");
            _collection.Add(1);
            _collection.Add(2);
            _collection.Add(2);
            _collection.Add("Test2");

            var indexes = new ItemOrListEditor<int>();

            indexes.Clear();
            decorator.TryGetIndexes(_collection, _collection, _collection[0], ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(0);

            indexes.Clear();
            decorator.TryGetIndexes(_collection, _collection, "Item: 1", ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(1);
            indexes[0].ShouldEqual(1);

            indexes.Clear();
            decorator.TryGetIndexes(_collection, _collection, "Item: 2", ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(2);
            indexes[0].ShouldEqual(2);
            indexes[1].ShouldEqual(3);
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

            for (var i = 0; i < 10; i++)
            {
                _collection.Move(i, i * 2 + 1);
                Assert();
            }

            for (var i = 0; i < 10; i++)
            {
                _collection.Move(i * 2 + 1, i);
                Assert();
            }
        }

        [Fact]
        public void RemoveShouldTrackChanges()
        {
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

            _collection.Reset(new object[] {1, 2, 3, 4, 5});
            Assert();
        }

        [Fact]
        public void ShouldTrackChanges()
        {
            for (var i = 0; i < 4; i++)
            {
                _collection.Add(1);
                Assert();

                _collection.Insert(1, 2);
                Assert();

                _collection.Move(0, 1);
                Assert();

                _collection.Move(1, 0);
                Assert();

                _collection.Remove(2);
                Assert();

                _collection.RemoveAt(0);
                Assert();

                _collection.Reset(new object[] {1, 2, 3, 4, 5, i});
                Assert();

                _collection[0] = 200;
                Assert();

                _collection[3] = 3;
                Assert();

                _collection.Move(0, _collection.Count - 1);
                Assert();

                _collection.Move(0, _collection.Count - 2);
                Assert();

                _collection[i] = i;
                Assert();
            }

            _collection.Clear();
            Assert();
        }

        private void Assert()
        {
            _tracker.ChangedItems.ShouldEqual(_collection.DecoratedItems());
            _tracker.ChangedItems.ShouldEqual(Decorate().ToArray());
        }

        private IEnumerable<object?> Decorate() => _collection.Select(o => _decorator.Converter(o, null));
    }
}