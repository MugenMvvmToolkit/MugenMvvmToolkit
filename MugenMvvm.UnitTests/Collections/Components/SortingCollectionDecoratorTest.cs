using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Collections.Internal;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class SortingCollectionDecoratorTest : UnitTestBase, IComparer<object?>
    {
        #region Properties

        private bool DefaultComparer { get; set; }

        #endregion

        #region Implementation of interfaces

        int IComparer<object?>.Compare(object? x1, object? x2)
        {
            var x = (int) x1!;
            var y = (int) x2!;
            if (DefaultComparer)
                return Comparer<int>.Default.Compare(x, y);
            return y.CompareTo(x);
        }

        #endregion

        #region Methods

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ReorderShouldTrackChanges1(bool defaultComparer)
        {
            DefaultComparer = defaultComparer;
            var observableCollection = new SynchronizedObservableCollection<int>
            {
                1, 2, 3, 4, 5, 6, 6
            };
            var decorator = new SortingCollectionDecorator(this);
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            tracker.ChangedItems.AddRange(observableCollection);
            observableCollection.AddComponent(tracker);
            var items = observableCollection.OrderBy(i => i, this);

            decorator.Reorder();
            tracker.ChangedItems.ShouldEqual(observableCollection.DecorateItems().Cast<int>());
            tracker.ChangedItems.ShouldEqual(items);

            DefaultComparer = !defaultComparer;

            decorator.Reorder();
            tracker.ChangedItems.ShouldEqual(observableCollection.DecorateItems().Cast<int>());
            tracker.ChangedItems.ShouldEqual(items);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddShouldTrackChanges1(bool defaultComparer)
        {
            DefaultComparer = defaultComparer;

            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new SortingCollectionDecorator(this);
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.OrderBy(i => i, this);

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddShouldTrackChanges2(bool defaultComparer)
        {
            DefaultComparer = defaultComparer;

            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new SortingCollectionDecorator(this);
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.OrderBy(i => i, this);

            for (var i = 0; i < 100; i++)
            {
                var id = Guid.NewGuid().GetHashCode();
                observableCollection.Add(id);
                tracker.ChangedItems.ShouldEqual(items);
            }


            for (var i = 0; i < 10; i++)
            {
                var id = Guid.NewGuid().GetHashCode();
                observableCollection.Insert(i, id);
                tracker.ChangedItems.ShouldEqual(items);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReplaceShouldTrackChanges1(bool defaultComparer)
        {
            DefaultComparer = defaultComparer;

            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new SortingCollectionDecorator(this);
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.OrderBy(i => i, this);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.ShouldEqual(items);

            for (var i = 0; i < 10; i++)
            {
                observableCollection[i] = i + Guid.NewGuid().GetHashCode();
                tracker.ChangedItems.ShouldEqual(items);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReplaceShouldTrackChanges2(bool defaultComparer)
        {
            DefaultComparer = defaultComparer;

            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new SortingCollectionDecorator(this);
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.OrderBy(i => i, this);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.ShouldEqual(items);

            for (var i = 0; i < 10; i++)
            {
                for (var j = 10; j < 20; j++)
                {
                    observableCollection[i] = observableCollection[j];
                    tracker.ChangedItems.ShouldEqual(items);
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MoveShouldTrackChanges(bool defaultComparer)
        {
            DefaultComparer = defaultComparer;

            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new SortingCollectionDecorator(this);
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.OrderBy(i => i, this);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.ShouldEqual(items);

            for (var i = 0; i < 10; i++)
            {
                observableCollection.Move(i, i + 1);
                tracker.ChangedItems.ShouldEqual(items);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RemoveShouldTrackChanges1(bool defaultComparer)
        {
            DefaultComparer = defaultComparer;

            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new SortingCollectionDecorator(this);
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.OrderBy(i => i, this);

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


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RemoveShouldTrackChanges2(bool defaultComparer)
        {
            DefaultComparer = defaultComparer;

            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new SortingCollectionDecorator(this);
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.OrderBy(i => i, this);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(Guid.NewGuid().GetHashCode());

            for (var i = 0; i < 100; i++)
            {
                observableCollection.RemoveAt(0);
                tracker.ChangedItems.ShouldEqual(items);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ResetShouldTrackChanges(bool defaultComparer)
        {
            DefaultComparer = defaultComparer;

            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new SortingCollectionDecorator(this);
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.OrderBy(i => i, this);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Reset(new[] {1, 2, 3, 4, 5});
            tracker.ChangedItems.ShouldEqual(items);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ClearShouldTrackChanges(bool defaultComparer)
        {
            DefaultComparer = defaultComparer;

            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new SortingCollectionDecorator(this);
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.OrderBy(i => i, this);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(i);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Clear();
            tracker.ChangedItems.ShouldEqual(items);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ItemChangedShouldTrackChanges(bool defaultComparer)
        {
            var comparer = Comparer<object?>.Create((x1, x2) =>
            {
                var item = (TestCollectionItem) x1!;
                var collectionItem = (TestCollectionItem) x2!;
                if (defaultComparer)
                    return item.Id.CompareTo(collectionItem.Id);
                return collectionItem.Id.CompareTo(item.Id);
            });
            var observableCollection = new SynchronizedObservableCollection<TestCollectionItem>();
            var decorator = new SortingCollectionDecorator(comparer);
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<TestCollectionItem>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.OrderBy(item => item, comparer);

            for (var i = 0; i < 100; i++)
                observableCollection.Add(new TestCollectionItem {Id = i});

            for (var i = 0; i < 100; i++)
            {
                observableCollection[i].Id = i == 0 ? 0 : Guid.NewGuid().GetHashCode();
                observableCollection.RaiseItemChanged(observableCollection[i], null);
                tracker.ChangedItems.ShouldEqual(items);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldTrackChanges1(bool defaultComparer)
        {
            DefaultComparer = defaultComparer;

            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new SortingCollectionDecorator(this);
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.OrderBy(i => i, this);

            observableCollection.Add(1);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Insert(1, 2);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Remove(2);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.RemoveAt(0);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Reset(new[] {1, 2, 3, 4, 5});
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection[0] = 200;
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Move(1, 2);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Clear();
            tracker.ChangedItems.ShouldEqual(items);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldTrackChanges2(bool defaultComparer)
        {
            DefaultComparer = defaultComparer;

            var observableCollection = new SynchronizedObservableCollection<int>();
            var decorator = new SortingCollectionDecorator(this);
            observableCollection.AddComponent(decorator);

            var tracker = new DecoratorObservableCollectionTracker<int>();
            observableCollection.AddComponent(tracker);
            var items = observableCollection.OrderBy(i => i, this);

            observableCollection.Add(Guid.NewGuid().GetHashCode());
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Insert(1, 2);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Remove(2);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.RemoveAt(0);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Reset(new[] {Guid.NewGuid().GetHashCode(), Guid.NewGuid().GetHashCode(), Guid.NewGuid().GetHashCode(), 4, 5});
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection[0] = 200;
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Move(1, 2);
            tracker.ChangedItems.ShouldEqual(items);

            observableCollection.Clear();
            tracker.ChangedItems.ShouldEqual(items);
        }

        #endregion
    }
}