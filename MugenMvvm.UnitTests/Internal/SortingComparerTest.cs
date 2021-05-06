using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Internal;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class SortingComparerTest : UnitTestBase
    {
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

            list.ShouldEqual(ints.OrderBy(i => i).ThenByDescending(i => i % 2 == 0).ThenBy(i => i % 3 == 0));
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(100)]
        public void CompareShouldBeCorrect1(int count)
        {
            var random = new Random();
            var ints = new List<int>();
            for (var i = 0; i < count; i++)
                ints.Add(random.Next());

            var comparer = SortingComparer<int>
                           .Compare((x1, x2) => x1.CompareTo(x2))
                           .ThenByDescending(i => i % 2 == 0)
                           .ThenBy(i => i % 3 == 0)
                           .Build();
            var list = ints.ToList();
            list.Sort(comparer);

            list.ShouldEqual(ints.OrderBy(i => i).ThenByDescending(i => i % 2 == 0).ThenBy(i => i % 3 == 0));
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(100)]
        public void CompareShouldBeCorrect2(int count)
        {
            var random = new Random();
            var ints = new List<int>();
            for (var i = 0; i < count; i++)
                ints.Add(random.Next());

            var comparer = SortingComparer<int>
                           .Compare((x1, x2) => x2.CompareTo(x1))
                           .ThenByDescending(i => i % 2 == 0)
                           .ThenBy(i => i % 3 == 0)
                           .Build();
            var list = ints.ToList();
            list.Sort(comparer);

            list.ShouldEqual(ints.OrderByDescending(i => i).ThenByDescending(i => i % 2 == 0).ThenBy(i => i % 3 == 0));
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

            list.ShouldEqual(ints.OrderByDescending(i => i).ThenByDescending(i => i % 2 == 0).ThenBy(i => i % 3 == 0));
        }
    }
}