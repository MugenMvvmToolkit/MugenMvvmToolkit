using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Collections
{
    public class ItemOrIReadOnlyListTest : UnitTestBase
    {
        [Fact]
        public void CastShouldBeValid1()
        {
            var objValue = new ItemOrIReadOnlyList<object>(this, false);
            var thisValue = objValue.Cast<ItemOrIReadOnlyListTest>();
            AssertEmpty(thisValue);
        }

        [Fact]
        public void CastShouldBeValid2()
        {
            var objValue = new ItemOrIReadOnlyList<object>(this, true);
            var thisValue = objValue.Cast<ItemOrIReadOnlyListTest>();
            AssertItem(thisValue, this);
        }

        [Fact]
        public void CastShouldBeValid3()
        {
            var list = new[] {this, this};
            var objValue = new ItemOrIReadOnlyList<object>(list);
            var thisValue = objValue.Cast<ItemOrIReadOnlyListTest>();
            AssertList(thisValue, list);
        }

        [Fact]
        public void FromRawValueShouldHandleList()
        {
            var list = new object[] {this, this};
            AssertList(ItemOrIReadOnlyList.FromRawValue<object>(list), list);
        }

        [Fact]
        public void FromRawValueShouldHandleNull() => AssertEmpty(ItemOrIReadOnlyList.FromRawValue<object>(null));

        [Fact]
        public void FromRawValueShouldHandleSingleItem() => AssertItem(ItemOrIReadOnlyList.FromRawValue<object>(this), this);

        [Fact]
        public void FromRawValueShouldHandleSingleItemList()
        {
            var list = new[] {this};
            AssertItem(ItemOrIReadOnlyList.FromRawValue<object>(list), list[0]);
        }

        [Fact]
        public void ShouldHandleArray1()
        {
            AssertEmpty(ItemOrIReadOnlyList.FromList<string>(array: null));
            AssertEmpty(new ItemOrIReadOnlyList<string>(array: null));
            AssertEmpty(new ItemOrIReadOnlyList<string>(Array.Empty<string>()));

            ItemOrIEnumerableTest.AssertEmpty<string>(new ItemOrIReadOnlyList<string>(array: null));
        }

        [Fact]
        public void ShouldHandleArray2()
        {
            var list = new[] {this};
            AssertItem(ItemOrIReadOnlyList.FromList(list), list[0]);
            AssertItem(list, list[0]);
            AssertItem(new ItemOrIReadOnlyList<ItemOrIReadOnlyListTest>(list), list[0]);

            ItemOrIEnumerableTest.AssertItem(new ItemOrIReadOnlyList<ItemOrIReadOnlyListTest>(list), this);
        }

        [Fact]
        public void ShouldHandleArray3()
        {
            var list = new[] {this, this};
            AssertList(ItemOrIReadOnlyList.FromList(list), list);
            AssertList(list, list);
            AssertList(new ItemOrIReadOnlyList<ItemOrIReadOnlyListTest>(list), list);

            ItemOrIEnumerableTest.AssertList(new ItemOrIReadOnlyList<ItemOrIReadOnlyListTest>(list), list);
        }

        [Fact]
        public void ShouldHandleList1()
        {
            AssertEmpty(ItemOrIReadOnlyList.FromList<string>(list: null));
            AssertEmpty(new ItemOrIReadOnlyList<string>(list: null));
            AssertEmpty(new ItemOrIReadOnlyList<string>(new List<string>()));
        }

        [Fact]
        public void ShouldHandleList2()
        {
            var list = new List<ItemOrIReadOnlyListTest> {this};
            AssertItem(ItemOrIReadOnlyList.FromList(list), list[0]);
            AssertItem(list, list[0]);
            AssertItem(new ItemOrIReadOnlyList<ItemOrIReadOnlyListTest>(list), list[0]);
        }

        [Fact]
        public void ShouldHandleList3()
        {
            var list = new List<ItemOrIReadOnlyListTest> {this, this};
            AssertList(ItemOrIReadOnlyList.FromList(list), list);
            AssertList(list, list);
            AssertList(new ItemOrIReadOnlyList<ItemOrIReadOnlyListTest>(list), list);
        }

        [Fact]
        public void ShouldHandleReadOnlyList1()
        {
            AssertEmpty(ItemOrIReadOnlyList.FromList<string>(readOnlyList: null));
            AssertEmpty(new ItemOrIReadOnlyList<string>(readOnlyList: null));
            AssertEmpty(new ItemOrIReadOnlyList<string>(readOnlyList: Array.Empty<string>()));
        }

        [Fact]
        public void ShouldHandleReadOnlyList2()
        {
            IReadOnlyList<ItemOrIReadOnlyListTest> list = new List<ItemOrIReadOnlyListTest> {this};
            AssertItem(ItemOrIReadOnlyList.FromList(list), list[0]);
            AssertItem(new ItemOrIReadOnlyList<ItemOrIReadOnlyListTest>(list), list[0]);
        }

        [Fact]
        public void ShouldHandleReadOnlyList3()
        {
            IReadOnlyList<ItemOrIReadOnlyListTest> list = new List<ItemOrIReadOnlyListTest> {this, this};
            AssertList(ItemOrIReadOnlyList.FromList(list), list);
            AssertList(new ItemOrIReadOnlyList<ItemOrIReadOnlyListTest>(list), list);
        }

        [Fact]
        public void ShouldHandleSingleItem()
        {
            AssertItem(ItemOrIReadOnlyList.FromItem(this), this);
            AssertEmpty(ItemOrIReadOnlyList.FromItem(this, false));
            AssertItem(new ItemOrIReadOnlyList<ItemOrIReadOnlyListTest>(this, true), this);
            AssertEmpty(new ItemOrIReadOnlyList<ItemOrIReadOnlyListTest>(this, false));

            ItemOrIEnumerableTest.AssertItem(new ItemOrIReadOnlyList<ItemOrIReadOnlyListTest>(this, true), this);
            ItemOrIEnumerableTest.AssertEmpty<ItemOrIReadOnlyListTest>(new ItemOrIReadOnlyList<ItemOrIReadOnlyListTest>(this, false));
        }

        internal static void AssertEmpty<T>(ItemOrIReadOnlyList<T> itemOrList) where T : class
        {
            itemOrList.Item.ShouldEqual(default!);
            itemOrList.IsEmpty.ShouldBeTrue();
            itemOrList.Count.ShouldEqual(0);
            itemOrList.HasItem.ShouldBeFalse();
            itemOrList.List.ShouldBeNull();
            itemOrList.AsList().ShouldEqual(Array.Empty<T>());
            itemOrList.ToArray().ShouldEqual(Array.Empty<T>());
            itemOrList.FirstOrDefault().ShouldEqual(default);
            itemOrList.GetRawValue().ShouldEqual(null);
            var buffer = new List<T>();
            foreach (var i in itemOrList)
                buffer.Add(i);
            buffer.ShouldEqual(Array.Empty<T>());

            buffer.Clear();
            for (var i = 0; i < itemOrList.Count; i++)
                buffer.Add(itemOrList[i]);
            buffer.ShouldEqual(Array.Empty<T>());
        }

        internal static void AssertItem<T>(ItemOrIReadOnlyList<T> itemOrList, T item) where T : class
        {
            var list = new[] {item};
            itemOrList.Item.ShouldEqual(item);
            itemOrList.IsEmpty.ShouldBeFalse();
            itemOrList.Count.ShouldEqual(1);
            itemOrList.HasItem.ShouldBeTrue();
            itemOrList.List.ShouldBeNull();
            itemOrList.AsList().ShouldEqual(list);
            itemOrList.ToArray().ShouldEqual(list);
            itemOrList.FirstOrDefault().ShouldEqual(item);
            itemOrList.GetRawValue().ShouldEqual(item);
            var buffer = new List<T>();
            foreach (var i in itemOrList)
                buffer.Add(i);
            buffer.ShouldEqual(list);

            buffer.Clear();
            for (var i = 0; i < itemOrList.Count; i++)
                buffer.Add(itemOrList[i]);
            buffer.ShouldEqual(list);
        }

        internal static void AssertList<T>(ItemOrIReadOnlyList<T> itemOrList, IReadOnlyList<T> list) where T : class
        {
            itemOrList.Item.ShouldBeNull();
            itemOrList.IsEmpty.ShouldBeFalse();
            itemOrList.Count.ShouldEqual(list.Count());
            itemOrList.HasItem.ShouldBeFalse();
            itemOrList.List.ShouldEqual(list);
            itemOrList.AsList().ShouldEqual(list);
            itemOrList.ToArray().ShouldEqual(list);
            itemOrList.FirstOrDefault().ShouldEqual(list[0]);
            itemOrList.GetRawValue().ShouldEqual(list);
            var buffer = new List<T>();
            foreach (var item in itemOrList)
                buffer.Add(item);
            buffer.ShouldEqual(list);

            buffer.Clear();
            for (var i = 0; i < itemOrList.Count; i++)
                buffer.Add(itemOrList[i]);
            buffer.ShouldEqual(list);
        }
    }
}