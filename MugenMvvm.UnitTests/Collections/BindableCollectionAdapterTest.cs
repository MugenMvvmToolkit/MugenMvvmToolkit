using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Internal;
using MugenMvvm.Threading;
using MugenMvvm.UnitTests.Collections.Internal;
using MugenMvvm.UnitTests.Internal.Internal;
using MugenMvvm.UnitTests.Threading.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Collections
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
            var adapterCollection = new ObservableCollection<object?>();
            var collectionAdapter = GetCollection(adapterCollection);
            var tracker = new ObservableCollectionTracker<object?>();
            adapterCollection.CollectionChanged += tracker.OnCollectionChanged;
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

            observableCollection.Reset(new object[] { 1, 2, 3, 4, 5 });
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
            observableCollection.GetComponent<ICollectionDecoratorManagerComponent>().ShouldBeType<CollectionDecoratorManager>();
        }

        [Fact]
        public void ShouldTrackChanges2()
        {
            var dispatcherComponent = new TestThreadDispatcherComponent { CanExecuteInline = (_, __) => true };
            using var s = TestComponentSubscriber.Subscribe(dispatcherComponent);

            var observableCollection = new ObservableCollection<object?>();
            var adapterCollection = new ObservableCollection<object?>();
            var collectionAdapter = GetCollection(adapterCollection);
            var tracker = new ObservableCollectionTracker<object?>();
            adapterCollection.CollectionChanged += tracker.OnCollectionChanged;
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
            var adapterCollection = new ObservableCollection<object?>();
            var collectionAdapter = GetCollection(adapterCollection);
            var tracker = new ObservableCollectionTracker<object?>();
            adapterCollection.CollectionChanged += tracker.OnCollectionChanged;
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
            var adapterCollection = new ObservableCollection<object?>();
            var collectionAdapter = GetCollection(adapterCollection);
            var tracker = new ObservableCollectionTracker<object?>();
            adapterCollection.CollectionChanged += tracker.OnCollectionChanged;
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
            var adapterCollection = new ObservableCollection<object?>();
            var collectionAdapter = GetCollection(adapterCollection);
            var tracker = new ObservableCollectionTracker<object?>();
            adapterCollection.CollectionChanged += tracker.OnCollectionChanged;
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
            var adapterCollection = new ObservableCollection<object?>();
            var collectionAdapter = GetCollection(adapterCollection);
            var tracker = new ObservableCollectionTracker<object?>();
            adapterCollection.CollectionChanged += tracker.OnCollectionChanged;
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
            var adapterCollection = new SuspendableObservableCollection<object?>();
            var collectionAdapter = GetCollection(adapterCollection);
            collectionAdapter.BatchSize = 3;
            var tracker = new ObservableCollectionTracker<object?>();
            adapterCollection.CollectionChanged += tracker.OnCollectionChanged;
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

            var invokeCount = 0;
            adapterCollection.CollectionChanged += (sender, args) =>
            {
                ++invokeCount;
                args.Action.ShouldEqual(NotifyCollectionChangedAction.Reset);
            };
            action!();
            invokeCount.ShouldEqual(1);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
            collectionAdapter.SequenceEqual(observableCollection).ShouldBeTrue();
        }

        protected virtual BindableCollectionAdapter GetCollection(IList<object?>? source = null) => new BindableCollectionAdapter(source);

        #endregion

        #region Nested types

        private class SuspendableObservableCollection<T> : ObservableCollection<T>, ISuspendable
        {
            #region Fields

            private int _suspendCount;

            #endregion

            #region Properties

            public bool IsSuspended => _suspendCount != 0;

            #endregion

            #region Implementation of interfaces

            public ActionToken Suspend(object? state = null, IReadOnlyMetadataContext? metadata = null)
            {
                ++_suspendCount;
                return new ActionToken((o, o1) => ((SuspendableObservableCollection<T>)o!).EndSuspend(), this);
            }

            #endregion

            #region Methods

            private void EndSuspend()
            {
                if (--_suspendCount == 0)
                {
                    OnCollectionChanged(Default.ResetCollectionEventArgs);
                    OnPropertyChanged(Default.CountPropertyChangedArgs);
                    OnPropertyChanged(Default.IndexerPropertyChangedArgs);
                }
            }

            protected override void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                if (!IsSuspended)
                    base.OnPropertyChanged(e);
            }

            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                if (!IsSuspended)
                    base.OnCollectionChanged(e);
            }

            #endregion
        }

        #endregion
    }
}