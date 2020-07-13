﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace MugenMvvm.UnitTest.Models
{
    [DebuggerDisplay("Id = {Id}")]
    public class TestCollectionItem
    {
        #region Fields

        private static int _idGenerator = -1;

        #endregion

        #region Constructors

        public TestCollectionItem()
        {
            Id = Interlocked.Increment(ref _idGenerator);
        }

        #endregion

        #region Properties

        public static IEqualityComparer<TestCollectionItem> IdComparer { get; } = new IdEqualityComparer();

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

        private sealed class IdEqualityComparer : IEqualityComparer<TestCollectionItem>
        {
            #region Implementation of interfaces

            public bool Equals([AllowNull] TestCollectionItem x, [AllowNull] TestCollectionItem y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id;
            }

            public int GetHashCode([AllowNull]TestCollectionItem obj)
            {
                return obj!.Id;
            }

            #endregion
        }

        #endregion
    }
}