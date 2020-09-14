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
        public void FromListRawShouldHandleList()
        {
            var list = new[] {this};
            var itemOrList = ItemOrList.FromListRaw<object, object[]>(list);
            itemOrList.List.ShouldEqual(list);
            itemOrList.Item.ShouldBeNull();
        }

        [Fact]
        public void FromItemShouldHandleSingleItem1()
        {
            var itemOrList = ItemOrList.FromItem(this);
            itemOrList.List.ShouldBeNull();
            itemOrList.Item.ShouldEqual(this);
        }

        [Fact]
        public void FromItemShouldHandleSingleItem2()
        {
            var itemOrList = ItemOrList.FromItem<ItemOrListTest, ItemOrListTest[]>(this);
            itemOrList.List.ShouldBeNull();
            itemOrList.Item.ShouldEqual(this);
        }

        [Fact]
        public void FromListToReadOnlyShouldHandleArray1() => ItemOrList.FromListToReadOnly<string>(array: null).IsNullOrEmpty().ShouldBeTrue();

        [Fact]
        public void FromListToReadOnlyShouldHandleArray2()
        {
            var array = new[] {this};
            var itemOrList = ItemOrList.FromListToReadOnly(array);
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void FromListToReadOnlyShouldHandleArray3()
        {
            var array = new[] {this, this};
            var itemOrList = ItemOrList.FromListToReadOnly(array);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(array);
        }

        [Fact]
        public void FromListToReadOnlyShouldHandleList1() => ItemOrList.FromListToReadOnly<string>(list: null).IsNullOrEmpty().ShouldBeTrue();

        [Fact]
        public void FromListToReadOnlyShouldHandleList2()
        {
            var list = new[] {this}.ToList();
            var itemOrList = ItemOrList.FromListToReadOnly(list);
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void FromListToReadOnlyShouldHandleList3()
        {
            var list = new[] {this, this}.ToList();
            var itemOrList = ItemOrList.FromListToReadOnly(list);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        [Fact]
        public void FromListShouldHandleArray1() => ItemOrList.FromList<string>(array: null).IsNullOrEmpty().ShouldBeTrue();

        [Fact]
        public void FromListShouldHandleArray2()
        {
            var array = new[] {this};
            var itemOrList = ItemOrList.FromList(array);
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void FromListShouldHandleArray3()
        {
            var array = new[] {this, this};
            var itemOrList = ItemOrList.FromList(array);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(array);
        }

        [Fact]
        public void FromListShouldHandleReadOnlyList1() => ItemOrList.FromList<string>(readOnlyList: null).IsNullOrEmpty().ShouldBeTrue();

        [Fact]
        public void FromListShouldHandleReadOnlyList2()
        {
            IReadOnlyList<ItemOrListTest> list = new[] {this};
            var itemOrList = ItemOrList.FromList(list);
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void FromListShouldHandleReadOnlyList3()
        {
            IReadOnlyList<ItemOrListTest> list = new[] {this, this};
            var itemOrList = ItemOrList.FromList(list);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        [Fact]
        public void FromListShouldHandleIList1() => ItemOrList.FromList<string>(iList: null).IsNullOrEmpty().ShouldBeTrue();

        [Fact]
        public void FromListShouldHandleIList2()
        {
            IList<ItemOrListTest> list = new[] {this};
            var itemOrList = ItemOrList.FromList(list);
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void FromListShouldHandleIList3()
        {
            IList<ItemOrListTest> list = new[] {this, this};
            var itemOrList = ItemOrList.FromList(list);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        [Fact]
        public void FromListShouldHandleList1() => ItemOrList.FromList<string>(list: null).IsNullOrEmpty().ShouldBeTrue();

        [Fact]
        public void FromListShouldHandleList2()
        {
            var list = new[] {this}.ToList();
            var itemOrList = ItemOrList.FromList(list);
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void FromListShouldHandleList3()
        {
            var list = new[] {this, this}.ToList();
            var itemOrList = ItemOrList.FromList(list);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        [Fact]
        public void FromListShouldHandleNullCollection() => ItemOrList.FromList<string, IEnumerable<string>>(null).IsNullOrEmpty().ShouldBeTrue();

        [Fact]
        public void FromListShouldHandleReadOnlyListCollection1()
        {
            IReadOnlyList<ItemOrListTest> list = new[] {this};
            var itemOrList = ItemOrList.FromList<object, IEnumerable<object>>(list);
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void FromListShouldHandleReadOnlyListCollection2()
        {
            IReadOnlyList<ItemOrListTest> list = new[] {this, this};
            var itemOrList = ItemOrList.FromList<object, IEnumerable<object>>(list);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        [Fact]
        public void FromListShouldHandleArrayCollection1()
        {
            var list = new[] {this};
            var itemOrList = ItemOrList.FromList<object, IEnumerable<object>>(list);
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void FromListShouldHandleArrayCollection2()
        {
            var list = new[] {this, this};
            var itemOrList = ItemOrList.FromList<object, IEnumerable<object>>(list);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        [Fact]
        public void FromListShouldHandleEnumerableCollection1()
        {
            var list = new[] {this}.Where(test => test != null);
            var itemOrList = ItemOrList.FromList<object, IEnumerable<object>>(list);
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void FromListShouldHandleEnumerableCollection2()
        {
            var list = new[] {this, this}.Where(test => test != null);
            var itemOrList = ItemOrList.FromList<object, IEnumerable<object>>(list);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FromRawValueShouldHandleSingleItem(bool @unchecked)
        {
            var itemOrList = ItemOrList.FromRawValue<object, object[]>(this, @unchecked);
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FromRawValueShouldHandleSingleItemInList(bool @unchecked)
        {
            var list = new[] {this};
            var itemOrList = ItemOrList.FromRawValue<object, object[]>(list, @unchecked);
            if (@unchecked)
            {
                itemOrList.List.ShouldEqual(list);
                itemOrList.Item.ShouldBeNull();
            }
            else
            {
                itemOrList.Item.ShouldEqual(this);
                itemOrList.List.ShouldBeNull();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FromRawValueShouldHandleList(bool @unchecked)
        {
            var list = new object[] {this, this};
            var itemOrList = ItemOrList.FromRawValue<object, object[]>(list, @unchecked);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        [Fact]
        public void ShouldHandleNull()
        {
            var itemOrList = new ItemOrList<object, object[]>(null);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleSingleItem()
        {
            var itemOrList = new ItemOrList<object, object[]>(this);
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleList1()
        {
            var list = new object[] {this};
            var itemOrList = new ItemOrList<object, object[]>(list);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        [Fact]
        public void ShouldHandleList2()
        {
            var list = new object[] {this, this};
            var itemOrList = new ItemOrList<object, object[]>(list);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        [Fact]
        public void CastShouldHandleList()
        {
            var list = new object[] {this, this};
            var itemOrList = ItemOrList.FromRawValue<object, object[]>(list);
            var cast = itemOrList.Cast<IReadOnlyList<object>>();
            cast.Item.ShouldBeNull();
            cast.List.ShouldEqual(list);
        }

        [Fact]
        public void ImplicitCastShouldHandleSingleItem()
        {
            ItemOrList<ItemOrListTest, ItemOrListTest[]> itemOrList = this;
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void ImplicitCastShouldHandleList1()
        {
            var list = new object[] {this, this};
            ItemOrList<object, object[]> itemOrList = list;
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        [Fact]
        public void ImplicitCastShouldHandleList2()
        {
            var list = new object[] {this};
            ItemOrList<object, object[]> itemOrList = list;
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void DeconstructShouldReturnItemOrList()
        {
            new ItemOrList<object, IEnumerable<object>>(this).Deconstruct(out var item, out var list);
            item.ShouldEqual(this);
            list.ShouldBeNull();

            var array = new object[] {this, this};
            new ItemOrList<object, IEnumerable<object>>(array).Deconstruct(out item, out list);
            item.ShouldBeNull();
            list.ShouldEqual(array);
        }

        #endregion
    }
}