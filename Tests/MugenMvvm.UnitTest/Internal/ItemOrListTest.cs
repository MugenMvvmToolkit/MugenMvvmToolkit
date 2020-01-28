using System.Collections.Generic;
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
        public void ShouldHandleSingleItemInList()
        {
            var itemOrList = new ItemOrList<object, object[]>(new[] { this });
            itemOrList.Item.ShouldEqual(this);
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleEmptyList()
        {
            var itemOrList = new ItemOrList<object, object[]>(new object[0]);
            itemOrList.Item.ShouldBeNull();
            itemOrList.List.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleList()
        {
            var list = new object[] { this, this };
            var itemOrList = new ItemOrList<object, object[]>(list);
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