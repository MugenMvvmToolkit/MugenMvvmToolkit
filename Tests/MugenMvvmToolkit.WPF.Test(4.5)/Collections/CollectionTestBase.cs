using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Test.TestInfrastructure;
using Should;

namespace MugenMvvmToolkit.Test.Collections
{

    [DebuggerDisplay("Id = {Id}")]
    public sealed class Item
    {
        private static int IdGenerator = -1;

        public Item()
        {
            Id = Interlocked.Increment(ref IdGenerator);
        }

        public bool Hidden { get; set; }

        public int Id { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}";
        }
    }

    [TestClass]
    public abstract class CollectionTestBase
    {
        [TestMethod]
        public void CreateWithItemsTest()
        {
            var items = new[] { new Item(), new Item() };
            ICollection<Item> collection = CreateCollection(items);
            collection.Count.ShouldEqual(2);
            collection.Any(item => item == items[0]).ShouldBeTrue();
            collection.Any(item => item == items[1]).ShouldBeTrue();
        }

        [TestMethod]
        public void AddTest()
        {
            var item = new Item();
            ICollection<Item> collection = CreateCollection<Item>();
            collection.Add(item);
            collection.Count.ShouldEqual(1);
            collection.Any(item1 => item1 == item).ShouldBeTrue();
        }

        [TestMethod]
        public void RemoveTest()
        {
            var item = new Item();
            ICollection<Item> collection = CreateCollection<Item>();
            collection.Add(item);
            collection.Count.ShouldEqual(1);
            collection.Any(item1 => item1 == item).ShouldBeTrue();

            collection.Remove(item);
            collection.Count.ShouldEqual(0);
            collection.Any(item1 => item1 == item).ShouldBeFalse();
        }

        [TestMethod]
        public void ContainsTest()
        {
            var item = new Item();
            ICollection<Item> collection = CreateCollection<Item>();
            collection.Add(item);
            collection.Count.ShouldEqual(1);
            collection.Contains(item).ShouldBeTrue();
        }

        [TestMethod]
        public void CopyToTest()
        {
            var item = new Item();
            ICollection<Item> collection = CreateCollection<Item>();
            collection.Add(item);

            var items = new Item[1];
            collection.CopyTo(items, 0);
            items[0].ShouldEqual(item);
        }

        [TestMethod]
        public void ClearItemsTest()
        {
            var items = new[] { new Item(), new Item() };
            ICollection<Item> collection = CreateCollection(items);
            collection.Count.ShouldEqual(2);
            collection.Any(item => item == items[0]).ShouldBeTrue();
            collection.Any(item => item == items[1]).ShouldBeTrue();

            collection.Clear();
            if (collection.Count != 0)
                collection.Count.ShouldEqual(0);
        }

        protected abstract ICollection<T> CreateCollection<T>(params T[] items) where T : class;
    }
}
