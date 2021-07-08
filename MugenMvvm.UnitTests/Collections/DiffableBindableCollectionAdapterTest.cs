using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Tests.Collections;
using MugenMvvm.UnitTests.Collections.Internal;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections
{
    public class DiffableBindableCollectionAdapterTest : BindableCollectionAdapterTest
    {
        public DiffableBindableCollectionAdapterTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Theory]
        [InlineData(200, 5, 20, true)]
        [InlineData(200, 5, 20, false)]
        [InlineData(200, 1, 10, true)]
        [InlineData(200, 1, 10, false)]
        [InlineData(200, 10, 40, true)]
        [InlineData(200, 10, 40, false)]
        public void ShouldUseCorrectIndexes(int iterationCount, int minCount, int maxCount, bool detectMoves)
        {
            var observableCollection = new SynchronizedObservableCollection<object?>(ComponentCollectionManager);
            var adapterCollection = new ObservableCollection<object?>();
            var collectionAdapter = (DiffableBindableCollectionAdapter)GetCollection(ThreadDispatcher, adapterCollection);
            collectionAdapter.DetectMoves = detectMoves;
            var tracker = new ObservableCollectionTracker<object?>();
            adapterCollection.CollectionChanged += tracker.OnCollectionChanged;
            collectionAdapter.Collection = observableCollection;
            var random = new Random(Guid.NewGuid().GetHashCode());

            for (var _ = 0; _ < iterationCount; _++)
            {
                var items = new List<string>();
                var count = observableCollection.Count == 0 ? 5 : observableCollection.Count == maxCount ? minCount : maxCount;
                for (var i = 0; i < count; i++)
                    items.Add(i.ToString());

                Shuffle(items, random);
#if DEBUG
                Logger.Log(LogLevel.Debug, $"before {string.Join(",", observableCollection.Select(model => $"\"{model}\""))}");
                Logger.Log(LogLevel.Debug, $"after {string.Join(",", items.Select(model => $"\"{model}\""))}");
#endif

                observableCollection.Reset(items);

                tracker.ChangedItems.ShouldEqual(observableCollection);
                collectionAdapter.ShouldEqual(observableCollection);
            }
        }

        [Fact]
        public void ShouldUseDiffableComparer()
        {
            var comparer = new TestDiffableEqualityComparer
            {
                AreItemsTheSame = (_, _) => true
            };

            var items = new object[] { 1, 2, 3, 4 };
            var resetItems = new object[] { 4, 3, 2, 1 };

            var observableCollection = new SynchronizedObservableCollection<object?>(items, ComponentCollectionManager);
            var adapterCollection = new ObservableCollection<object?>();
            var collectionAdapter = (DiffableBindableCollectionAdapter)GetCollection(ThreadDispatcher, adapterCollection);
            collectionAdapter.DiffableComparer = comparer;
            var tracker = new ObservableCollectionTracker<object?>();
            adapterCollection.CollectionChanged += tracker.OnCollectionChanged;
            collectionAdapter.Collection = observableCollection;

            observableCollection.Reset(resetItems);
            tracker.ChangedItems.ShouldEqual(items);
            collectionAdapter.ShouldEqual(items);
        }

        protected override bool IsSuspendSupported => true;

        private static void Shuffle<T>(List<T> list, Random rng)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        protected override BindableCollectionAdapter GetCollection(IThreadDispatcher threadDispatcher, IList<object?>? source = null) =>
            new DiffableBindableCollectionAdapter(source, threadDispatcher) { BatchDelay = 0 };
    }
}