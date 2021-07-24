using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    [Collection(SharedContext)]
    public abstract class CollectionObserverBaseTest : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<TestNotifyPropertyChangedModel> _collection;
        private readonly CollectionObserverBase _listener;
        private TestNotifyPropertyChangedModel? _currentItem;
        private int _collectionChangedCount;
        private int _itemChangedCount;

        protected CollectionObserverBaseTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<TestNotifyPropertyChangedModel>(ComponentCollectionManager);
            _listener = GetObserver();
            _listener.AddCollectionObserver(this, (s, c) =>
            {
                s.ShouldEqual(this);
                c.ShouldEqual(_collection);
                ++_collectionChangedCount;
            });
            _listener.AddItemObserver<object>(info => info.IsMemberChanged(nameof(_currentItem.Property)), item =>
            {
                item.Member.ShouldEqual(nameof(_currentItem.Property));
                _currentItem.ShouldEqual(item.Item);
                ++_itemChangedCount;
            });
            _collection.Components.Add(_listener);
            _itemChangedCount = 0;
            _collectionChangedCount = 0;
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
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

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void DelayShouldTrackAllEvents(int count)
        {
            const int delay = 10;
            var changedItems = new List<object>();
            _listener.ClearObservers();
            _listener.AddCollectionObserver(this, (s, c) =>
            {
                s.ShouldEqual(this);
                c.ShouldEqual(_collection);
                ++_collectionChangedCount;
            }, delay);
            _listener.AddItemObserver<object>(info => info.IsMemberChanged(nameof(_currentItem.Property)), item =>
            {
                item.Member.ShouldEqual(nameof(_currentItem.Property));
                changedItems.Add(item.Item!);
            }, delay);
            WaitCompletion(delay, () => _collectionChangedCount == 1);
            _collectionChangedCount = 0;
            changedItems.Count.ShouldEqual(0);

            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel());
            _collectionChangedCount.ShouldEqual(0);
            changedItems.Count.ShouldEqual(0);

            WaitCompletion(delay, () => _collectionChangedCount == 1);
            _collectionChangedCount.ShouldEqual(1);
            changedItems.Count.ShouldEqual(0);

            for (var i = 0; i < count; i++)
            {
                _collection[i].OnPropertyChanged(nameof(_currentItem.Property));
                changedItems.Count.ShouldEqual(0);
            }

            WaitCompletion(delay, () => changedItems.Count == count);
            _collectionChangedCount.ShouldEqual(1);
            changedItems.Count.ShouldEqual(count);
            foreach (var item in _collection)
                changedItems.Remove(item);
            changedItems.Count.ShouldEqual(0);
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RaiseChangedShouldNotify(bool force)
        {
            const int delay = 10;
            var invokeCount = 0;
            var delayInvokeCount = 0;
            _listener.ClearObservers();
            _listener.AddItemObserver<object>(_ => true, _ => ++invokeCount);
            _listener.AddItemObserver<object>(_ => true, _ => ++delayInvokeCount, delay);
            invokeCount = 0;
            delayInvokeCount = 0;

            _listener.RaiseChanged(force);
            invokeCount.ShouldEqual(1);
            delayInvokeCount.ShouldEqual(force ? 1 : 0);
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

        [Fact]
        public void ShouldSubscribeOnPropertyChangeHasObservers()
        {
            _collection.RemoveComponent(_listener);
            var observer = GetObserver();
            _collection.AddComponent(observer);

            var item1 = new TestNotifyPropertyChangedModel();
            _collection.Add(item1);
            item1.HasSubscribers.ShouldBeFalse();

            observer.AddCollectionObserver(this, (_, _) => { });
            item1.HasSubscribers.ShouldBeFalse();

            observer.AddItemObserver<object>(_ => true, _ => { });
            item1.HasSubscribers.ShouldBeTrue();
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

            _collection.Reset(new[] {item2});
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

        [Fact(Skip = ReleaseTest)]
        public void WeakObserverShouldBeValid()
        {
            WeakTest(out var ref1, out var ref2);
            _itemChangedCount = 0;
            _collectionChangedCount = 0;
            GcCollect();
            GcCollect();
            GcCollect();

            foreach (var item in _collection)
                item.OnPropertyChanged(nameof(_currentItem.Property));
            _collection.Add(new TestNotifyPropertyChangedModel());
            _itemChangedCount.ShouldEqual(0);
            _collectionChangedCount.ShouldEqual(0);
            ref1.IsAlive.ShouldBeFalse();
            ref2.IsAlive.ShouldBeFalse();
        }

        protected abstract CollectionObserverBase GetObserver();
        
        private void WeakTest(out WeakReference weakReference1, out WeakReference weakReference2)
        {
            var target1 = new object();
            var target2 = new object();
            weakReference1 = new WeakReference(target1);
            weakReference2 = new WeakReference(target2);
            var ref1 = weakReference1;
            var ref2 = weakReference2;

            var count = 10;
            _listener.ClearObservers();
            _listener.AddCollectionObserverWeak(target1, (s, c) =>
            {
                s.ShouldNotBeNull();
                s.ShouldEqual(ref1.Target);
                c.ShouldEqual(_collection);
                ++_collectionChangedCount;
            });
            _listener.AddItemObserverWeak<object, object>(target2, (s, info) =>
            {
                s.ShouldNotBeNull();
                s.ShouldEqual(ref2.Target);
                return info.IsMemberChanged(nameof(_currentItem.Property));
            }, (s, item) =>
            {
                s.ShouldNotBeNull();
                s.ShouldEqual(ref2.Target);
                item.Member.ShouldEqual(nameof(_currentItem.Property));
                item.Item.ShouldEqual(_currentItem);
                ++_itemChangedCount;
            });

            _collectionChangedCount = 0;
            _itemChangedCount.ShouldEqual(0);

            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel());

            _collectionChangedCount.ShouldEqual(count);
            _itemChangedCount.ShouldEqual(0);

            for (var i = 0; i < count; i++)
            {
                _currentItem = _collection[i];
                _currentItem.OnPropertyChanged(nameof(_currentItem.Property));
            }

            _collectionChangedCount.ShouldEqual(count);
            _itemChangedCount.ShouldEqual(count);
        }
    }
}