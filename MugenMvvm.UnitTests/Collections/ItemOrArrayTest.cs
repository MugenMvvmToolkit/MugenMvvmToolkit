using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Collections
{
    public class ItemOrArrayTest : UnitTestBase
    {
        [Fact]
        public void FromRawValueShouldHandleList()
        {
            var list = new object[] { this, this };
            AssertList(ItemOrArray.FromRawValue<object>(list), list);
        }

        [Fact]
        public void FromRawValueShouldHandleNull() => AssertEmpty(ItemOrArray.FromRawValue<object>(null));

        [Fact]
        public void FromRawValueShouldHandleSingleItem() => AssertItem(ItemOrArray.FromRawValue<object>(this), this);

        [Fact]
        public void FromRawValueShouldHandleSingleItemList()
        {
            var list = new[] { this };
            AssertItem(ItemOrArray.FromRawValue<object>(list), list[0]);
        }

        [Fact]
        public void ShouldHandleArray1()
        {
            AssertEmpty(ItemOrArray.FromList<string>(null));
            AssertEmpty(ItemOrArray.FromItem<string>(null));
            AssertEmpty(ItemOrArray.FromList(Array.Empty<string>()));

            ItemOrIReadOnlyListTest.AssertEmpty<string>(ItemOrArray.FromItem<string>(null));
            ItemOrIEnumerableTest.AssertEmpty<string>(ItemOrArray.FromItem<string>(null));
        }

        [Fact]
        public void ShouldHandleArray2()
        {
            var list = new[] { this };
            AssertItem(ItemOrArray.FromList(list), list[0]);
            AssertItem(list, list[0]);
            AssertItem(ItemOrArray.FromList(list), list[0]);

            ItemOrIReadOnlyListTest.AssertItem(ItemOrArray.FromList(list), this);
            ItemOrIEnumerableTest.AssertItem(ItemOrArray.FromList(list), this);
        }

        [Fact]
        public void ShouldHandleArray3()
        {
            var list = new[] { this, this };
            AssertList(ItemOrArray.FromList(list), list);
            AssertList(list, list);
            AssertList(ItemOrArray.FromList(list), list);

            ItemOrIReadOnlyListTest.AssertList(ItemOrArray.FromList(list), list);
            ItemOrIEnumerableTest.AssertList(ItemOrArray.FromList(list), list);
        }

        [Fact]
        public void ShouldHandleSingleItem()
        {
            AssertItem(ItemOrArray.FromItem(this), this);
            AssertEmpty(ItemOrArray.FromItem(this, false));
            AssertItem(ItemOrArray.FromItem(this, true), this);
            AssertEmpty(ItemOrArray.FromItem(this, false));

            ItemOrIReadOnlyListTest.AssertItem(ItemOrArray.FromItem(this, true), this);
            ItemOrIReadOnlyListTest.AssertEmpty<ItemOrArrayTest>(ItemOrArray.FromItem(this, false));
            ItemOrIEnumerableTest.AssertItem(ItemOrArray.FromItem(this, true), this);
            ItemOrIEnumerableTest.AssertEmpty<ItemOrArrayTest>(ItemOrArray.FromItem(this, false));
        }

        private static void AssertEmpty<T>(ItemOrArray<T> itemOrList) where T : class
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

            buffer.Clear();
            foreach (var i in itemOrList.AsEnumerable())
                buffer.Add(i);
            buffer.ShouldEqual(Array.Empty<T>());
        }

        private static void AssertItem<T>(ItemOrArray<T> itemOrList, T item) where T : class
        {
            var list = new[] { item };
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

            buffer.Clear();
            foreach (var i in itemOrList.AsEnumerable())
                buffer.Add(i);
            buffer.ShouldEqual(list);
        }

        private static void AssertList<T>(ItemOrArray<T> itemOrList, T[] list) where T : class
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

            buffer.Clear();
            foreach (var i in itemOrList.AsEnumerable())
                buffer.Add(i);
            buffer.ShouldEqual(list);
        }
    }
}