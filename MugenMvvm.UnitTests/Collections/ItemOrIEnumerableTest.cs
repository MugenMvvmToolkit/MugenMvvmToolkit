﻿using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using Should;
using Xunit;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.UnitTests.Collections
{
    public class ItemOrIEnumerableTest : UnitTestBase
    {
        [Fact]
        public void FromRawValueShouldHandleList()
        {
            var list = new object[] { this, this };
            AssertList(ItemOrIEnumerable.FromRawValue<object>(list), list);
        }

        [Fact]
        public void FromRawValueShouldHandleNull() => AssertEmpty(ItemOrIEnumerable.FromRawValue<object>(null));

        [Fact]
        public void FromRawValueShouldHandleSingleItem() => AssertItem(ItemOrIEnumerable.FromRawValue<object>(this), this);

        [Fact]
        public void FromRawValueShouldHandleSingleItemList()
        {
            var list = new[] { this };
            AssertItem(ItemOrIEnumerable.FromRawValue<object>(list), list[0]);
        }

        [Fact]
        public void ShouldHandleArray1()
        {
            AssertEmpty(ItemOrIEnumerable.FromList<string>(array: null));
            AssertEmpty(ItemOrIEnumerable.FromList<string>(array: null));
            AssertEmpty(ItemOrIEnumerable.FromList(Array.Empty<string>()));
        }

        [Fact]
        public void ShouldHandleArray2()
        {
            var list = new[] { this };
            AssertItem(ItemOrIEnumerable.FromList(list), list[0]);
            AssertItem(list, list[0]);
            AssertItem(ItemOrIEnumerable.FromList(list), list[0]);
        }

        [Fact]
        public void ShouldHandleArray3()
        {
            var list = new[] { this, this };
            AssertList(ItemOrIEnumerable.FromList(list), list);
            AssertList(list, list);
            AssertList(ItemOrIEnumerable.FromList(list), list);
        }

        [Fact]
        public void ShouldHandleEnumerable1()
        {
            AssertEmpty(ItemOrIEnumerable.FromList<string>(enumerable: null));
            AssertEmpty(ItemOrIEnumerable.FromList<string>(list: null));
            AssertEmpty(ItemOrIEnumerable.FromList(enumerable: Array.Empty<string>()));
        }

        [Fact]
        public void ShouldHandleEnumerable2()
        {
            var list = new List<ItemOrIEnumerableTest> { this }.Where(_ => true);
            AssertItem(ItemOrIEnumerable.FromList(list), list.ElementAt(0));
            AssertItem(ItemOrIEnumerable.FromList(list), list.ElementAt(0));
        }

        [Fact]
        public void ShouldHandleEnumerable3()
        {
            var list = new List<ItemOrIEnumerableTest> { this, this }.Where(_ => true);
            AssertList(ItemOrIEnumerable.FromList(list), list);
            AssertList(ItemOrIEnumerable.FromList(list), list);
        }

        [Fact]
        public void ShouldHandleList1()
        {
            AssertEmpty(ItemOrIEnumerable.FromList<string>(list: null));
            AssertEmpty(ItemOrIEnumerable.FromList<string>(list: null));
            AssertEmpty(ItemOrIEnumerable.FromList(new List<string>()));
        }

        [Fact]
        public void ShouldHandleList2()
        {
            var list = new List<ItemOrIEnumerableTest> { this };
            AssertItem(ItemOrIEnumerable.FromList(list), list[0]);
            AssertItem(list, list[0]);
            AssertItem(ItemOrIEnumerable.FromList(list), list[0]);
        }

        [Fact]
        public void ShouldHandleList3()
        {
            var list = new List<ItemOrIEnumerableTest> { this, this };
            AssertList(ItemOrIEnumerable.FromList(list), list);
            AssertList(list, list);
            AssertList(ItemOrIEnumerable.FromList(list), list);
        }

        [Fact]
        public void ShouldHandleReadOnlyList1()
        {
            AssertEmpty(ItemOrIEnumerable.FromList<string>(readOnlyList: null));
            AssertEmpty(ItemOrIEnumerable.FromList<string>(readOnlyList: null));
        }

        [Fact]
        public void ShouldHandleReadOnlyList2()
        {
            IReadOnlyList<ItemOrIEnumerableTest> list = new List<ItemOrIEnumerableTest> { this };
            AssertItem(ItemOrIEnumerable.FromList(list), list[0]);
            AssertItem(ItemOrIEnumerable.FromList(list), list[0]);
            AssertEmpty(ItemOrIEnumerable.FromList(readOnlyList: Array.Empty<string>()));
        }

        [Fact]
        public void ShouldHandleReadOnlyList3()
        {
            IReadOnlyList<ItemOrIEnumerableTest> list = new List<ItemOrIEnumerableTest> { this, this };
            AssertList(ItemOrIEnumerable.FromList(list), list);
            AssertList(ItemOrIEnumerable.FromList(list), list);
        }

        [Fact]
        public void ShouldHandleSingleItem()
        {
            AssertItem(ItemOrIEnumerable.FromItem(this), this);
            AssertEmpty(ItemOrIEnumerable.FromItem(this, false));
            AssertItem(ItemOrIEnumerable.FromItem(this, true), this);
            AssertEmpty(ItemOrIEnumerable.FromItem(this, false));
        }

        internal static void AssertEmpty<T>(ItemOrIEnumerable<T> itemOrList) where T : class
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
            foreach (var i in itemOrList.AsEnumerable())
                buffer.Add(i);
            buffer.ShouldEqual(Array.Empty<T>());
        }

        internal static void AssertItem<T>(ItemOrIEnumerable<T> itemOrList, T item) where T : class
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
            foreach (var i in itemOrList.AsEnumerable())
                buffer.Add(i);
            buffer.ShouldEqual(list);
        }

        internal static void AssertList<T>(ItemOrIEnumerable<T> itemOrList, IEnumerable<T> list) where T : class
        {
            itemOrList.Item.ShouldBeNull();
            itemOrList.IsEmpty.ShouldBeFalse();
            itemOrList.Count.ShouldEqual(list.Count());
            itemOrList.HasItem.ShouldBeFalse();
            itemOrList.List.ShouldEqual(list);
            itemOrList.ToArray().ShouldEqual(list);
            itemOrList.AsList().ShouldEqual(list);
            itemOrList.FirstOrDefault().ShouldEqual(list.FirstOrDefault());
            itemOrList.GetRawValue().ShouldEqual(list);
            var buffer = new List<T>();
            foreach (var item in itemOrList)
                buffer.Add(item);
            buffer.ShouldEqual(list);

            buffer.Clear();
            foreach (var i in itemOrList.AsEnumerable())
                buffer.Add(i);
            buffer.ShouldEqual(list);
        }
    }
}