using System.Collections.Generic;
using Should;

namespace MugenMvvm.UnitTest
{
    public static class UnitTestExtensions
    {
        #region Methods

        public static void ShouldContain<T>(this IEnumerable<T> enumerable, IEnumerable<T> itemsEnumerable)
        {
            foreach (var item in itemsEnumerable)
                CollectionAssertExtensions.ShouldContain(enumerable, item);
        }

        public static void ShouldContain<T>(this IEnumerable<T> enumerable, params T[] items)
        {
            ShouldContain(enumerable, itemsEnumerable: items);
        }

        public static void ShouldBeNull(this object @object, string msg)
        {
            @object.ShouldBeNull();
        }

        #endregion
    }
}