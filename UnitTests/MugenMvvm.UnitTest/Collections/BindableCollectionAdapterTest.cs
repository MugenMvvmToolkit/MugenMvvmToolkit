using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Threading;
using MugenMvvm.UnitTest.Collections.Internal;
using MugenMvvm.UnitTest.Internal.Internal;
using MugenMvvm.UnitTest.Threading.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Collections
{
    public class BindableCollectionAdapterTest : UnitTestBase
    {
        #region Methods

        protected override void InitializeThreadDispatcher()
        {
            var threadDispatcher = new ThreadDispatcher();
            MugenService.Configuration.InitializeInstance<IThreadDispatcher>(threadDispatcher);
        }

        [Fact]
        public void ShouldTrackChanges1()
        {
            var dispatcherComponent = new TestThreadDispatcherComponent { CanExecuteInline = (_, __) => true };
            using var s = TestComponentSubscriber.Subscribe(dispatcherComponent);

            var observableCollection = new SynchronizedObservableCollection<object?>();
            var collectionAdapter = GetCollection();
            var tracker = new ObservableCollectionTracker<object?>();
            collectionAdapter.CollectionChanged += tracker.OnCollectionChanged;
            collectionAdapter.Collection = observableCollection;

            observableCollection.Add(1);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.Insert(1, 2);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.Remove(2);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.RemoveAt(0);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.Reset(new Object[] { 1, 2, 3, 4, 5 });
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection[0] = 200;
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.Move(1, 2);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.Clear();
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();

            collectionAdapter.Collection = null;
            collectionAdapter.ShouldBeEmpty();
            observableCollection.Components.Count.ShouldEqual(1);
            observableCollection.GetComponent<ICollectionDecoratorManagerComponent>().ShouldEqual(CollectionDecoratorManager.Instance);
        }

        [Fact]
        public void ShouldTrackChanges2()
        {
            var dispatcherComponent = new TestThreadDispatcherComponent { CanExecuteInline = (_, __) => true };
            using var s = TestComponentSubscriber.Subscribe(dispatcherComponent);

            var observableCollection = new ObservableCollection<object?>();
            var collectionAdapter = GetCollection();
            var tracker = new ObservableCollectionTracker<object?>();
            collectionAdapter.CollectionChanged += tracker.OnCollectionChanged;
            collectionAdapter.Collection = observableCollection;

            observableCollection.Add(1);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.Insert(1, 2);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.Remove(2);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.RemoveAt(0);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.Clear();
            observableCollection.AddRange(new object[] { 1, 2, 3, 4, 5 });
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection[0] = 200;
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.Move(1, 2);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.Clear();
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();

            collectionAdapter.Collection = null;
            collectionAdapter.ShouldBeEmpty();
            observableCollection.Add(1);
            collectionAdapter.ShouldBeEmpty();
        }

        [Fact]
        public void ShouldTrackChangesThreadDispatcher1()
        {
            Action? action = null;
            var dispatcherComponent = new TestThreadDispatcherComponent
            {
                CanExecuteInline = (_, __) => false,
                Execute = (action1, mode, arg3, _) =>
                {
                    action += () => action1(arg3);
                    return true;
                }
            };
            using var s = TestComponentSubscriber.Subscribe(dispatcherComponent);

            var observableCollection = new SynchronizedObservableCollection<object?>();
            var collectionAdapter = GetCollection();
            var tracker = new ObservableCollectionTracker<object?>();
            collectionAdapter.CollectionChanged += tracker.OnCollectionChanged;
            collectionAdapter.Collection = observableCollection;

            observableCollection.Add(1);
            observableCollection.Insert(1, 2);
            observableCollection.Remove(2);
            observableCollection.RemoveAt(0);
            observableCollection.Reset(new object?[] { 1, 2, 3, 4, 5 });
            observableCollection[0] = 200;
            observableCollection.Move(1, 2);
            tracker.ChangedItems.Count.ShouldEqual(0);
            collectionAdapter.Count.ShouldEqual(0);

            action.ShouldNotBeNull();
            action!();
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();
        }

        [Fact]
        public void ShouldTrackChangesThreadDispatcher2()
        {
            Action? action = null;
            var dispatcherComponent = new TestThreadDispatcherComponent
            {
                CanExecuteInline = (_, __) => false,
                Execute = (action1, mode, arg3, _) =>
                {
                    action += () => action1(arg3);
                    return true;
                }
            };
            using var s = TestComponentSubscriber.Subscribe(dispatcherComponent);

            var observableCollection = new ObservableCollection<object?>();
            var collectionAdapter = GetCollection();
            var tracker = new ObservableCollectionTracker<object?>();
            collectionAdapter.CollectionChanged += tracker.OnCollectionChanged;
            collectionAdapter.Collection = observableCollection;

            observableCollection.Add(1);
            observableCollection.Insert(1, 2);
            observableCollection.Remove(2);
            observableCollection.RemoveAt(0);
            observableCollection.Clear();
            observableCollection.AddRange(new object?[] { 1, 2, 3, 4, 5 });
            observableCollection[0] = 200;
            observableCollection.Move(1, 2);
            tracker.ChangedItems.Count.ShouldEqual(0);
            collectionAdapter.Count.ShouldEqual(0);

            action.ShouldNotBeNull();
            action!();
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();
        }

        [Fact]
        public void ShouldTrackChangesBatchUpdate1()
        {
            var dispatcherComponent = new TestThreadDispatcherComponent { CanExecuteInline = (_, __) => true };
            using var s = TestComponentSubscriber.Subscribe(dispatcherComponent);

            var observableCollection = new SynchronizedObservableCollection<object?>();
            var collectionAdapter = GetCollection();
            var tracker = new ObservableCollectionTracker<object?>();
            collectionAdapter.CollectionChanged += tracker.OnCollectionChanged;
            collectionAdapter.Collection = observableCollection;

            using (observableCollection.BeginBatchUpdate())
            {
                observableCollection.Add(1);
                observableCollection.Insert(1, 2);
                observableCollection.Remove(2);
                observableCollection.RemoveAt(0);
                observableCollection.Reset(new object?[] { 1, 2, 3, 4, 5 });
                observableCollection[0] = 200;
                observableCollection.Move(1, 2);
                tracker.ChangedItems.Count.ShouldEqual(0);
                collectionAdapter.Count.ShouldEqual(0);
            }

            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();
        }

        [Fact]
        public void ShouldTrackChangesBatchUpdate2()
        {
            var dispatcherComponent = new TestThreadDispatcherComponent { CanExecuteInline = (_, __) => true };
            using var s = TestComponentSubscriber.Subscribe(dispatcherComponent);

            var observableCollection = new SynchronizedObservableCollection<object?>();
            var collectionAdapter = GetCollection();
            var tracker = new ObservableCollectionTracker<object?>();
            collectionAdapter.CollectionChanged += tracker.OnCollectionChanged;
            collectionAdapter.Collection = observableCollection;

            using (observableCollection.BeginBatchUpdate())
            {
                observableCollection.Add(1);
                observableCollection.Insert(1, 2);
                observableCollection.Add(3);
                observableCollection.Add(4);
                observableCollection.Remove(4);
                observableCollection.RemoveAt(2);
                observableCollection.Move(0, 1);
                observableCollection[0] = 200;

                tracker.ChangedItems.Count.ShouldEqual(0);
                collectionAdapter.Count.ShouldEqual(0);
            }

            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();
        }

        [Fact]
        public void ShouldTrackChangesResetLimit()
        {
            Action? action = null;
            var dispatcherComponent = new TestThreadDispatcherComponent
            {
                CanExecuteInline = (_, __) => false,
                Execute = (action1, mode, arg3, _) =>
                {
                    action += () => action1(arg3);
                    return true;
                }
            };
            using var s = TestComponentSubscriber.Subscribe(dispatcherComponent);

            var observableCollection = new SynchronizedObservableCollection<object?>();
            var collectionAdapter = GetCollection();
            collectionAdapter.EventsResetLimit = 3;
            var tracker = new ObservableCollectionTracker<object?>();
            collectionAdapter.CollectionChanged += tracker.OnCollectionChanged;
            collectionAdapter.Collection = observableCollection;

            observableCollection.Add(1);
            observableCollection.Insert(1, 2);
            observableCollection.Add(3);
            observableCollection.Add(4);
            observableCollection.Remove(4);
            observableCollection.RemoveAt(2);
            observableCollection.Move(0, 1);
            observableCollection[0] = 200;

            tracker.ChangedItems.Count.ShouldEqual(0);
            collectionAdapter.Count.ShouldEqual(0);

            int invokeCount = 0;
            collectionAdapter.CollectionChanged += (sender, args) =>
            {
                ++invokeCount;
                args.Action.ShouldEqual(NotifyCollectionChangedAction.Reset);
            };
            action!();
            invokeCount.ShouldEqual(1);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();
        }

        protected virtual BindableCollectionAdapter GetCollection()
        {
            return new BindableCollectionAdapter();
        }

        #endregion
    }
}