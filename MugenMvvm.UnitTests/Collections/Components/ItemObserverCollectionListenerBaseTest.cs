using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    [Collection(SharedContext)]
    public abstract class ItemObserverCollectionListenerBaseTest : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<TestNotifyPropertyChangedModel> _collection;
        private readonly ItemObserverCollectionListenerBase<object?> _listener;
        private TestNotifyPropertyChangedModel? _currentItem;
        private int _collectionChangedCount;
        private int _itemChangedCount;

        protected ItemObserverCollectionListenerBaseTest(ItemObserverCollectionListenerBase<object?> listener, ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<TestNotifyPropertyChangedModel>(ComponentCollectionManager);
            _listener = listener;
            _collection.Components.Add(_listener);
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
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
            RegisterDisposeToken(ActionToken.FromDisposable(_listener));
        }

        [Fact]
        public void AttachShouldTrackChanges()
        {
            _collection.Components.Remove(_listener);
            var item1 = new TestNotifyPropertyChangedModel();
            var item2 = new TestNotifyPropertyChangedModel();

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

            _collection.Components.Add(_listener);
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
            var item1 = new TestNotifyPropertyChangedModel();
            var item2 = new TestNotifyPropertyChangedModel();

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
            var item1 = new TestNotifyPropertyChangedModel();
            var item2 = new TestNotifyPropertyChangedModel();

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

            _collection.Reset(new[] { item2 });
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
            const int delay = 10;
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
                _collection.Add(new TestNotifyPropertyChangedModel());
            _collectionChangedCount.ShouldEqual(0);
            _itemChangedCount.ShouldEqual(0);

            await Task.Delay(delay * 3);
            _collectionChangedCount.ShouldEqual(1);
            _itemChangedCount.ShouldEqual(0);

            _currentItem = _collection[0];
            for (var i = 0; i < count; i++)
            {
                _currentItem.OnPropertyChanged(nameof(_currentItem.Property));
                _itemChangedCount.ShouldEqual(0);
            }

            WaitCompletion(delay, () => _collectionChangedCount == 1 && _itemChangedCount == 1);
            _collectionChangedCount.ShouldEqual(1);
            _itemChangedCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void AddRemoveShouldTrackChanges(int count)
        {
            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel());
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
                _collection.Add(new TestNotifyPropertyChangedModel());
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
                _collection.Add(new TestNotifyPropertyChangedModel());
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
            _collection.Components.Remove(_listener);
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
                _collection.Add(new TestNotifyPropertyChangedModel());
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

        [Fact]
        public void ShouldSuspendTrackChanges()
        {
            using var t = _collection.TrySuspend();
            var item1 = new TestNotifyPropertyChangedModel();
            var item2 = new TestNotifyPropertyChangedModel();

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

            _collection.Reset(new[] { item2 });
            _itemChangedCount.ShouldEqual(0);
            _collectionChangedCount.ShouldEqual(0);

            _currentItem = item1;
            item1.OnPropertyChanged(nameof(item1.Property));
            _itemChangedCount.ShouldEqual(0);
            _collectionChangedCount.ShouldEqual(0);

            _currentItem = item2;
            item2.OnPropertyChanged(nameof(item1.Property));
            _itemChangedCount.ShouldEqual(0);
            _collectionChangedCount.ShouldEqual(0);

            t.Dispose();
            _collectionChangedCount.ShouldEqual(1);
            _itemChangedCount.ShouldEqual(0);

            _currentItem = item2;
            item2.OnPropertyChanged(nameof(item1.Property));
            _itemChangedCount.ShouldEqual(1);
            _collectionChangedCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldRaiseChangeEvent()
        {
            _listener.ClearObservers();
            var item1 = new TestNotifyPropertyChangedModel();
            var item2 = new TestNotifyPropertyChangedModel();

            int invokeCount = 0;
            _listener.Changed += (sender, args) =>
            {
                sender.ShouldEqual(_listener);
                ++invokeCount;
            };
            _collection.Add(item1);
            invokeCount.ShouldEqual(1);
            _collection.Add(item2);
            invokeCount.ShouldEqual(2);

            item1.OnPropertyChanged(nameof(item1.Property));
            invokeCount.ShouldEqual(3);

            item2.OnPropertyChanged(nameof(item2.Property));
            invokeCount.ShouldEqual(4);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldRaiseChanged(bool force)
        {
            const int delay = 10;
            int invokeCount = 0;
            int delayInvokeCount = 0;
            _listener.ClearObservers();
            _listener.AddObserver(this, (_, _) => true, (_, _) => ++invokeCount);
            _listener.AddObserver(this, (_, _) => true, (_, _) => ++delayInvokeCount, delay);

            _listener.RaiseChanged(force);
            invokeCount.ShouldEqual(1);
            delayInvokeCount.ShouldEqual(force ? 1 : 0);
        }
    }
}