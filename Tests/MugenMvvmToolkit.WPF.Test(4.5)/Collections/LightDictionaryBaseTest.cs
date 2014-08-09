using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Collections;
using Should;

namespace MugenMvvmToolkit.Test.Collections
{
    [TestClass]
    public class LightDictionaryBaseTest : TestBase
    {
        #region Nested types

        private sealed class LightDictionary<TKey, TValue> : LightDictionaryBase<TKey, TValue>
        {
            private readonly IEqualityComparer<TKey> _comparer;

            public LightDictionary(IEqualityComparer<TKey> comparer = null)
                : base(true)
            {
                _comparer = comparer ?? EqualityComparer<TKey>.Default;
            }

            #region Overrides of LightDictionaryBase<TKey,TValue>

            /// <summary>
            ///     Determines whether the specified objects are equal.
            /// </summary>
            protected override bool Equals(TKey x, TKey y)
            {
                return _comparer.Equals(x, y);
            }

            /// <summary>
            ///     Returns a hash code for the specified object.
            /// </summary>
            protected override int GetHashCode(TKey key)
            {
                return _comparer.GetHashCode(key);
            }

            #endregion
        }

        #endregion

        #region Methods

        [TestMethod]
        public virtual void BaseOperationTest()
        {
            var dict = Create<string, string>();
            dict.Count.ShouldEqual(0, "new dict should be empty");
            dict.Add("A", "B");
            dict.Count.ShouldEqual(1);
            dict["A"].ShouldEqual("B");
            dict.Add("C", "D");
            dict.Count.ShouldEqual(2);
            dict["A"].ShouldEqual("B");
            dict["C"].ShouldEqual("D");
            dict.Remove("A").ShouldBeTrue();
            dict.Count.ShouldEqual(1);
            dict["C"].ShouldEqual("D");
            dict.Remove("Z").ShouldBeFalse();
            dict.Count.ShouldEqual(1);
            dict["C"].ShouldEqual("D");
        }

        [TestMethod]
        public virtual void ContainsTest()
        {
            const string name = "A";
            var item = new Item();
            var collection = Create<Item, string>();
            collection.Add(item, name);
            collection.Count.ShouldEqual(1);
            collection.ContainsKey(item).ShouldBeTrue();
        }

        [TestMethod]
        public virtual void IllegalAddTest()
        {
            var item = new Item();
            var dict = Create<Item, Item>();
            dict.Add(item, item);
            ShouldThrow(() => dict.Add(item, item));
        }

        [TestMethod]
        public virtual void GettingNonExistingTest()
        {
            var dict = Create<Item, Item>();
            ShouldThrow(() => Equals(dict[new Item()], null));
        }

        [TestMethod]
        public virtual void SetterTest()
        {
            var key1 = new Item();
            var key2 = new Item();
            const string value1 = "v1";
            const string value2 = "v2";
            const string value3 = "v3";
            var dict = Create<Item, string>();

            dict[key1] = value1;
            dict[key1].ShouldEqual(value1);

            dict[key1] = value2;
            dict[key1].ShouldEqual(value2);

            dict[key2] = value3;
            dict[key1].ShouldEqual(value2);
            dict[key2].ShouldEqual(value3);
        }

        [TestMethod]
        public virtual void ToArrayTest()
        {
            var item1 = new Item();
            var item2 = new Item();
            var item3 = new Item();
            var item4 = new Item();
            var dict = Create<Item, string>();
            dict[item1] = "1";
            dict[item2] = "2";
            dict[item3] = "3";
            dict[item4] = "4";
            var array = dict.ToArray();

            array.Length.ShouldEqual(4);
            var keys = array.Select(pair => pair.Key).ToArray();
            keys.Contains(item4).ShouldBeTrue();
            keys.Contains(item3).ShouldBeTrue();
            keys.Contains(item2).ShouldBeTrue();
            keys.Contains(item1).ShouldBeTrue();

            var values = array.Select(pair => pair.Value).ToArray();
            values.Contains("1").ShouldBeTrue();
            values.Contains("2").ShouldBeTrue();
            values.Contains("3").ShouldBeTrue();
            values.Contains("4").ShouldBeTrue();
        }

        [TestMethod]
        public virtual void ClearItemsTest()
        {
            var item1 = new Item();
            var item2 = new Item();
            LightDictionaryBase<Item, Item> dict = Create<Item, Item>();
            dict.Add(item1, item1);
            dict.Add(item2, item2);
            dict.Count.ShouldEqual(2);
            dict.Clear();
            dict.Count.ShouldEqual(0);
            dict.ContainsKey(item1).ShouldBeFalse();
            dict.ContainsKey(item2).ShouldBeFalse();
        }

        [TestMethod]
        public virtual void ComparerTest()
        {
            LightDictionaryBase<string, string> dict = Create<string, string>(StringComparer.Ordinal);
            dict["a"] = "a";
            dict["A"] = "A";
            dict.Count.ShouldEqual(2);
            dict["a"].ShouldEqual("a");
            dict["A"].ShouldEqual("A");

            dict = Create<string, string>(StringComparer.OrdinalIgnoreCase);
            dict["a"] = "a";
            dict["A"] = "A";
            dict.Count.ShouldEqual(1);
            dict["a"].ShouldEqual("A");
            dict["A"].ShouldEqual("A");
        }

        protected virtual LightDictionaryBase<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey> comparer = null)
            where TKey : class
        {
            return new LightDictionary<TKey, TValue>(comparer);
        }

        #endregion
    }
}