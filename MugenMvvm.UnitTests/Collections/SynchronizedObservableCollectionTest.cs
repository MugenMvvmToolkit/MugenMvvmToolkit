using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Collections
{
    public class SynchronizedObservableCollectionTest : ObservableCollectionTestBase
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void ValueEnumeratorTest(int size)
        {
            var items = new List<TestCollectionItem>();
            var collection = new SynchronizedObservableCollection<TestCollectionItem>(items, ComponentCollectionManager);
            for (var i = 0; i < size; i++)
            {
                items.Add(new TestCollectionItem());
                collection.Add(items[i]);
            }

            var index = 0;
            foreach (var item in collection)
                collection[index++].ShouldEqual(item);
        }

        protected override IObservableCollection<T> CreateCollection<T>(params T[] items) => new SynchronizedObservableCollection<T>(items, ComponentCollectionManager);
    }
}