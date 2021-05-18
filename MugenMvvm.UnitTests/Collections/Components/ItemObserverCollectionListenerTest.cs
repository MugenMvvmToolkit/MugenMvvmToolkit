using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class ItemObserverCollectionListenerTest : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<TestNotifyPropertyChangedModel> _collection;
        private readonly ItemObserverCollectionListener<TestNotifyPropertyChangedModel> _listener;
        private TestNotifyPropertyChangedModel? _currentItem;
        private int _collectionChangedCount;
        private int _itemChangedCount;

        public ItemObserverCollectionListenerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<TestNotifyPropertyChangedModel>(ComponentCollectionManager);
            _listener = new ItemObserverCollectionListener<TestNotifyPropertyChangedModel>();
            _collection.AddComponent(_listener);
            _listener.AddObserver(this, (s, info) => s == this && info.IsCollectionEvent, (s, item) =>
            {
                s.ShouldEqual(this);
                item.ShouldBeNull();
                ++_collectionChangedCount;
            });
            _listener.AddObserver(this, (s, info) => s == this && info.IsMemberChanged(nameof(_currentItem.Property)), (s, item) =>
            {
                s.ShouldEqual(this);
                _currentItem.ShouldEqual(item);
                ++_itemChangedCount;
            });
        }

        [Fact]
        public void AttachShouldTrackChanges()
        {
            _collection.RemoveComponent(_listener);
            var item1 = new TestNotifyPropertyChangedModel {ThreadDispatcher = ThreadDispatcher};
            var item2 = new TestNotifyPropertyChangedModel {ThreadDispatcher = ThreadDispatcher};

            _collection.Add(item1);
            _collection.Add(item2);
            _collectionChangedCount.ShouldEqual(0);
            _itemChangedCount.ShouldEqual(0);

            _currentItem = item1;
            item1.OnPropertyChanged(nameof(item1.Property));
            _itemChangedCount.ShouldEqual(0);
            _collectionChangedCount.ShouldEqual(0);

            _currentItem = item2;
            item2.OnPropertyChanged(nameof(item1.Property));
            _itemChangedCount.ShouldEqual(0);
            _collectionChangedCount.ShouldEqual(0);

            _collection.AddComponent(_listener);
            _itemChangedCount.ShouldEqual(0);
            _collectionChangedCount.ShouldEqual(1);

            _currentItem = item1;
            item1.OnPropertyChanged(nameof(item1.Property));
            _itemChangedCount.ShouldEqual(1);
            _collectionChangedCount.ShouldEqual(1);

            _currentItem = item2;
            item2.OnPropertyChanged(nameof(item1.Property));
            _itemChangedCount.ShouldEqual(2);
            _collectionChangedCount.ShouldEqual(1);
        }

        [Fact]
        public void ReplaceShouldTrackChanges()
        {
            var item1 = new TestNotifyPropertyChangedModel {ThreadDispatcher = ThreadDispatcher};
            var item2 = new TestNotifyPropertyChangedModel {ThreadDispatcher = ThreadDispatcher};

            _collection.Add(item1);
            _collectionChangedCount.ShouldEqual(1);
            _itemChangedCount.ShouldEqual(0);

            _currentItem = item1;
            item1.OnPropertyChanged(nameof(item1.Property));
            _itemChangedCount.ShouldEqual(1);
            _collectionChangedCount.ShouldEqual(1);

            _collection[0] = item2;
            _itemChangedCount.ShouldEqual(1);
            _collectionChangedCount.ShouldEqual(2);

            item1.OnPropertyChanged(nameof(item1.Property));
            _itemChangedCount.ShouldEqual(1);
            _collectionChangedCount.ShouldEqual(2);

            _currentItem = item2;
            item2.OnPropertyChanged(nameof(item1.Property));
            _itemChangedCount.ShouldEqual(2);
            _collectionChangedCount.ShouldEqual(2);
        }

        [Fact]
        public void ResetShouldTrackChanges()
        {
            var item1 = new TestNotifyPropertyChangedModel {ThreadDispatcher = ThreadDispatcher};
            var item2 = new TestNotifyPropertyChangedModel {ThreadDispatcher = ThreadDispatcher};

            _collection.Add(item1);
            _collection.Add(item2);
            _collectionChangedCount.ShouldEqual(2);
            _itemChangedCount.ShouldEqual(0);

            _currentItem = item1;
            item1.OnPropertyChanged(nameof(item1.Property));
            _itemChangedCount.ShouldEqual(1);
            _collectionChangedCount.ShouldEqual(2);

            _currentItem = item2;
            item2.OnPropertyChanged(nameof(item1.Property));
            _itemChangedCount.ShouldEqual(2);
            _collectionChangedCount.ShouldEqual(2);

            _collection.Reset(new[] {item2});
            _itemChangedCount.ShouldEqual(2);
            _collectionChangedCount.ShouldEqual(3);

            _currentItem = item1;
            item1.OnPropertyChanged(nameof(item1.Property));
            _itemChangedCount.ShouldEqual(2);
            _collectionChangedCount.ShouldEqual(3);

            _currentItem = item2;
            item2.OnPropertyChanged(nameof(item1.Property));
            _itemChangedCount.ShouldEqual(3);
            _collectionChangedCount.ShouldEqual(3);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public async Task DelayShouldCancelPreviousChanges(int count)
        {
            const int delay = 20;
            _listener.ClearObservers();
            _listener.AddObserver(this, (s, info) => s == this && info.IsCollectionEvent, (s, item) =>
            {
                s.ShouldEqual(this);
                item.ShouldBeNull();
                ++_collectionChangedCount;
            }, delay);
            _listener.AddObserver(this, (s, info) => s == this && info.IsMemberChanged(nameof(_currentItem.Property)), (s, item) =>
            {
                s.ShouldEqual(this);
                _currentItem.ShouldEqual(item);
                ++_itemChangedCount;
            }, delay);

            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel {ThreadDispatcher = ThreadDispatcher});
            _collectionChangedCount.ShouldEqual(0);
            _itemChangedCount.ShouldEqual(0);

            await Task.Delay(delay + 30);
            _collectionChangedCount.ShouldEqual(1);
            _itemChangedCount.ShouldEqual(0);

            _currentItem = _collection[0];
            for (var i = 0; i < count; i++)
            {
                _currentItem.OnPropertyChanged(nameof(_currentItem.Property));
                _itemChangedCount.ShouldEqual(0);
            }

            await Task.Delay(delay + 30);
            _collectionChangedCount.ShouldEqual(1);
            _itemChangedCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void AddRemoveShouldTrackChanges(int count)
        {
            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel {ThreadDispatcher = ThreadDispatcher});
            _collectionChangedCount.ShouldEqual(count);
            _itemChangedCount.ShouldEqual(0);

            for (var i = 0; i < count; i++)
            {
                _currentItem = _collection[i];
                _currentItem.OnPropertyChanged(nameof(_currentItem.Property));
                _itemChangedCount.ShouldEqual(i + 1);
            }

            _collectionChangedCount = 0;
            _itemChangedCount = 0;

            var items = _collection.ToArray();
            for (var i = 0; i < count; i++)
                _collection.RemoveAt(0);

            _collectionChangedCount.ShouldEqual(count);
            _itemChangedCount.ShouldEqual(0);

            foreach (var item in items)
            {
                _currentItem = item;
                item.OnPropertyChanged(nameof(item.PropertyChanged));
            }

            _collectionChangedCount.ShouldEqual(count);
            _itemChangedCount.ShouldEqual(0);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void AddClearShouldTrackChanges(int count)
        {
            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel {ThreadDispatcher = ThreadDispatcher});
            _collectionChangedCount.ShouldEqual(count);
            _itemChangedCount.ShouldEqual(0);

            for (var i = 0; i < count; i++)
            {
                _currentItem = _collection[i];
                _currentItem.OnPropertyChanged(nameof(_currentItem.Property));
                _itemChangedCount.ShouldEqual(i + 1);
            }

            _collectionChangedCount = 0;
            _itemChangedCount = 0;

            var items = _collection.ToArray();
            _collection.Clear();
            _collectionChangedCount.ShouldEqual(1);
            _itemChangedCount.ShouldEqual(0);

            foreach (var item in items)
            {
                _currentItem = item;
                item.OnPropertyChanged(nameof(item.PropertyChanged));
            }

            _collectionChangedCount.ShouldEqual(1);
            _itemChangedCount.ShouldEqual(0);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void AddDetachShouldTrackChanges(int count)
        {
            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel {ThreadDispatcher = ThreadDispatcher});
            _collectionChangedCount.ShouldEqual(count);
            _itemChangedCount.ShouldEqual(0);

            for (var i = 0; i < count; i++)
            {
                _currentItem = _collection[i];
                _currentItem.OnPropertyChanged(nameof(_currentItem.Property));
                _itemChangedCount.ShouldEqual(i + 1);
            }

            _collectionChangedCount = 0;
            _itemChangedCount = 0;

            var items = _collection.ToArray();
            _collection.RemoveComponent(_listener);
            _collectionChangedCount.ShouldEqual(0);
            _itemChangedCount.ShouldEqual(0);

            foreach (var item in items)
            {
                _currentItem = item;
                item.OnPropertyChanged(nameof(item.PropertyChanged));
            }

            _collectionChangedCount.ShouldEqual(0);
            _itemChangedCount.ShouldEqual(0);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void MoveShouldTrackChanges(int count)
        {
            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel {ThreadDispatcher = ThreadDispatcher});
            _collectionChangedCount.ShouldEqual(count);
            _itemChangedCount.ShouldEqual(0);

            _collectionChangedCount = 0;
            _itemChangedCount = 0;

            for (var i = 0; i < count; i++)
            {
                _collection.Move(0, 1);
                _collectionChangedCount.ShouldEqual(i + 1);
                _itemChangedCount.ShouldEqual(0);
            }
        }
    }
}