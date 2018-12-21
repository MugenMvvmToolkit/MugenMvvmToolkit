using System.Collections.Generic;
using Should;

namespace MugenMvvm.UnitTest
{
    public static class UnitTestExtensions
    {
        #region Methods

        public static void ShouldContain<T>(this IEnumerable<T> enumerable, params T[] items)
        {
            foreach (var item in items)
                CollectionAssertExtensions.ShouldContain(enumerable, item);
        }

        #endregion
    }
}