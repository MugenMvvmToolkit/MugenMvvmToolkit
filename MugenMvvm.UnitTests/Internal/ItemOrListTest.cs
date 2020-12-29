using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class ItemOrListTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void FromItemShouldHandleSingleItem1() => AssertItem(ItemOrList.FromItem(this), this);

        [Fact]
        public void FromItemShouldHandleSingleItem2() => AssertItem(ItemOrList.FromItem<ItemOrListTest, ItemOrListTest[]>(this), this);

        [Fact]
        public void FromListShouldHandleReadOnlyList1() => AssertEmpty(ItemOrList.FromList<string>(readOnlyList: null));

        [Fact]
        public void FromListShouldHandleReadOnlyList2()
        {
            IReadOnlyList<ItemOrListTest> list = new[] {this};
            AssertItem(ItemOrList.FromList(list), list[0]);
        }

        [Fact]
        public void FromListShouldHandleReadOnlyList3()
        {
            IReadOnlyList<ItemOrListTest> list = new[] {this, this};
            AssertList(ItemOrList.FromList(list), list);
        }

        [Fact]
        public void FromListShouldHandleIList1() => AssertEmpty(ItemOrList.FromList<string>(iList: null));

        [Fact]
        public void FromListShouldHandleIList2()
        {
            IList<ItemOrListTest> list = new[] {this};
            AssertItem(ItemOrList.FromList(list), list[0]);
        }

        [Fact]
        public void FromListShouldHandleIList3()
        {
            IList<ItemOrListTest> list = new[] {this, this};
            AssertList(ItemOrList.FromList(list), list);
        }

        [Fact]
        public void FromListShouldHandleNullCollection() => AssertEmpty(ItemOrList.FromList<string, IEnumerable<string>>(null));

        [Fact]
        public void FromListShouldHandleReadOnlyListCollection1()
        {
            IReadOnlyList<ItemOrListTest> list = new[] {this};
            AssertItem(ItemOrList.FromList<object, IEnumerable<object>>(list), list[0]);
        }

        [Fact]
        public void FromListShouldHandleReadOnlyListCollection2()
        {
            IReadOnlyList<ItemOrListTest> list = new[] {this, this};
            AssertList(ItemOrList.FromList<object, IEnumerable<object>>(list), list);
        }

        [Fact]
        public void FromListShouldHandleArrayCollection1()
        {
            var list = new[] {this};
            AssertItem(ItemOrList.FromList<object, IEnumerable<object>>(list), list[0]);
        }

        [Fact]
        public void FromListShouldHandleArrayCollection2()
        {
            var list = new[] {this, this};
            AssertList(ItemOrList.FromList<object, IEnumerable<object>>(list), list);
        }

        [Fact]
        public void FromListShouldHandleEnumerableCollection1()
        {
            var list = new[] {this}.Where(test => test != null);
            AssertItem(ItemOrList.FromList<object, IEnumerable<object>>(list), list.First());
        }

        [Fact]
        public void FromListShouldHandleEnumerableCollection2()
        {
            var list = new[] {this, this}.Where(test => test != null);
            AssertList(ItemOrList.FromList<object, IEnumerable<object>>(list), list);
        }

        [Fact]
        public void FromRawValueShouldHandleSingleItem()
        {
            var itemOrList = ItemOrList.FromRawValue<object, object[]>(this);
            AssertItem(ItemOrList.FromRawValue<object, object[]>(this), this);
        }

        [Fact]
        public void FromRawValueShouldHandleSingleItemInList()
        {
            var list = new[] {this};
            AssertItem(ItemOrList.FromRawValue<object, object[]>(list), list[0]);
        }

        [Fact]
        public void FromRawValueShouldHandleList()
        {
            var list = new object[] {this, this};
            AssertList(ItemOrList.FromRawValue<object, object[]>(list), list);
        }

        [Fact]
        public void ShouldHandleNull()
        {
            AssertEmpty(new ItemOrList<object, object[]>(null));
        }

        [Fact]
        public void ShouldHandleSingleItem()
        {
            AssertItem(new ItemOrList<object, object[]>(this, true), this);
        }

        [Fact]
        public void ShouldHandleList1()
        {
            var list = new object[] {this};
            AssertItem(new ItemOrList<object, object[]>(list), list[0]);
        }

        [Fact]
        public void ShouldHandleList2()
        {
            var list = new object[] {this, this};
            AssertList(new ItemOrList<object, object[]>(list), list);
        }

        [Fact]
        public void CastShouldHandleList()
        {
            var list = new object[] {this, this};
            var itemOrList = ItemOrList.FromRawValue<object, object[]>(list);
            AssertList(itemOrList.Cast<IReadOnlyList<object>>(), list);
        }

        [Fact]
        public void ImplicitCastShouldHandleList1()
        {
            var list = new object[] {this, this};
            ItemOrList<object, object[]> itemOrList = list;
            AssertList(itemOrList, list);
        }

        [Fact]
        public void ImplicitCastShouldHandleList2()
        {
            var list = new object[] {this};
            ItemOrList<object, object[]> itemOrList = list;
            AssertItem(itemOrList, list[0]);
        }

        private static void AssertEmpty<T, TList>(ItemOrList<T, TList> itemOrList) where TList : class, IEnumerable<T>
        {
            itemOrList.Item.ShouldEqual(default!);
            itemOrList.IsEmpty.ShouldBeTrue();
            itemOrList.Count.ShouldEqual(0);
            itemOrList.HasItem.ShouldBeFalse();
            itemOrList.List.ShouldBeNull();
            itemOrList.AsList().ShouldEqual(Default.Array<T>());
            itemOrList.ToArray().ShouldEqual(Default.Array<T>());
            var buffer = new List<T>();
            foreach (var i in itemOrList)
                buffer.Add(i);
            buffer.ShouldEqual(Default.Array<T>());
        }

        private static void AssertItem<T, TList>(ItemOrList<T, TList> itemOrList, T item) where TList : class, IEnumerable<T>
        {
            var list = new[] {item};
            itemOrList.Item.ShouldEqual(item);
            itemOrList.IsEmpty.ShouldBeFalse();
            itemOrList.Count.ShouldEqual(1);
            itemOrList.HasItem.ShouldBeTrue();
            itemOrList.List.ShouldBeNull();
            itemOrList.AsList().ShouldEqual(list);
            itemOrList.ToArray().ShouldEqual(list);
            var buffer = new List<T>();
            foreach (var i in itemOrList)
                buffer.Add(i);
            buffer.ShouldEqual(list);
        }

        private static void AssertList<T, TList>(ItemOrList<T, TList> itemOrList, IEnumerable<T> list) where TList : class, IEnumerable<T>
        {
            itemOrList.Item.ShouldBeNull();
            itemOrList.IsEmpty.ShouldBeFalse();
            itemOrList.Count.ShouldEqual(list.Count());
            itemOrList.HasItem.ShouldBeFalse();
            itemOrList.List.ShouldEqual(list);
            itemOrList.AsList().ShouldEqual(list);
            itemOrList.ToArray().ShouldEqual(list);
            var buffer = new List<T>();
            foreach (var item in itemOrList)
                buffer.Add(item);
            buffer.ShouldEqual(list);
        }

        #endregion
    }
}