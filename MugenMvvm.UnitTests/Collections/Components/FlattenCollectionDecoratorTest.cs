using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Collections.Internal;
using Xunit;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class FlattenCollectionDecoratorTest : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<int> _sourceCollection;
        private readonly SynchronizedObservableCollection<int> _targetCollection;
        private readonly DecoratorObservableCollectionTracker<object> _tracker;

        public FlattenCollectionDecoratorTest()
        {
            _sourceCollection = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            var decorator = new FilterCollectionDecorator<int> {Filter = i => i % 2 == 0};
            _sourceCollection.AddComponent(decorator);
            _targetCollection = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            _tracker = new DecoratorObservableCollectionTracker<object>();
            _targetCollection.AddComponent(_tracker);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddShouldTrackChanges(bool isHeader)
        {
            AddDecorator(isHeader);
            for (var i = 0; i < 10; i++)
            {
                _sourceCollection.Add(i);
                Assert(isHeader);
                _targetCollection.Add(i);
                Assert(isHeader);
            }

            for (var i = 0; i < 10; i++)
            {
                _sourceCollection.Insert(i, i);
                Assert(isHeader);
                _targetCollection.Insert(i, i);
                Assert(isHeader);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ClearShouldTrackChanges(bool isHeader)
        {
            AddDecorator(isHeader);
            for (var i = 0; i < 100; i++)
            {
                _sourceCollection.Add(i);
                _targetCollection.Add(i);
            }

            Assert(isHeader);

            _sourceCollection.Clear();
            Assert(isHeader);
            _targetCollection.Clear();
            Assert(isHeader);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MoveShouldTrackChanges1(bool isHeader)
        {
            AddDecorator(isHeader);
            for (var i = 0; i < 100; i++)
            {
                _sourceCollection.Add(i);
                _targetCollection.Add(i);
            }

            Assert(isHeader);

            for (var i = 0; i < 10; i++)
            {
                _sourceCollection.Move(i, i + 1);
                Assert(isHeader);
                _targetCollection.Move(i, i + 1);
                Assert(isHeader);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MoveShouldTrackChanges2(bool isHeader)
        {
            AddDecorator(isHeader);

            for (var i = 0; i < 100; i++)
            {
                _sourceCollection.Add(i);
                _targetCollection.Add(i);
            }

            Assert(isHeader);

            for (var i = 1; i < 10; i++)
            {
                _sourceCollection.Move(i, i + i);
                Assert(isHeader);
                _targetCollection.Move(i, i + i);
                Assert(isHeader);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RemoveShouldTrackChanges(bool isHeader)
        {
            AddDecorator(isHeader);

            for (var i = 0; i < 100; i++)
            {
                _sourceCollection.Add(i);
                _targetCollection.Add(i);
            }

            Assert(isHeader);

            for (var i = 0; i < 20; i++)
            {
                _sourceCollection.Remove(i);
                Assert(isHeader);
                _targetCollection.Remove(i);
                Assert(isHeader);
            }

            for (var i = 0; i < 10; i++)
            {
                _sourceCollection.RemoveAt(i);
                Assert(isHeader);
                _targetCollection.RemoveAt(i);
                Assert(isHeader);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReplaceShouldTrackChanges1(bool isHeader)
        {
            AddDecorator(isHeader);

            for (var i = 0; i < 100; i++)
            {
                _sourceCollection.Add(i);
                _targetCollection.Add(i);
            }

            Assert(isHeader);

            for (var i = 0; i < 10; i++)
            {
                _sourceCollection[i] = i + 101;
                Assert(isHeader);
                _targetCollection[i] = i + 101;
                Assert(isHeader);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReplaceShouldTrackChanges2(bool isHeader)
        {
            AddDecorator(isHeader);

            for (var i = 0; i < 100; i++)
            {
                _sourceCollection.Add(i);
                _targetCollection.Add(i);
            }

            Assert(isHeader);

            for (var i = 0; i < 10; i++)
            for (var j = 10; j < 20; j++)
            {
                _sourceCollection[i] = _sourceCollection[j];
                Assert(isHeader);
                _targetCollection[i] = _targetCollection[j];
                Assert(isHeader);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ResetShouldTrackChanges(bool isHeader)
        {
            AddDecorator(isHeader);

            for (var i = 0; i < 100; i++)
            {
                _sourceCollection.Add(i);
                _targetCollection.Add(i);
            }

            Assert(isHeader);

            _sourceCollection.Reset(new[] {1, 2, 3, 4, 5});
            Assert(isHeader);
            _targetCollection.Reset(new[] {1, 2, 3, 4, 5});
            Assert(isHeader);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldTrackChanges(bool isHeader)
        {
            AddDecorator(isHeader);

            _sourceCollection.Add(1);
            Assert(isHeader);
            _targetCollection.Add(1);
            Assert(isHeader);

            _sourceCollection.Insert(1, 2);
            Assert(isHeader);
            _targetCollection.Insert(1, 2);
            Assert(isHeader);

            _sourceCollection.Move(0, 1);
            Assert(isHeader);
            _targetCollection.Move(0, 1);
            Assert(isHeader);

            _sourceCollection.Remove(2);
            Assert(isHeader);
            _targetCollection.Remove(2);
            Assert(isHeader);

            _sourceCollection.RemoveAt(0);
            Assert(isHeader);
            _targetCollection.RemoveAt(0);
            Assert(isHeader);

            _sourceCollection.Reset(new[] {1, 2, 3, 4, 5});
            Assert(isHeader);
            _targetCollection.Reset(new[] {1, 2, 3, 4, 5});
            Assert(isHeader);

            _sourceCollection[0] = 200;
            Assert(isHeader);
            _targetCollection[0] = 200;
            Assert(isHeader);

            _sourceCollection.Clear();
            Assert(isHeader);
            _targetCollection.Clear();
            Assert(isHeader);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldTrackChangesEmptyFilter(bool isHeader)
        {
            AddDecorator(isHeader);

            _sourceCollection.Add(1);
            Assert(isHeader);
            _targetCollection.Add(1);
            Assert(isHeader);

            _sourceCollection.Insert(1, 2);
            Assert(isHeader);
            _targetCollection.Insert(1, 2);
            Assert(isHeader);

            _sourceCollection.Remove(2);
            Assert(isHeader);
            _targetCollection.Remove(2);
            Assert(isHeader);

            _sourceCollection.RemoveAt(0);
            Assert(isHeader);
            _targetCollection.RemoveAt(0);
            Assert(isHeader);

            _sourceCollection.Reset(new[] {1, 2, 3, 4, 5});
            Assert(isHeader);
            _targetCollection.Reset(new[] {1, 2, 3, 4, 5});
            Assert(isHeader);

            _sourceCollection[0] = 200;
            Assert(isHeader);
            _targetCollection[0] = 200;
            Assert(isHeader);

            _sourceCollection.Move(1, 2);
            Assert(isHeader);
            _targetCollection.Move(1, 2);
            Assert(isHeader);

            _sourceCollection.Clear();
            Assert(isHeader);
            _targetCollection.Clear();
            Assert(isHeader);
        }

        private void Assert(bool isHeader)
        {
            _targetCollection.Decorate().ShouldEqual(_tracker.ChangedItems);
            _targetCollection.Decorate().ShouldEqual(isHeader
                ? _sourceCollection.Decorate().Concat(_targetCollection.OfType<object>())
                : _targetCollection.OfType<object>().Concat(_sourceCollection.Decorate()));
        }

        private void AddDecorator(bool isHeader) => _targetCollection.AddComponent(new FlattenCollectionDecorator(_sourceCollection, isHeader));
    }
}