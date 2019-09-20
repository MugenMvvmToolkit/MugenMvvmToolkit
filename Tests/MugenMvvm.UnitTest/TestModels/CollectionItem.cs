using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MugenMvvm.UnitTest.TestModels
{
    [DebuggerDisplay("Id = {Id}")]
    public class CollectionItem
    {
        #region Fields

        private static int _idGenerator = -1;

        #endregion

        #region Constructors

        public CollectionItem()
        {
            Id = Interlocked.Increment(ref _idGenerator);
        }

        #endregion

        #region Properties

        public static IEqualityComparer<CollectionItem> IdComparer { get; } = new IdEqualityComparer();

        public bool Hidden { get; set; }

        public string? Name { get; set; }

        public int Id { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Id: {Id}";
        }

        #endregion

        #region Nested types

        private sealed class IdEqualityComparer : IEqualityComparer<CollectionItem>
        {
            #region Implementation of interfaces

            public bool Equals(CollectionItem x, CollectionItem y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(CollectionItem obj)
            {
                return obj.Id;
            }

            #endregion
        }

        #endregion
    }
}