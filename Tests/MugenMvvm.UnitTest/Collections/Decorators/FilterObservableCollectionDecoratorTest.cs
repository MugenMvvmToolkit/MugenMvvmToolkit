using System;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Decorators;
using MugenMvvm.UnitTest.TestInfrastructure;
using MugenMvvm.UnitTest.TestModels;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Collections.Decorators
{
    public class FilterObservableCollectionDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldTrackChangesEmptyFilter()
        {
            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new FilterObservableCollectionDecorator<int>();
            observableCollection.Decorators.Add(decorator);

            var tracker = new ObservableCollectionTracker<int>();
            observableCollection.DecoratorListeners.Add(tracker);

            observableCollection.Add(1);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.Insert(1, 2);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.Remove(2);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.RemoveAt(0);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.Reset(new[] {1, 2, 3, 4, 5});
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection[0] = 200;
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.Move(1, 2);
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();

            observableCollection.Clear();
            tracker.ChangedItems.SequenceEqual(observableCollection).ShouldBeTrue();
        }

        [Fact]
        public void ShouldTrackChangesSetFilter()
        {
            var observableCollection = new SynchronizedObservableCollection<int>
            {
                1, 2, 3, 4, 5
            };
            var decorator = new FilterObservableCollectionDecorator<int>();
            observableCollection.Decorators.Add(decorator);

            var tracker = new ObservableCollectionTracker<int>();
            observableCollection.DecoratorListeners.Add(tracker);

            decorator.Filter = i => i % 2 == 0;

            tracker.ChangedItems.SequenceEqual(observableCollection.DecorateItems()).ShouldBeTrue();
            tracker.ChangedItems.SequenceEqual(observableCollection.Where(decorator.Filter)).ShouldBeTrue();
        }

        [Fact]
        public void ShouldTrackChangesAdd()
        {
            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new FilterObservableCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.Decorators.Add(decorator);

            var tracker = new ObservableCollectionTracker<int>();
            observableCollection.DecoratorListeners.Add(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
            {
                observableCollection.Add(i);
                tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
            }


            for (var i = 0; i < 10; i++)
            {
                observableCollection.Insert(i, i);
                tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
            }
        }

        [Fact]
        public void ShouldTrackChangesReplace1()
        {
            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new FilterObservableCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.Decorators.Add(decorator);

            var tracker = new ObservableCollectionTracker<int>();
            observableCollection.DecoratorListeners.Add(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            for (var i = 0; i < 10; i++)
            {
                observableCollection[i] = i + 101;
                tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
            }
        }

        [Fact]
        public void ShouldTrackChangesReplace2()
        {
            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new FilterObservableCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.Decorators.Add(decorator);

            var tracker = new ObservableCollectionTracker<int>();
            observableCollection.DecoratorListeners.Add(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            for (var i = 0; i < 10; i++)
            {
                for (var j = 10; j < 20; j++)
                {
                    observableCollection[i] = observableCollection[j];
                    tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
                }
            }
        }

        [Fact]
        public void ShouldTrackChangesMove1()
        {
            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new FilterObservableCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.Decorators.Add(decorator);

            var tracker = new ObservableCollectionTracker<int>();
            observableCollection.DecoratorListeners.Add(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            for (var i = 0; i < 10; i++)
            {
                observableCollection.Move(i, i + 1);
                tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
            }
        }

        [Fact]
        public void ShouldTrackChangesMove2()
        {
            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new FilterObservableCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.Decorators.Add(decorator);

            var tracker = new ObservableCollectionTracker<int>();
            observableCollection.DecoratorListeners.Add(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            for (var i = 1; i < 10; i++)
            {
                observableCollection.Move(i, i + i);
                tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
            }
        }

        [Fact]
        public void ShouldTrackChangesRemove()
        {
            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new FilterObservableCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.Decorators.Add(decorator);

            var tracker = new ObservableCollectionTracker<int>();
            observableCollection.DecoratorListeners.Add(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);

            for (var i = 0; i < 20; i++)
            {
                observableCollection.Remove(i);
                tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
            }

            for (var i = 0; i < 10; i++)
            {
                observableCollection.RemoveAt(i);
                tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
            }
        }

        [Fact]
        public void ShouldTrackChangesReset()
        {
            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new FilterObservableCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.Decorators.Add(decorator);

            var tracker = new ObservableCollectionTracker<int>();
            observableCollection.DecoratorListeners.Add(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            observableCollection.Reset(new[] {1, 2, 3, 4, 5});
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
        }

        [Fact]
        public void ShouldTrackChangesClear()
        {
            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new FilterObservableCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.Decorators.Add(decorator);

            var tracker = new ObservableCollectionTracker<int>();
            observableCollection.DecoratorListeners.Add(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            observableCollection.Clear();
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
        }

        [Fact]
        public void ShouldTrackChangesItemChanged()
        {
            var observableCollection = new SynchronizedObservableCollection<CollectionItem>();
            var decorator = new FilterObservableCollectionDecorator<CollectionItem> {Filter = i => i.Id % 2 == 0};
            observableCollection.Decorators.Add(decorator);

            var tracker = new ObservableCollectionTracker<CollectionItem>();
            observableCollection.DecoratorListeners.Add(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(new CollectionItem {Id = i});

            for (var i = 0; i < 100; i++)
            {
                observableCollection[i].Id = i == 0 ? 0 : Guid.NewGuid().GetHashCode();
                observableCollection.RaiseItemChanged(observableCollection[i], null);
                tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
            }
        }

        [Fact]
        public void ShouldTrackChanges()
        {
            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new FilterObservableCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.Decorators.Add(decorator);

            var tracker = new ObservableCollectionTracker<int>();
            observableCollection.DecoratorListeners.Add(tracker);
            var items = observableCollection.Where(decorator.Filter);

            observableCollection.Add(1);
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            observableCollection.Insert(1, 2);
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            observableCollection.Move(0, 1);
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            observableCollection.Remove(2);
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            observableCollection.RemoveAt(0);
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            observableCollection.Reset(new[] {1, 2, 3, 4, 5});
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            observableCollection[0] = 200;
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();

            observableCollection.Clear();
            tracker.ChangedItems.SequenceEqual(items).ShouldBeTrue();
        }

        #endregion
    }
}