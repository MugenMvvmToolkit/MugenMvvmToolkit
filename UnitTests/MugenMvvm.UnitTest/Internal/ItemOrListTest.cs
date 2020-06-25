using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Internal
{
    public class ItemOrListTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldHandleNull()
        {
            var itemOrList = new ItemOrList<object, object[]>((object[]?)null);
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
        public void ShouldHandleSingleItemInList1()
        {
            var itemOrList = new ItemOrList<object, object[]>(new object[] { this });
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleSingleItemInList2()
        {
            var itemOrList = new ItemOrList<object, List<object>>(new List<object> { this });
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleSingleItemInList3()
        {
            var itemOrList = new ItemOrList<object, HashSet<object>>(new HashSet<object> { this });
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleSingleItemInList4()
        {
            var itemOrList = new ItemOrList<object, IEnumerable<object>>(new object[] { this }.Where(o => o != null));
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleEmptyList1()
        {
            var itemOrList = new ItemOrList<object, object[]>(new object[0]);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleEmptyList2()
        {
            var itemOrList = new ItemOrList<object, List<object>>(new List<object>());
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleEmptyList3()
        {
            var itemOrList = new ItemOrList<object, HashSet<object>>(new HashSet<object>());
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleEmptyList4()
        {
            var itemOrList = new ItemOrList<object, IEnumerable<object>>(new object[0].Where(o => o != null));
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleList1()
        {
            var list = new object[] { this, this };
            var itemOrList = new ItemOrList<object, object[]>(list);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        [Fact]
        public void ShouldHandleList2()
        {
            var list = new List<object> { this, this };
            var itemOrList = new ItemOrList<object, List<object>>(list);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        [Fact]
        public void ShouldHandleList3()
        {
            var list = new HashSet<object> { this, "" };
            var itemOrList = new ItemOrList<object, HashSet<object>>(list);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        [Fact]
        public void ShouldHandleList4()
        {
            var list = new object[] { this, this }.Where(o => o != null);
            var itemOrList = new ItemOrList<object, IEnumerable<object>>(list);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        [Fact]
        public void FromRawValueShouldHandleSingleItem()
        {
            var itemOrList = ItemOrList<object, object[]>.FromRawValue(this);
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void FromRawValueShouldHandleSingleItemInList()
        {
            var itemOrList = ItemOrList<object, object[]>.FromRawValue(new[] { this });
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void FromRawValueShouldHandleList()
        {
            var list = new object[] { this, this };
            var itemOrList = ItemOrList<object, object[]>.FromRawValue(list);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        [Fact]
        public void CastShouldHandleList()
        {
            var list = new object[] { this, this };
            var itemOrList = ItemOrList<object, object[]>.FromRawValue(list);
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
        public void ImplicitCastShouldHandleList()
        {
            var list = new object[] { this, this };
            ItemOrList<object, object[]> itemOrList = list;
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldEqual(list);
        }

        #endregion
    }
}