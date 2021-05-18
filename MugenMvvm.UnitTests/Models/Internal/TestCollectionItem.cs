using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MugenMvvm.UnitTests.Models.Internal
{
    [DebuggerDisplay("Id = {Id}")]
    public class TestCollectionItem
    {
        private static int _idGenerator = -1;

        public TestCollectionItem()
        {
            Id = Interlocked.Increment(ref _idGenerator);
            StableId = Id;
            if (StableId % 10 == 0)
                Items = new object[] {Guid.NewGuid(), Guid.NewGuid()};
        }

        public static IEqualityComparer<TestCollectionItem> IdComparer { get; } = new IdEqualityComparer();

        public bool Hidden { get; set; }

        public string? Name { get; set; }

        public int Id { get; set; }

        public int StableId { get; }

        public IEnumerable<object>? Items { get; }

        public override string ToString() => $"Id: {Id}";

        private sealed class IdEqualityComparer : IEqualityComparer<TestCollectionItem>
        {
            public bool Equals(TestCollectionItem? x, TestCollectionItem? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(TestCollectionItem obj) => obj!.Id;
        }
    }
}