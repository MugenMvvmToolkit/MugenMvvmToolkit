using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Enums;
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
        private readonly CollectionObserverBase _observer;
        private readonly SynchronizedObservableCollection<TestNotifyPropertyChangedModel> _collection;

        protected CollectionObserverBaseTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<TestNotifyPropertyChangedModel>(ComponentCollectionManager);
            _observer = GetObserver();
            _collection.AddComponent(_observer);
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void AddShouldTrackChanges(int count)
        {
            var invokeCount = 0;
            TestNotifyPropertyChangedModel? currentItem = null;
            _observer.AddObserver<TestNotifyPropertyChangedModel>(info => info.IsCollectionEvent, infos =>
            {
                var eventInfo = infos.Single();
                eventInfo.Collection!.ShouldEqual(_collection);
                eventInfo.Items!.ShouldEqual(_collection);
                currentItem.ShouldEqual(eventInfo.Item);
                eventInfo.Action.ShouldEqual(CollectionChangedAction.Add);
                ++invokeCount;
            }, 0, false);

            for (var i = 0; i < count; i++)
            {
                currentItem = new TestNotifyPropertyChangedModel();
                _collection.Add(currentItem);
                invokeCount.ShouldEqual(i + 1);
            }

            foreach (var model in _collection)
                model.HasSubscribers.ShouldBeFalse();
        }

        [Theory]
        [InlineData(10, true)]
        [InlineData(10, false)]
        [InlineData(100, true)]
        [InlineData(100, false)]
        public void AddShouldTrackItemChanges(int count, bool postAdd)
        {
            var invokeCount = 0;
            TestNotifyPropertyChangedModel? currentItem = null;

            void AddObserver()
            {
                _observer.AddObserver<TestNotifyPropertyChangedModel>(info => info.IsMemberChanged(nameof(TestNotifyPropertyChangedModel.Property)), infos =>
                {
                    var eventInfo = infos.Single();
                    eventInfo.Collection!.ShouldEqual(_collection);
                    eventInfo.Items!.ShouldEqual(_collection);
                    currentItem.ShouldEqual(eventInfo.Item);
                    eventInfo.Action.ShouldEqual(CollectionChangedAction.Changed);
                    eventInfo.Member.ShouldEqual(nameof(TestNotifyPropertyChangedModel.Property));
                    ++invokeCount;
                }, 0, true);
            }

            if (!postAdd)
                AddObserver();
            for (var i = 0; i < count; i++)
            {
                currentItem = new TestNotifyPropertyChangedModel();
                _collection.Add(currentItem);
            }

            if (postAdd)
                AddObserver();

            foreach (var model in _collection)
                model.HasSubscribers.ShouldBeTrue();
            invokeCount.ShouldEqual(0);

            for (var i = 0; i < _collection.Count; i++)
            {
                currentItem = _collection[i];
                currentItem.OnPropertyChanged(nameof(currentItem.Property));
                invokeCount.ShouldEqual(i + 1);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void ClearShouldTrackChanges(int count)
        {
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel());

            _observer.AddObserver<TestNotifyPropertyChangedModel>(info => info.IsCollectionEvent, infos =>
            {
                var eventInfo = infos.Single();
                eventInfo.Collection!.ShouldEqual(_collection);
                eventInfo.Items!.ShouldEqual(_collection);
                eventInfo.Item.ShouldBeNull();
                eventInfo.Parameter.ShouldBeNull();
                eventInfo.Action.ShouldEqual(CollectionChangedAction.Reset);
                ++invokeCount;
            }, 0, false);

            _collection.Clear();
            invokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void ClearShouldTrackItemChanges(int count)
        {
            var invokeCount = 0;
            TestNotifyPropertyChangedModel? currentItem = null;

            _observer.AddObserver<TestNotifyPropertyChangedModel>(info => info.IsMemberChanged(nameof(TestNotifyPropertyChangedModel.Property)), infos =>
            {
                var eventInfo = infos.Single();
                eventInfo.Collection!.ShouldEqual(_collection);
                eventInfo.Items!.ShouldEqual(_collection);
                currentItem.ShouldEqual(eventInfo.Item);
                eventInfo.Action.ShouldEqual(CollectionChangedAction.Changed);
                eventInfo.Member.ShouldEqual(nameof(TestNotifyPropertyChangedModel.Property));
                ++invokeCount;
            }, 0, true);
            for (var i = 0; i < count; i++)
            {
                currentItem = new TestNotifyPropertyChangedModel();
                _collection.Add(currentItem);
            }


            foreach (var model in _collection)
                model.HasSubscribers.ShouldBeTrue();

            var oldItems = _collection.ToArray();
            _collection.Clear();

            foreach (var t in oldItems)
            {
                t.HasSubscribers.ShouldBeFalse();
                t.OnPropertyChanged(nameof(currentItem.Property));
                invokeCount.ShouldEqual(0);
            }
        }

        [Fact]
        public void DelayShouldTrackChanged()
        {
            const int count = 100;
            const int delay = 20;

            var invokeCount = 0;
            _observer.AddObserver<TestNotifyPropertyChangedModel>(info => info.IsCollectionEvent, infos =>
            {
                infos.Count.ShouldEqual(count);
                infos.Select(info => info.Item).ShouldContain(_collection);
                infos.All(info => info.Action == CollectionChangedAction.Add).ShouldBeTrue();
                ++invokeCount;
            }, delay, false);

            for (var i = 0; i < count; i++)
            {
                var currentItem = new TestNotifyPropertyChangedModel();
                _collection.Add(currentItem);
                invokeCount.ShouldEqual(0);
            }

            WaitCompletion(delay, () => invokeCount > 0);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void DelayShouldTrackItemChanged()
        {
            const int count = 100;
            const int delay = 20;

            var invokeCount = 0;
            _observer.AddObserver<TestNotifyPropertyChangedModel>(info => info.IsMemberChanged(nameof(TestNotifyPropertyChangedModel.Property)), infos =>
            {
                infos.Count.ShouldEqual(count);
                infos.Select(info => info.Item).ShouldContain(_collection);
                infos.All(info => info.Action == CollectionChangedAction.Changed).ShouldBeTrue();
                infos.All(info => info.Member == nameof(TestNotifyPropertyChangedModel.Property)).ShouldBeTrue();
                ++invokeCount;
            }, delay, true);

            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel());

            for (var i = 0; i < 2; i++)
            {
                foreach (var model in _collection)
                    model.OnPropertyChanged(nameof(TestNotifyPropertyChangedModel.Property));
            }

            WaitCompletion(delay, () => invokeCount > 0);
            invokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void MoveShouldTrackChanges(int count)
        {
            var invokeCount = 0;
            TestNotifyPropertyChangedModel? currentItem = null;
            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel());

            _observer.AddObserver<TestNotifyPropertyChangedModel>(info => info.IsCollectionEvent, infos =>
            {
                var eventInfo = infos.Single();
                eventInfo.Collection!.ShouldEqual(_collection);
                eventInfo.Items!.ShouldEqual(_collection);
                currentItem.ShouldEqual(eventInfo.Item);
                eventInfo.Action.ShouldEqual(CollectionChangedAction.Move);
                ++invokeCount;
            }, 0, false);

            foreach (var model in _collection)
                model.HasSubscribers.ShouldBeFalse();

            for (var i = 0; i < count - 2; i++)
            {
                currentItem = _collection[0];
                _collection.Move(0, i + 1);
                invokeCount.ShouldEqual(i + 1);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void RemoveShouldTrackChanges(int count)
        {
            var invokeCount = 0;
            TestNotifyPropertyChangedModel? currentItem = null;
            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel());

            _observer.AddObserver<TestNotifyPropertyChangedModel>(info => info.IsCollectionEvent, infos =>
            {
                var eventInfo = infos.Single();
                eventInfo.Collection!.ShouldEqual(_collection);
                eventInfo.Items!.ShouldEqual(_collection);
                currentItem.ShouldEqual(eventInfo.Item);
                eventInfo.Action.ShouldEqual(CollectionChangedAction.Remove);
                ++invokeCount;
            }, 0, false);

            for (var i = 0; i < count; i++)
            {
                currentItem = _collection[0];
                _collection.RemoveAt(0);
                invokeCount.ShouldEqual(i + 1);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void RemoveShouldTrackItemChanges(int count)
        {
            var invokeCount = 0;
            TestNotifyPropertyChangedModel? currentItem = null;

            _observer.AddObserver<TestNotifyPropertyChangedModel>(info => info.IsMemberChanged(nameof(TestNotifyPropertyChangedModel.Property)), infos =>
            {
                var eventInfo = infos.Single();
                eventInfo.Collection!.ShouldEqual(_collection);
                eventInfo.Items!.ShouldEqual(_collection);
                currentItem.ShouldEqual(eventInfo.Item);
                eventInfo.Action.ShouldEqual(CollectionChangedAction.Changed);
                eventInfo.Member.ShouldEqual(nameof(TestNotifyPropertyChangedModel.Property));
                ++invokeCount;
            }, 0, true);
            for (var i = 0; i < count; i++)
            {
                currentItem = new TestNotifyPropertyChangedModel();
                _collection.Add(currentItem);
            }


            foreach (var model in _collection)
                model.HasSubscribers.ShouldBeTrue();

            var oldItems = _collection.ToArray();
            for (var i = 0; i < count; i++)
                _collection.RemoveAt(0);

            foreach (var t in oldItems)
            {
                t.HasSubscribers.ShouldBeFalse();
                t.OnPropertyChanged(nameof(currentItem.Property));
                invokeCount.ShouldEqual(0);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void ReplaceShouldTrackChanges(int count)
        {
            var invokeCount = 0;
            TestNotifyPropertyChangedModel? currentItem = null;
            TestNotifyPropertyChangedModel? oldItem = null;
            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel());

            _observer.AddObserver<TestNotifyPropertyChangedModel>(info => info.IsCollectionEvent, infos =>
            {
                var eventInfo = infos.Single();
                eventInfo.Collection!.ShouldEqual(_collection);
                eventInfo.Items!.ShouldEqual(_collection);
                currentItem.ShouldEqual(eventInfo.Item);
                oldItem.ShouldEqual(eventInfo.OldItem);
                eventInfo.Action.ShouldEqual(CollectionChangedAction.Replace);
                ++invokeCount;
            }, 0, false);

            foreach (var model in _collection)
                model.HasSubscribers.ShouldBeFalse();

            for (var i = 0; i < count; i++)
            {
                oldItem = _collection[i];
                currentItem = new TestNotifyPropertyChangedModel();
                _collection[i] = currentItem;
                invokeCount.ShouldEqual(i + 1);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void ReplaceShouldTrackItemChanges(int count)
        {
            var invokeCount = 0;
            TestNotifyPropertyChangedModel? currentItem = null;
            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel());

            _observer.AddObserver<TestNotifyPropertyChangedModel>(info => info.IsMemberChanged(nameof(TestNotifyPropertyChangedModel.Property)), infos =>
            {
                var eventInfo = infos.Single();
                eventInfo.Collection!.ShouldEqual(_collection);
                eventInfo.Items!.ShouldEqual(_collection);
                currentItem.ShouldEqual(eventInfo.Item);
                eventInfo.Action.ShouldEqual(CollectionChangedAction.Changed);
                eventInfo.Member.ShouldEqual(nameof(TestNotifyPropertyChangedModel.Property));
                ++invokeCount;
            }, 0, true);

            foreach (var model in _collection)
                model.HasSubscribers.ShouldBeTrue();

            for (var i = 0; i < count; i++)
            {
                var oldItem = _collection[i];
                currentItem = new TestNotifyPropertyChangedModel();
                _collection[i] = currentItem;
                oldItem.HasSubscribers.ShouldBeFalse();
                currentItem.HasSubscribers.ShouldBeTrue();
                currentItem.OnPropertyChanged(nameof(TestNotifyPropertyChangedModel.Property));
                invokeCount.ShouldEqual(i + 1);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void ResetShouldTrackChanges(int count)
        {
            var resetItems = new List<TestNotifyPropertyChangedModel> { new() };
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel());

            _observer.AddObserver<TestNotifyPropertyChangedModel>(info => info.IsCollectionEvent, infos =>
            {
                var eventInfo = infos.Single();
                eventInfo.Collection!.ShouldEqual(_collection);
                eventInfo.Items!.ShouldEqual(_collection);
                eventInfo.Item.ShouldBeNull();
                eventInfo.Parameter.ShouldEqual(resetItems);
                eventInfo.Action.ShouldEqual(CollectionChangedAction.Reset);
                ++invokeCount;
            }, 0, false);

            resetItems.AddRange(_collection.Take(2));
            _collection.Reset(resetItems);
            invokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void ResetShouldTrackItemChanges(int count)
        {
            var resetItems = new List<TestNotifyPropertyChangedModel> { new() };
            var invokeCount = 0;
            TestNotifyPropertyChangedModel? currentItem = null;
            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel());

            _observer.AddObserver<TestNotifyPropertyChangedModel>(info => info.IsMemberChanged(nameof(TestNotifyPropertyChangedModel.Property)), infos =>
            {
                var eventInfo = infos.Single();
                eventInfo.Collection!.ShouldEqual(_collection);
                eventInfo.Items!.ShouldEqual(_collection);
                currentItem.ShouldEqual(eventInfo.Item);
                eventInfo.Action.ShouldEqual(CollectionChangedAction.Changed);
                eventInfo.Member.ShouldEqual(nameof(TestNotifyPropertyChangedModel.Property));
                ++invokeCount;
            }, 0, true);

            foreach (var model in _collection)
                model.HasSubscribers.ShouldBeTrue();

            var oldItems = _collection.ToList();
            _collection.Clear();

            resetItems.AddRange(_collection.Take(2));
            _collection.Reset(resetItems);

            var currentInvokeCount = 0;
            foreach (var t in oldItems)
            {
                if (_collection.Contains(t))
                {
                    currentItem = t;
                    t.HasSubscribers.ShouldBeTrue();
                    t.OnPropertyChanged(nameof(currentItem.Property));
                    ++currentInvokeCount;
                    invokeCount.ShouldEqual(currentInvokeCount);
                }
                else
                {
                    t.HasSubscribers.ShouldBeFalse();
                    t.OnPropertyChanged(nameof(currentItem.Property));
                    invokeCount.ShouldEqual(0);
                }
            }

            foreach (var t in _collection)
            {
                currentItem = t;
                t.HasSubscribers.ShouldBeTrue();
                t.OnPropertyChanged(nameof(currentItem.Property));
                ++currentInvokeCount;
                invokeCount.ShouldEqual(currentInvokeCount);
            }
        }

        [Fact]
        public void ShouldSuspendTrackChanges()
        {
            const int count = 100;
            var invokeCount = 0;

            _observer.AddObserver<TestNotifyPropertyChangedModel>(info => info.IsCollectionEvent, infos =>
            {
                var eventInfo = infos.Single();
                eventInfo.Collection!.ShouldEqual(_collection);
                eventInfo.Items!.ShouldEqual(_collection);
                eventInfo.Item.ShouldBeNull();
                ((IEnumerable<object>)eventInfo.Parameter!).ShouldEqual(_collection);
                eventInfo.Action.ShouldEqual(CollectionChangedAction.Reset);
                ++invokeCount;
            }, 0, false);

            var token = _collection.TrySuspend();
            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel());
            invokeCount.ShouldEqual(0);
            token.Dispose();
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldSuspendTrackItemChanges()
        {
            const int count = 100;
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel());

            _observer.AddObserver<TestNotifyPropertyChangedModel>(info => info.IsMemberOrCollectionChanged(nameof(TestNotifyPropertyChangedModel.Property)), infos =>
            {
                var eventInfo = infos.Single();
                eventInfo.Collection!.ShouldEqual(_collection);
                eventInfo.Items!.ShouldEqual(_collection);
                eventInfo.Item.ShouldBeNull();
                ((IEnumerable<object>)eventInfo.Parameter!).ShouldEqual(_collection);
                eventInfo.Action.ShouldEqual(CollectionChangedAction.Reset);
                ++invokeCount;
            }, 0, true);

            var token = _collection.TrySuspend();
            foreach (var model in _collection)
                model.OnPropertyChanged(nameof(model.Property));

            invokeCount.ShouldEqual(0);
            token.Dispose();
            invokeCount.ShouldEqual(1);
        }

        [Fact(Skip = ReleaseTest)]
        public void WeakObserverShouldBeValid()
        {
            var weakReference = WeakTest();
            GcCollect();
            GcCollect();
            GcCollect();

            foreach (var item in _collection)
                item.OnPropertyChanged(nameof(item.Property));
            _collection.Add(new TestNotifyPropertyChangedModel());
            weakReference.IsAlive.ShouldBeFalse();
        }

        protected abstract CollectionObserverBase GetObserver();

        private WeakReference WeakTest()
        {
            var target = new object();
            var weakReference = new WeakReference(target);
            var invokeCount = 0;
            _observer.AddObserverWeak<object, object>(target, (_, _) => true, (_, _) => ++invokeCount, 0, true);
            _collection.Add(new TestNotifyPropertyChangedModel());
            invokeCount.ShouldEqual(1);

            _collection[0].OnPropertyChanged(nameof(TestNotifyPropertyChangedModel.Property));
            invokeCount.ShouldEqual(2);
            return weakReference;
        }
    }
}