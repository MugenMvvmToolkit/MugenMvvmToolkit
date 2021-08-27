using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.UnitTests.Collections.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class ConvertCollectionDecoratorTest : CollectionDecoratorTestBase
    {
        private readonly SynchronizedObservableCollection<object> _collection;
        private readonly DecoratedCollectionChangeTracker<object> _tracker;
        private ConvertCollectionDecorator<object, object> _decorator;

        public ConvertCollectionDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            _tracker = new DecoratedCollectionChangeTracker<object>();
            _collection.AddComponent(_tracker);
            _decorator = new ConvertCollectionDecorator<object, object>((o, c) => "Item: " + o);
            _collection.AddComponent(_decorator);
            _tracker.Changed += Assert;
        }

        [Fact]
        public void ChangeShouldCleanupReuseItems()
        {
            var ignoreItems = new HashSet<string>();
            var values = new Dictionary<string, object>();
            var newValues = new Dictionary<string, object>();
            var item1 = NewId();
            var item2 = NewId();
            var item3 = NewId();
            ignoreItems.Add(item3);

            var cleanupItems = new List<string>();
            var collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            var collectionTracker = new DecoratedCollectionChangeTracker<object>();

            IEnumerable<object?> GetItems()
            {
                foreach (var o in collection)
                {
                    if (o is string s && !ignoreItems.Contains(s))
                    {
                        if (newValues.TryGetValue(s, out var v))
                            yield return v;
                        else
                            yield return values[s];
                    }
                    else
                        yield return o;
                }
            }

            Action assert = () =>
            {
                collectionTracker.ChangedItems.ShouldEqual(collection.DecoratedItems());
                collectionTracker.ChangedItems.ShouldEqual(GetItems());
            };
            collectionTracker.Changed += assert;
            collection.AddComponent(collectionTracker);

            collection.AddComponent(new ConvertCollectionDecorator<string, object>((s, reuseItem) =>
            {
                if (ignoreItems.Contains(s))
                    return s;

                if (values.TryGetValue(s, out var item))
                    reuseItem.ShouldEqual(item);
                else
                {
                    if (!ReferenceEquals(s, reuseItem))
                        reuseItem.ShouldBeNull();
                    item = new object();
                    values[s] = item;
                }

                if (newValues.TryGetValue(s, out var v))
                    return v;

                return item;
            }, (s, o) =>
            {
                if (!ReferenceEquals(s, o))
                    values[s].ShouldEqual(o);
                cleanupItems.Add(s);
            }));
            collection.Add(item1);
            collection.Add(1);
            collection.Add(item2);
            collection.Add(2);
            collection.Add(item3);

            collection.RaiseItemChanged(1);
            collectionTracker.ItemChangedCount.ShouldEqual(1);
            cleanupItems.ShouldBeEmpty();
            assert();

            collection.RaiseItemChanged(2);
            collectionTracker.ItemChangedCount.ShouldEqual(2);
            cleanupItems.ShouldBeEmpty();
            assert();

            collection.RaiseItemChanged(item2);
            collectionTracker.ItemChangedCount.ShouldEqual(3);
            cleanupItems.ShouldBeEmpty();
            assert();

            collectionTracker.ItemChangedCount = 0;
            newValues[item1] = new object();
            collection.RaiseItemChanged(item1);
            collectionTracker.ItemChangedCount.ShouldEqual(0);
            cleanupItems.Count.ShouldEqual(1);
            cleanupItems.ShouldContain(item1);
            assert();

            cleanupItems.Clear();
            collectionTracker.ItemChangedCount = 0;
            ignoreItems.Remove(item3);
            collection.RaiseItemChanged(item3);
            collectionTracker.ItemChangedCount.ShouldEqual(0);
            cleanupItems.Count.ShouldEqual(1);
            cleanupItems.ShouldContain(item3);
            assert();
        }

        [Fact]
        public void ChangeShouldTrackChanges()
        {
            for (var i = 0; i < 100; i++)
            {
                _collection.Add(i);
                Assert();
            }

            for (var i = 0; i < _collection.Count; i++)
            {
                _collection.RaiseItemChanged(_collection[i]);
                Assert();
                _tracker.ItemChangedCount.ShouldEqual(i + 1);
            }
        }

        [Fact]
        public void DetachShouldCleanupItems()
        {
            var dictionary = new Dictionary<string, object>();
            var item1 = NewId();
            var item2 = NewId();
            var item3 = NewId();
            var cleanupItems = new List<string>();

            var collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            var collectionTracker = new DecoratedCollectionChangeTracker<object>();
            collectionTracker.Changed += () => collectionTracker.ChangedItems.ShouldEqual(collection.DecoratedItems());
            collection.AddComponent(collectionTracker);
            collection.AddComponent(new ConvertCollectionDecorator<string, object>((s, reuseItem) =>
            {
                if (dictionary.TryGetValue(s, out var item))
                    reuseItem.ShouldEqual(item);
                else
                {
                    reuseItem.ShouldBeNull();
                    item = new object();
                    dictionary[s] = item;
                }

                return item;
            }, (s, o) =>
            {
                dictionary[s].ShouldEqual(o);
                cleanupItems.Add(s);
            }));
            collection.Add(item1);
            collection.Add(item2);
            collection.Add(item3);

            collection.RemoveComponents<ConvertCollectionDecorator<string, object>>();
            cleanupItems.Count.ShouldEqual(3);
            cleanupItems.ShouldContain(item3);
            cleanupItems.ShouldContain(item2);
            cleanupItems.ShouldContain(item1);
        }

        [Fact]
        public void IndexOfShouldBeValid()
        {
            _collection.RemoveComponent(_decorator);
            _decorator = new ConvertCollectionDecorator<object, object>((o, c) =>
            {
                if (o is int)
                    return "Item: " + o;
                return o;
            });
            ICollectionDecorator decorator = new ConvertImmutableCollectionDecorator<int, string>(o => "Item: " + o);
            _collection.AddComponent(decorator);
            _collection.Add("Test1");
            _collection.Add(1);
            _collection.Add(2);
            _collection.Add(2);
            _collection.Add("Test2");

            var indexes = new ItemOrListEditor<int>();

            indexes.Clear();
            decorator.TryGetIndexes(_collection, _collection, _collection[0], false, ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(0);

            indexes.Clear();
            decorator.TryGetIndexes(_collection, _collection, "Item: 1", false, ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(1);
            indexes[0].ShouldEqual(1);

            indexes.Clear();
            decorator.TryGetIndexes(_collection, _collection, "Item: 2", false, ref indexes).ShouldBeTrue();
            indexes.Count.ShouldEqual(2);
            indexes[0].ShouldEqual(2);
            indexes[1].ShouldEqual(3);
        }

        [Fact]
        public void RemoveShouldCleanupItems()
        {
            var dictionary = new Dictionary<string, object>();
            var item1 = NewId();
            var item2 = NewId();
            var item3 = NewId();
            var cleanupItems = new List<string>();

            var collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            var collectionTracker = new DecoratedCollectionChangeTracker<object>();
            collectionTracker.Changed += () => collectionTracker.ChangedItems.ShouldEqual(collection.DecoratedItems());
            collection.AddComponent(collectionTracker);
            collection.AddComponent(new ConvertCollectionDecorator<string, object>((s, reuseItem) =>
            {
                if (dictionary.TryGetValue(s, out var item))
                    reuseItem.ShouldEqual(item);
                else
                {
                    reuseItem.ShouldBeNull();
                    item = new object();
                    dictionary[s] = item;
                }

                return item;
            }, (s, o) =>
            {
                dictionary[s].ShouldEqual(o);
                cleanupItems.Add(s);
            }));
            collection.Add(item1);
            collection.Add(item2);
            collection.Add(item3);

            collection.Remove(item3);
            cleanupItems.Count.ShouldEqual(1);
            cleanupItems.ShouldContain(item3);

            collection.RemoveAt(1);
            cleanupItems.Count.ShouldEqual(2);
            cleanupItems.ShouldContain(item3);
            cleanupItems.ShouldContain(item2);

            collection.RemoveAt(0);
            cleanupItems.Count.ShouldEqual(3);
            cleanupItems.ShouldContain(item3);
            cleanupItems.ShouldContain(item2);
            cleanupItems.ShouldContain(item1);
        }

        [Fact]
        public void ReplaceShouldCleanupItems()
        {
            var dictionary = new Dictionary<string, object>();
            var item1 = NewId();
            var item2 = NewId();
            var item3 = NewId();
            var cleanupItems = new List<string>();

            var collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            var collectionTracker = new DecoratedCollectionChangeTracker<object>();
            collectionTracker.Changed += () => collectionTracker.ChangedItems.ShouldEqual(collection.DecoratedItems());
            collection.AddComponent(collectionTracker);
            collection.AddComponent(new ConvertCollectionDecorator<string, object>((s, reuseItem) =>
            {
                if (dictionary.TryGetValue(s, out var item))
                    reuseItem.ShouldEqual(item);
                else
                {
                    reuseItem.ShouldBeNull();
                    item = new object();
                    dictionary[s] = item;
                }

                return item;
            }, (s, o) =>
            {
                dictionary[s].ShouldEqual(o);
                cleanupItems.Add(s);
            }));
            collection.Add(item1);
            collection.Add(item2);

            collection[0] = item3;
            cleanupItems.Count.ShouldEqual(1);
            cleanupItems.ShouldContain(item1);
        }

        [Fact]
        public void ResetShouldReuseCleanupItems()
        {
            var dictionary = new Dictionary<string, object>();
            var item1 = NewId();
            var item2 = NewId();
            var item3 = NewId();
            var cleanupItems = new List<string>();

            var collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            var collectionTracker = new DecoratedCollectionChangeTracker<object>();
            collectionTracker.Changed += () => collectionTracker.ChangedItems.ShouldEqual(collection.DecoratedItems());
            collection.AddComponent(collectionTracker);
            collection.AddComponent(new ConvertCollectionDecorator<string, object>((s, reuseItem) =>
            {
                if (dictionary.TryGetValue(s, out var item))
                    reuseItem.ShouldEqual(item);
                else
                {
                    reuseItem.ShouldBeNull();
                    item = new object();
                    dictionary[s] = item;
                }

                return item;
            }, (s, o) =>
            {
                dictionary[s].ShouldEqual(o);
                cleanupItems.Add(s);
            }));
            collection.Add(item1);
            collection.Add(item2);
            collection.Add(item3);

            collection.Reset(new[] { item1, item2 });
            cleanupItems.Count.ShouldEqual(1);
            cleanupItems.ShouldContain(item3);
        }

        protected override IObservableCollection<object> GetCollection() => _collection;

        protected override void Assert()
        {
            _tracker.ChangedItems.ShouldEqual(_collection.DecoratedItems());
            _tracker.ChangedItems.ShouldEqual(Decorate().ToArray());
        }

        private IEnumerable<object?> Decorate() => _collection.Select(o => _decorator.Converter(o, null));
    }
}