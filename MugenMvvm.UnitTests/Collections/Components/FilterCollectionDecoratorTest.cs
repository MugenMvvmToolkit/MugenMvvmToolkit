using System;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Collections.Internal;
using MugenMvvm.UnitTests.Models.Internal;
using Xunit;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class FilterCollectionDecoratorTest : UnitTestBase
    {
        [Fact]
        public void AddShouldTrackChanges()
        {
            var observableCollection = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            var decorator = new FilterCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
            {
                observableCollection.Add(i);
                tracker.ChangedItems.ShouldEqual(items);
            }


            for (var i = 0; i < 10; i++)
            {
                observableCollection.Insert(i, i);
                tracker.ChangedItems.ShouldEqual(items);
            }
        }

        [Fact]
        public void ClearShouldTrackChanges()
        {
            var observableCollection = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            var decorator = new FilterCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Clear();
            tracker.ChangedItems.ShouldEqual(items);
        }

        [Fact]
        public void ItemChangedShouldTrackChanges()
        {
            var observableCollection = new SynchronizedObservableCollection<TestCollectionItem>(ComponentCollectionManager);
            var decorator = new FilterCollectionDecorator<TestCollectionItem> {Filter = i => i.Id % 2 == 0};
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<TestCollectionItem>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(new TestCollectionItem {Id = i});

            for (var i = 0; i < 100; i++)
            {
                observableCollection[i].Id = i == 0 ? 0 : Guid.NewGuid().GetHashCode();
                observableCollection.RaiseItemChanged(observableCollection[i], null);
                tracker.ChangedItems.ShouldEqual(items);
            }
        }

        [Fact]
        public void MoveShouldTrackChanges1()
        {
            var observableCollection = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            var decorator = new FilterCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.ShouldEqual(items);

            for (var i = 0; i < 10; i++)
            {
                observableCollection.Move(i, i + 1);
                tracker.ChangedItems.ShouldEqual(items);
            }
        }

        [Fact]
        public void MoveShouldTrackChanges2()
        {
            var observableCollection = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            var decorator = new FilterCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.ShouldEqual(items);

            for (var i = 1; i < 10; i++)
            {
                observableCollection.Move(i, i + i);
                tracker.ChangedItems.ShouldEqual(items);
            }
        }

        [Fact]
        public void RemoveShouldTrackChanges()
        {
            var observableCollection = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            var decorator = new FilterCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);

            for (var i = 0; i < 20; i++)
            {
                observableCollection.Remove(i);
                tracker.ChangedItems.ShouldEqual(items);
            }

            for (var i = 0; i < 10; i++)
            {
                observableCollection.RemoveAt(i);
                tracker.ChangedItems.ShouldEqual(items);
            }
        }

        [Fact]
        public void ReplaceShouldTrackChanges1()
        {
            var observableCollection = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            var decorator = new FilterCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.ShouldEqual(items);

            for (var i = 0; i < 10; i++)
            {
                observableCollection[i] = i + 101;
                tracker.ChangedItems.ShouldEqual(items);
            }
        }

        [Fact]
        public void ReplaceShouldTrackChanges2()
        {
            var observableCollection = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            var decorator = new FilterCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.ShouldEqual(items);

            for (var i = 0; i < 10; i++)
            for (var j = 10; j < 20; j++)
            {
                observableCollection[i] = observableCollection[j];
                tracker.ChangedItems.ShouldEqual(items);
            }
        }

        [Fact]
        public void ResetShouldTrackChanges()
        {
            var observableCollection = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            var decorator = new FilterCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.Where(decorator.Filter);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Reset(new[] {1, 2, 3, 4, 5});
            tracker.ChangedItems.ShouldEqual(items);
        }

        [Fact]
        public void ShouldTrackChanges()
        {
            var observableCollection = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            var decorator = new FilterCollectionDecorator<int> {Filter = i => i % 2 == 0};
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.Where(decorator.Filter);

            observableCollection.Add(1);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Insert(1, 2);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Move(0, 1);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Remove(2);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.RemoveAt(0);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Reset(new[] {1, 2, 3, 4, 5});
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection[0] = 200;
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Clear();
            tracker.ChangedItems.ShouldEqual(items);
        }

        [Fact]
        public void ShouldTrackChangesEmptyFilter()
        {
            var observableCollection = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            var decorator = new FilterCollectionDecorator<int>();
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);

            observableCollection.Add(1);
            tracker.ChangedItems.ShouldEqual(observableCollection);

            observableCollection.Insert(1, 2);
            tracker.ChangedItems.ShouldEqual(observableCollection);

            observableCollection.Remove(2);
            tracker.ChangedItems.ShouldEqual(observableCollection);

            observableCollection.RemoveAt(0);
            tracker.ChangedItems.ShouldEqual(observableCollection);

            observableCollection.Reset(new[] {1, 2, 3, 4, 5});
            tracker.ChangedItems.ShouldEqual(observableCollection);

            observableCollection[0] = 200;
            tracker.ChangedItems.ShouldEqual(observableCollection);

            observableCollection.Move(1, 2);
            tracker.ChangedItems.ShouldEqual(observableCollection);

            observableCollection.Clear();
            tracker.ChangedItems.ShouldEqual(observableCollection);
        }

        [Fact]
        public void ShouldTrackChangesSetFilter()
        {
            var observableCollection = new SynchronizedObservableCollection<int>(ComponentCollectionManager)
            {
                1, 2, 3, 4, 5
            };
            var decorator = new FilterCollectionDecorator<int>();
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);

            decorator.Filter = i => i % 2 == 0;

            tracker.ChangedItems.ShouldEqual(observableCollection.Decorate().Cast<int>());
            tracker.ChangedItems.ShouldEqual(observableCollection.Where(decorator.Filter));
        }
    }
}