using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using Should;
using Xunit;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.UnitTests.Collections
{
    public class ItemOrIReadOnlyCollectionTest : UnitTestBase
    {
        [Fact]
        public void FromRawValueShouldHandleList()
        {
            var list = new object[] { this, this };
            AssertList(ItemOrIReadOnlyCollection.FromRawValue<object>(list), list);
        }

        [Fact]
        public void FromRawValueShouldHandleNull() => AssertEmpty(ItemOrIReadOnlyCollection.FromRawValue<object>(null));

        [Fact]
        public void FromRawValueShouldHandleSingleItem() => AssertItem(ItemOrIReadOnlyCollection.FromRawValue<object>(this), this);

        [Fact]
        public void FromRawValueShouldHandleSingleItemList()
        {
            var list = new[] { this };
            AssertItem(ItemOrIReadOnlyCollection.FromRawValue<object>(list), list[0]);
        }

        [Fact]
        public void ShouldHandleArray1()
        {
            AssertEmpty(ItemOrIReadOnlyCollection.FromList<string>(array: null));
            AssertEmpty(ItemOrIReadOnlyCollection.FromList<string>(array: null));
            AssertEmpty(ItemOrIReadOnlyCollection.FromList(Array.Empty<string>()));
        }

        [Fact]
        public void ShouldHandleArray2()
        {
            var list = new[] { this };
            AssertItem(ItemOrIReadOnlyCollection.FromList(list), list[0]);
            AssertItem(list, list[0]);
            AssertItem(ItemOrIReadOnlyCollection.FromList(list), list[0]);
        }

        [Fact]
        public void ShouldHandleArray3()
        {
            var list = new[] { this, this };
            AssertList(ItemOrIReadOnlyCollection.FromList(list), list);
            AssertList(list, list);
            AssertList(ItemOrIReadOnlyCollection.FromList(list), list);
        }

        [Fact]
        public void ShouldHandleReadOnlyCollection1()
        {
            AssertEmpty(ItemOrIReadOnlyCollection.FromList<string>(readOnlyCollection: null));
            AssertEmpty(ItemOrIReadOnlyCollection.FromList<string>(list: null));
            AssertEmpty(ItemOrIReadOnlyCollection.FromList(Array.Empty<string>()));
        }

        [Fact]
        public void ShouldHandleReadOnlyCollection2()
        {
            var list = new HashSet<ItemOrIReadOnlyCollectionTest> { this };
            AssertList(ItemOrIReadOnlyCollection.FromList(list), list);
        }

        [Fact]
        public void ShouldHandleReadOnlyCollection3()
        {
            var list = new Dictionary<ItemOrIReadOnlyCollectionTest, object> { { this, "" } }.Keys;
            AssertList(ItemOrIReadOnlyCollection.FromList(list), list);
        }

        [Fact]
        public void ShouldHandleList1()
        {
            AssertEmpty(ItemOrIReadOnlyCollection.FromList<string>(list: null));
            AssertEmpty(ItemOrIReadOnlyCollection.FromList<string>(list: null));
            AssertEmpty(ItemOrIReadOnlyCollection.FromList(new List<string>()));
        }

        [Fact]
        public void ShouldHandleList2()
        {
            var list = new List<ItemOrIReadOnlyCollectionTest> { this };
            AssertItem(ItemOrIReadOnlyCollection.FromList(list), list[0]);
            AssertItem(list, list[0]);
            AssertItem(ItemOrIReadOnlyCollection.FromList(list), list[0]);
        }

        [Fact]
        public void ShouldHandleList3()
        {
            var list = new List<ItemOrIReadOnlyCollectionTest> { this, this };
            AssertList(ItemOrIReadOnlyCollection.FromList(list), list);
            AssertList(list, list);
            AssertList(ItemOrIReadOnlyCollection.FromList(list), list);
        }

        [Fact]
        public void ShouldHandleReadOnlyList1()
        {
            AssertEmpty(ItemOrIReadOnlyCollection.FromList<string>(readOnlyList: null));
            AssertEmpty(ItemOrIReadOnlyCollection.FromList<string>(readOnlyList: null));
        }

        [Fact]
        public void ShouldHandleReadOnlyList2()
        {
            IReadOnlyList<ItemOrIReadOnlyCollectionTest> list = new List<ItemOrIReadOnlyCollectionTest> { this };
            AssertItem(ItemOrIReadOnlyCollection.FromList(list), list[0]);
            AssertItem(ItemOrIReadOnlyCollection.FromList(list), list[0]);
            AssertEmpty(ItemOrIReadOnlyCollection.FromList(readOnlyList: Array.Empty<string>()));
        }

        [Fact]
        public void ShouldHandleReadOnlyList3()
        {
            IReadOnlyList<ItemOrIReadOnlyCollectionTest> list = new List<ItemOrIReadOnlyCollectionTest> { this, this };
            AssertList(ItemOrIReadOnlyCollection.FromList(list), list);
            AssertList(ItemOrIReadOnlyCollection.FromList(list), list);
        }

        [Fact]
        public void ShouldHandleSingleItem()
        {
            AssertItem(ItemOrIReadOnlyCollection.FromItem(this), this);
            AssertEmpty(ItemOrIReadOnlyCollection.FromItem(this, false));
            AssertItem(ItemOrIReadOnlyCollection.FromItem(this, true), this);
            AssertEmpty(ItemOrIReadOnlyCollection.FromItem(this, false));
        }

        internal static void AssertEmpty<T>(ItemOrIReadOnlyCollection<T> itemOrList) where T : class
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

        internal static void AssertItem<T>(ItemOrIReadOnlyCollection<T> itemOrList, T item) where T : class
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

        internal static void AssertList<T>(ItemOrIReadOnlyCollection<T> itemOrList, IReadOnlyCollection<T> list) where T : class
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