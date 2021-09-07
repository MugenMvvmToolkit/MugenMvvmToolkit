using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Internal;
using Should;
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

        [Theory]
        [InlineData(1000)]
        [InlineData(100)]
        public void PinHeaderFooterShouldBeCorrect1(int count)
        {
            var ints = new List<int>();
            for (var i = 0; i < count; i++)
                ints.Insert(0, i);

            Func<int, bool?> condition = i =>
            {
                if (i >= 90)
                    return true;
                if (i < 10)
                    return false;
                return null;
            };
            var comparer = SortingComparer<int>.PinHeaderFooter(condition).ThenBy(i => i).Build();

            var list = ints.ToList();
            list.Sort(comparer);

            var headers = ints.Where(i => condition(i).GetValueOrDefault()).OrderBy(i => i).ToList();
            var footers = ints.Where(i => !condition(i).GetValueOrDefault(true)).OrderBy(i => i).ToList();
            ints.RemoveAll(i => condition(i).HasValue);

            headers.ShouldEqual(list.Where(i => condition(i).GetValueOrDefault()));
            footers.ShouldEqual(list.Where(i => !condition(i).GetValueOrDefault(true)));
            list.Where(i => !condition(i).HasValue).ShouldNotEqual(ints.OrderBy(i => i));
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(100)]
        public void PinHeaderFooterShouldBeCorrect2(int count)
        {
            Func<int, bool?> condition = i =>
            {
                if (i >= 90)
                    return true;
                if (i < 10)
                    return false;
                return null;
            };

            var items = new List<object>();
            for (var i = 0; i < count; i++)
            {
                if (condition(i).HasValue)
                    items.Insert(0, i);
                else
                    items.Insert(0, i.ToString());
            }

            var comparer = SortingComparer<object>.PinHeaderFooter(o =>
            {
                if (o is int i)
                    return condition(i);
                return null;
            }).ThenBy(i => i.GetHashCode()).Build().AsObjectComparer();

            var list = items.ToList();
            list.Sort(comparer);

            var headers = items.OfType<int>().Where(i => condition(i).GetValueOrDefault()).OrderBy(i => i).OfType<object>().ToList();
            var footers = items.OfType<int>().Where(i => !condition(i).GetValueOrDefault(true)).OrderBy(i => i).OfType<object>().ToList();
            items.RemoveAll(o => o is int i && condition(i).HasValue);

            headers.ShouldEqual(list.OfType<int>().Where(i => condition(i).GetValueOrDefault()).OfType<object>());
            footers.ShouldEqual(list.OfType<int>().Where(i => !condition(i).GetValueOrDefault(true)).OfType<object>());
        }
    }
}