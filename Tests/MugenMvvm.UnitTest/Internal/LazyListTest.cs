using System.Collections.Generic;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Internal
{
    public class LazyListTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void CountShouldReturnZeroEmpty()
        {
            LazyList<object> list = default;
            list.Count.ShouldEqual(0);
        }

        [Fact]
        public void CountShouldReturnListCount()
        {
            LazyList<object> list = default;
            var objects = list.Get();
            objects.Add(this);
            objects.Add(this);
            list.Count.ShouldEqual(objects.Count);
        }

        [Fact]
        public void ImplicitCastShouldReturnList()
        {
            LazyList<object> list = default;
            List<object>? l = list;
            l.ShouldBeNull();

            var objects = list.Get();
            l = list;
            l.ShouldEqual(objects);
        }

        [Fact]
        public void GetShouldCreateList()
        {
            LazyList<object> list = default;
            list.List.ShouldEqual(null);

            var objects = list.Get();
            objects.ShouldNotBeNull();
            list.List.ShouldEqual(objects);
        }

        #endregion
    }
}