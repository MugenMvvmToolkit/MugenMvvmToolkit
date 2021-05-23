using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.UnitTests.Collections.Internal;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections
{
    public class DiffableBindableCollectionAdapterTest : BindableCollectionAdapterTest
    {
        private readonly ITestOutputHelper _outputHelper;

        public DiffableBindableCollectionAdapterTest(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void ShouldUseDiffableComparer()
        {
            var comparer = new TestDiffableEqualityComparer
            {
                AreItemsTheSame = (x1, x2) => true
            };

            var items = new object[] {1, 2, 3, 4};
            var resetItems = new object[] {4, 3, 2, 1};

            var observableCollection = new SynchronizedObservableCollection<object?>(items, ComponentCollectionManager);
            var adapterCollection = new ObservableCollection<object?>();
            var collectionAdapter = (DiffableBindableCollectionAdapter) GetCollection(LocalThreadDispatcher, adapterCollection);
            collectionAdapter.DiffableComparer = comparer;
            var tracker = new ObservableCollectionTracker<object?>();
            adapterCollection.CollectionChanged += tracker.OnCollectionChanged;
            collectionAdapter.Collection = observableCollection;

            observableCollection.Reset(resetItems);
            tracker.ChangedItems.ShouldEqual(items);
            collectionAdapter.ShouldEqual(items);
        }

        [Theory]
        [InlineData(200, true)]
        [InlineData(200, false)]
        public void ShouldUseCorrectIndexes(int iterationCount, bool detectMoves)
        {
            var observableCollection = new SynchronizedObservableCollection<object?>(ComponentCollectionManager);
            var adapterCollection = new ObservableCollection<object?>();
            var collectionAdapter = (DiffableBindableCollectionAdapter) GetCollection(LocalThreadDispatcher, adapterCollection);
            collectionAdapter.DetectMoves = detectMoves;
            var tracker = new ObservableCollectionTracker<object?>();
            adapterCollection.CollectionChanged += tracker.OnCollectionChanged;
            collectionAdapter.Collection = observableCollection;
            var random = new Random(Guid.NewGuid().GetHashCode());

            for (var _ = 0; _ < iterationCount; _++)
            {
                var items = new List<string>();
                var count = observableCollection.Count == 0 ? 5 : observableCollection.Count == 20 ? 5 : 20;
                for (var i = 0; i < count; i++)
                    items.Add(i.ToString());

                Shuffle(items, random);
#if DEBUG
                _outputHelper.WriteLine($"before {string.Join(",", observableCollection.Select(model => $"\"{model}\""))}");
                _outputHelper.WriteLine($"after {string.Join(",", items.Select(model => $"\"{model}\""))}");
#endif

                observableCollection.Reset(items);

                tracker.ChangedItems.ShouldEqual(observableCollection);
                collectionAdapter.ShouldEqual(observableCollection);
            }
        }

        private static void Shuffle<T>(List<T> list, Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        protected override BindableCollectionAdapter GetCollection(IThreadDispatcher threadDispatcher, IList<object?>? source = null) =>
            new DiffableBindableCollectionAdapter(source, threadDispatcher);
    }
}