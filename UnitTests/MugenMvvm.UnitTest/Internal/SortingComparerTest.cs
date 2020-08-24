using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Internal
{
    public class SortingComparerTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(1000)]
        [InlineData(100)]
        public void AscendingShouldBeCorrect(int count)
        {
            var random = new Random();
            var ints = new List<int>();
            for (var i = 0; i < count; i++)
                ints.Add(random.Next());

            var comparer = SortingComparer<int>
                .Ascending(i => i)
                .ThenByDescending(i => i % 2 == 0)
                .ThenBy(i => i % 3 == 0)
                .Build();
            var list = ints.ToList();
            list.Sort(comparer);

            list.SequenceEqual(ints.OrderBy(i => i).ThenByDescending(i => i % 2 == 0).ThenBy(i => i % 3 == 0)).ShouldBeTrue();
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(100)]
        public void DescendingShouldBeCorrect(int count)
        {
            var random = new Random();
            var ints = new List<int>();
            for (var i = 0; i < count; i++)
                ints.Add(random.Next());

            var comparer = SortingComparer<int>
                .Descending(i => i)
                .ThenByDescending(i => i % 2 == 0)
                .ThenBy(i => i % 3 == 0)
                .Build();
            var list = ints.ToList();
            list.Sort(comparer);

            list.SequenceEqual(ints.OrderByDescending(i => i).ThenByDescending(i => i % 2 == 0).ThenBy(i => i % 3 == 0)).ShouldBeTrue();
        }

        #endregion
    }
}