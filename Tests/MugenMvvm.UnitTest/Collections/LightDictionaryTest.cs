using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using MugenMvvm.Collections;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Collections
{
    //https://github.com/mdae/MonoRT/blob/dcc14105888ec567d00d0deec825c9c19cb875f2/mono-rt/mcs/class/corlib/Test/System.Collections.Generic/DictionaryTest.cs
    public class LightDictionaryTest : UnitTestBase
    {
        #region Fields

        private readonly LightDictionary<string, object> _dictionary;
        private readonly LightDictionary<MyClass, MyClass> _dictionary2;
        private readonly LightDictionary<int, int> _dictionary3;

        #endregion

        #region Constructors

        public LightDictionaryTest()
        {
            _dictionary = new LightDictionary<string, object>();
            _dictionary2 = new LightDictionary<MyClass, MyClass>();
            _dictionary3 = new LightDictionary<int, int>();
        }

        #endregion

        #region Methods

        [Fact]
        public void InternalValuesTest()
        {
            var dictionary = new LightDictionary<int, int>();
            var negativeDictionary = new LightDictionary<int, int>();
            for (int i = 0; i < 100; i++)
            {
                dictionary[i] = i;
                negativeDictionary[i] = -i;
            }

            var valuesInternal = dictionary.ValuesInternal;
            foreach (var keyValuePair in dictionary)
                valuesInternal.Contains(keyValuePair).ShouldBeTrue();

            var clone = new LightDictionary<int, int>(-1);
            clone.ValuesInternal = negativeDictionary.ValuesInternal;
            clone.SequenceEqual(negativeDictionary).ShouldBeTrue();
        }

        [Fact]
        public void CloneTest()
        {
            var dictionary = new LightDictionary<int, int>();
            var negativeDictionary = new LightDictionary<int, int>();
            for (int i = 0; i < 100; i++)
            {
                dictionary[i] = i;
                negativeDictionary[i] = -i;
            }

            var clone = new LightDictionary<int, int>(-1);
            dictionary.Clone(clone);
            clone.SequenceEqual(dictionary).ShouldBeTrue();

            clone = new LightDictionary<int, int>(-1);
            dictionary.Clone(clone, i => -i);
            clone.SequenceEqual(negativeDictionary).ShouldBeTrue();
        }

        [Fact]
        public void ToArrayTest()
        {
            var m1 = new MyClass(10, 5);
            var m2 = new MyClass(20, 5);
            var m3 = new MyClass(12, 3);
            _dictionary2.Add(m1, m1);
            _dictionary2.Add(m2, m2);
            _dictionary2.Add(m3, m3);

            var array = _dictionary2.ToArray();
            array.Length.ShouldEqual(3);
            array.ShouldContain(new KeyValuePair<MyClass, MyClass>(m1, m1));
            array.ShouldContain(new KeyValuePair<MyClass, MyClass>(m2, m2));
            array.ShouldContain(new KeyValuePair<MyClass, MyClass>(m3, m3));
        }

        [Fact]
        public void AddTest()
        {
            _dictionary.Add("key1", "value");
            "value".ShouldEqual(_dictionary["key1"].ToString(), "Add failed!");
        }

        [Fact]
        public void AddTest2()
        {
            var m1 = new MyClass(10, 5);
            var m2 = new MyClass(20, 5);
            var m3 = new MyClass(12, 3);
            _dictionary2.Add(m1, m1);
            _dictionary2.Add(m2, m2);
            _dictionary2.Add(m3, m3);
            20.ShouldEqual(_dictionary2[m2].Value, "#1");
            10.ShouldEqual(_dictionary2[m1].Value, "#2");
            12.ShouldEqual(_dictionary2[m3].Value, "#3");
        }

        [Fact]
        public void AddTest3()
        {
            _dictionary3.Add(1, 2);
            _dictionary3.Add(2, 3);
            _dictionary3.Add(3, 4);
            2.ShouldEqual(_dictionary3[1], "#1");
            3.ShouldEqual(_dictionary3[2], "#2");
            4.ShouldEqual(_dictionary3[3], "#3");
        }

        [Fact]
        public void AddNullTest()
        {
            ShouldThrow<ArgumentNullException>(() => _dictionary.Add(null!, ""));
        }

        [Fact]
        public void AddDuplicateTest()
        {
            _dictionary.Add("foo", "bar");
            ShouldThrow<ArgumentException>(() => _dictionary.Add("foo", "bar"));
        }

        //Tests Add when resize takes place
        [Fact]
        public void AddLargeTest()
        {
            int i, numElems = 50;

            for (i = 0; i < numElems; i++)
                _dictionary3.Add(i, i);

            i = 0;
            foreach (var entry in _dictionary3)
                i++;

            i.ShouldEqual(numElems, "Add with resize failed!");
        }

        [Fact]
        public void IndexerGetExistingTest()
        {
            _dictionary.Add("key1", "value");
            "value".ShouldEqual(_dictionary["key1"].ToString(), "Add failed!");
        }

        [Fact]
        public void IndexerGetNonExistingTest()
        {
            ShouldThrow<KeyNotFoundException>(() =>
            {
                var foo = _dictionary["foo"];
            });
        }

        [Fact]
        public void IndexerGetNullTest()
        {
            ShouldThrow<ArgumentNullException>(() =>
            {
                var s = _dictionary[null!];
            });
        }

        [Fact]
        public void IndexerSetExistingTest()
        {
            _dictionary.Add("key1", "value1");
            _dictionary["key1"] = "value2";
            1.ShouldEqual(_dictionary.Count);
            "value2".ShouldEqual(_dictionary["key1"]);
        }

        [Fact]
        public void IndexerSetNonExistingTest()
        {
            _dictionary["key1"] = "value1";
            1.ShouldEqual(_dictionary.Count);
            "value1".ShouldEqual(_dictionary["key1"]);
        }

        [Fact]
        public void RemoveTest()
        {
            _dictionary.Add("key1", "value1");
            _dictionary.Add("key2", "value2");
            _dictionary.Add("key3", "value3");
            _dictionary.Add("key4", "value4");
            _dictionary.Remove("key3").ShouldBeTrue();
            _dictionary.Remove("foo").ShouldBeFalse();
            3.ShouldEqual(_dictionary.Count);
            _dictionary.ContainsKey("key3").ShouldBeFalse();
        }

        [Fact]
        public void RemoveTest2()
        {
            var m1 = new MyClass(10, 5);
            var m2 = new MyClass(20, 5);
            var m3 = new MyClass(12, 3);
            _dictionary2.Add(m1, m1);
            _dictionary2.Add(m2, m2);
            _dictionary2.Add(m3, m3);
            _dictionary2.Remove(m1); // m2 is in rehash path
            20.ShouldEqual(_dictionary2[m2].Value, "#4");
        }

#if !DEBUG
        [Fact]
        [Category("NotWorking")]
        public void Remove_ZeroOut()
        {
            var key = new object();
            var value = new object();

            var wrKey = new WeakReference(key);
            var wrValue = new WeakReference(value);

            var dictionary = new LightDictionary<object, object>();
            dictionary.Add(key, value);
            dictionary.Remove(key);

            key = null;
            value = null;
            GC.Collect();
            Thread.Sleep(200);

            wrKey.Target.ShouldBeNull("#1");
            wrValue.Target.ShouldBeNull("#2");
        }
#endif

        [Fact]
        public void IndexerSetNullTest()
        {
            ShouldThrow<ArgumentNullException>(() => { _dictionary[null!] = "bar"; });
        }

        [Fact]
        public void ClearTest()
        {
            _dictionary.Add("key1", "value1");
            _dictionary.Add("key2", "value2");
            _dictionary.Add("key3", "value3");
            _dictionary.Add("key4", "value4");
            _dictionary.Clear();
            0.ShouldEqual(_dictionary.Count, "Clear method failed!");
            _dictionary.ContainsKey("key2").ShouldBeFalse();
        }

        [Fact] // bug 432441
        public void Clear_Iterators()
        {
            var d = new LightDictionary<object, object>();

            d[new object()] = new object();
            d.Clear();
            var hash = 0;
            foreach (object o in d)
                hash += o.GetHashCode();
        }

#if !DEBUG
        [Fact]
        [Category("NotWorking")]
        public void Clear_ZeroOut()
        {
            var key = new object();
            var value = new object();

            var wrKey = new WeakReference(key);
            var wrValue = new WeakReference(value);

            var dictionary = new LightDictionary<object, object>();
            dictionary.Add(key, value);
            dictionary.Clear();

            key = null;
            value = null;
            GC.Collect();
            Thread.Sleep(200);

            wrKey.Target.ShouldBeNull("#1");
            wrValue.Target.ShouldBeNull("#2");
        }
#endif

        [Fact]
        public void ContainsKeyTest()
        {
            _dictionary.Add("key1", "value1");
            _dictionary.Add("key2", "value2");
            _dictionary.Add("key3", "value3");
            _dictionary.Add("key4", "value4");
            var contains = _dictionary.ContainsKey("key4");
            contains.ShouldBeTrue("ContainsKey does not return correct value!");
            contains = _dictionary.ContainsKey("key5");
            contains.ShouldBeFalse("ContainsKey for non existant does not return correct value!");
        }

        [Fact]
        public void ContainsKeyTest2()
        {
            ShouldThrow<ArgumentNullException>(() => _dictionary.ContainsKey(null!));
        }

        [Fact]
        public void TryGetValueTest()
        {
            _dictionary.Add("key1", "value1");
            _dictionary.Add("key2", "value2");
            _dictionary.Add("key3", "value3");
            _dictionary.Add("key4", "value4");
            object value = "";
            var retrieved = _dictionary.TryGetValue("key4", out value);
            retrieved.ShouldBeTrue();
            "value4".ShouldEqual((string)value, "TryGetValue does not return value!");

            retrieved = _dictionary.TryGetValue("key7", out value);
            retrieved.ShouldBeFalse();
            value.ShouldBeNull("value for non existant value should be null!");
        }

        [Fact]
        public void ValueTypeTest()
        {
            var dict = new LightDictionary<int, float>();
            dict.Add(10, 10.3f);
            dict.Add(11, 10.4f);
            dict.Add(12, 10.5f);
            10.4f.ShouldEqual(dict[11], "#5");
        }

        [Fact]
        public void ObjectAsKeyTest()
        {
            var dict = new LightDictionary<object, object>();
            MyTest key1, key2, key3;
            dict.Add(key1 = new MyTest("key1", 234), "value1");
            dict.Add(key2 = new MyTest("key2", 444), "value2");
            dict.Add(key3 = new MyTest("key3", 5655), "value3");

            "value2".ShouldEqual(dict[key2], "value is not returned!");
            "value3".ShouldEqual(dict[key3], "neg: exception should not be thrown!");
        }

        [Fact]
        public void IEnumeratorTest()
        {
            _dictionary.Add("key1", "value1");
            _dictionary.Add("key2", "value2");
            _dictionary.Add("key3", "value3");
            _dictionary.Add("key4", "value4");
            var itr = ((IEnumerable)_dictionary).GetEnumerator();
            while (itr.MoveNext())
            {
                var o = itr.Current;
                typeof(KeyValuePair<string, object>).ShouldEqual(o.GetType(), "Current should return a type of KeyValuePair");
                var entry = (KeyValuePair<string, object>)itr.Current;
            }

            "value4".ShouldEqual(_dictionary["key4"].ToString(), "");
        }


        [Fact]
        public void IEnumeratorGenericTest()
        {
            _dictionary.Add("key1", "value1");
            _dictionary.Add("key2", "value2");
            _dictionary.Add("key3", "value3");
            _dictionary.Add("key4", "value4");
            var itr = ((IEnumerable<KeyValuePair<string, object>>)_dictionary).GetEnumerator();
            while (itr.MoveNext())
            {
                object o = itr.Current;
                typeof(KeyValuePair<string, object>).ShouldEqual(o.GetType(), "Current should return a type of KeyValuePair<object,string>");
                var entry = itr.Current;
            }

            "value4".ShouldEqual(_dictionary["key4"].ToString(), "");
        }

        [Fact]
        public void ForEachTest()
        {
            _dictionary.Add("key1", "value1");
            _dictionary.Add("key2", "value2");
            _dictionary.Add("key3", "value3");
            _dictionary.Add("key4", "value4");

            var i = 0;
            foreach (var entry in _dictionary)
                i++;
            4.ShouldEqual(i, "fail1: foreach entry failed!");

            i = 0;
            foreach (KeyValuePair<string, object> entry in (IEnumerable)_dictionary)
                i++;
            4.ShouldEqual(i, "fail2: foreach entry failed!");
        }

        [Fact]
        public void ResizeTest()
        {
            var dictionary = new LightDictionary<string, object>(3);
            dictionary.Add("key1", "value1");
            dictionary.Add("key2", "value2");
            dictionary.Add("key3", "value3");

            3.ShouldEqual(dictionary.Count);

            dictionary.Add("key4", "value4");
            4.ShouldEqual(dictionary.Count);
            "value1".ShouldEqual(dictionary["key1"].ToString(), "");
            "value2".ShouldEqual(dictionary["key2"].ToString(), "");
            "value4".ShouldEqual(dictionary["key4"].ToString(), "");
            "value3".ShouldEqual(dictionary["key3"].ToString(), "");
        }

        [Fact]
        public void PlainEnumeratorReturnTest()
        {
            // Test that we return a KeyValuePair even for non-generic dictionary iteration
            _dictionary["foo"] = "bar";
            IEnumerator<KeyValuePair<string, object>> enumerator = _dictionary.GetEnumerator();
            enumerator.MoveNext().ShouldBeTrue("#1");
            typeof(KeyValuePair<string, object>).ShouldEqual(((IEnumerator)enumerator).Current.GetType(), "#2");
            typeof(KeyValuePair<string, object>).ShouldEqual(((object)enumerator.Current).GetType(), "#5");
        }

        [Fact]
        [Category("TargetJvmNotWorking")] // BUGBUG Very very slow on TARGET_JVM.
        public void SerializationTest()
        {
            for (var i = 0; i < 50; i++)
                _dictionary3.Add(i, i);

            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            formatter.Serialize(stream, _dictionary3);

            stream.Position = 0;
            var deserialized = formatter.Deserialize(stream);

            deserialized.ShouldNotBeNull();
            (deserialized == _dictionary3).ShouldBeFalse();

            (deserialized is LightDictionary<int, int>).ShouldBeTrue();
            var d3 = deserialized as LightDictionary<int, int>;

            50.ShouldEqual(d3!.Count);
            for (var i = 0; i < 50; i++)
                i.ShouldEqual(d3[i]);
        }

        [Fact]
        public void ZeroCapacity()
        {
            var x = new LightDictionary<int, int>(0);
            x.Add(1, 2);

            x = new LightDictionary<int, int>(0);
            x.Clear();

            x = new LightDictionary<int, int>(0);
            var aa = x.Count;

            x = new LightDictionary<int, int>(0);
            try
            {
                var j = x[1];
            }
            catch (KeyNotFoundException)
            {
            }

            bool b;
            b = x.ContainsKey(10);

            x = new LightDictionary<int, int>(0);
            x.Remove(10);

            x = new LightDictionary<int, int>(0);
            x.TryGetValue(1, out _);

            object oa = x.Keys;
            object ob = x.Values;
            foreach (var a in x)
            {
            }
        }

        [Fact] // bug #332534
        public void Dictionary_MoveNext()
        {
            var a = new LightDictionary<int, int>();
            a.Add(3, 1);
            a.Add(4, 1);

            IEnumerator en = a.GetEnumerator();
            for (var i = 1; i < 10; i++)
                en.MoveNext();
        }

        [Fact]
        public void KeyObjectMustNotGetChangedIfKeyAlreadyExists()
        {
            var d = new LightDictionary<string, int>();
            var s1 = "Test";
            var s2 = "Tes" + "T".ToLowerInvariant();
            d[s1] = 1;
            d[s2] = 2;
            var comp = string.Empty;
            foreach (var s in d.Keys)
                comp = s;
            ReferenceEquals(s1, comp).ShouldBeTrue();
        }

        [Fact]
        public void ResetKeysEnumerator()
        {
            var test = new LightDictionary<string, string>();
            test.Add("monkey", "singe");
            test.Add("singe", "mono");
            test.Add("mono", "monkey");

            IEnumerator enumerator = test.Keys.GetEnumerator();

            enumerator.MoveNext().ShouldBeTrue();
            enumerator.MoveNext().ShouldBeTrue();

            enumerator.Reset();

            enumerator.MoveNext().ShouldBeTrue();
            enumerator.MoveNext().ShouldBeTrue();
            enumerator.MoveNext().ShouldBeTrue();
            enumerator.MoveNext().ShouldBeFalse();
        }

        [Fact]
        public void ResetValuesEnumerator()
        {
            var test = new LightDictionary<string, string>();
            test.Add("monkey", "singe");
            test.Add("singe", "mono");
            test.Add("mono", "monkey");

            IEnumerator enumerator = test.Values.GetEnumerator();

            enumerator.MoveNext().ShouldBeTrue();
            enumerator.MoveNext().ShouldBeTrue();

            enumerator.Reset();

            enumerator.MoveNext().ShouldBeTrue();
            enumerator.MoveNext().ShouldBeTrue();
            enumerator.MoveNext().ShouldBeTrue();
            enumerator.MoveNext().ShouldBeFalse();
        }

        #endregion

        #region Nested types

        private class MyClass
        {
            #region Fields

            private readonly int _b;

            #endregion

            #region Constructors

            public MyClass(int a, int b)
            {
                Value = a;
                _b = b;
            }

            #endregion

            #region Properties

            public int Value { get; }

            #endregion

            #region Methods

            public override int GetHashCode()
            {
                return Value + _b;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is MyClass))
                    return false;
                return ((MyClass)obj).Value == Value;
            }

            #endregion
        }

        private class MyTest
        {
            #region Fields

            public readonly string Name;
            public readonly int RollNo;

            #endregion

            #region Constructors

            public MyTest(string name, int number)
            {
                Name = name;
                RollNo = number;
            }

            #endregion

            #region Methods

            public override int GetHashCode()
            {
                return Name.GetHashCode() ^ RollNo;
            }

            public override bool Equals(object obj)
            {
                var myt = obj as MyTest;
                return myt!.Name.Equals(Name) &&
                       myt!.RollNo.Equals(RollNo);
            }

            #endregion
        }

        #endregion
    }
}