#pragma warning disable CS1718
using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Enums;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Enums
{
    public class EnumBaseTest : UnitTestBase
    {
        [Fact]
        public void CompareToEqualsShouldBeValid()
        {
            var v1 = 1;
            var v2 = 2;
            var enum1 = new TestEnum(v1, null);
            var enum2 = new TestEnum(v2, null);
            var enum3 = new TestEnum(v1, null);
            enum1.CompareTo(null).ShouldEqual(1);
            enum1.CompareTo(enum2).ShouldEqual(v1.CompareTo(v2));
            enum2.CompareTo(enum1).ShouldEqual(v2.CompareTo(v1));


            (enum1 >= enum2).ShouldEqual(v1 >= v2);
            (enum1 <= enum2).ShouldEqual(v1 <= v2);
            (enum1 > enum2).ShouldEqual(v1 > v2);
            (enum1 < enum2).ShouldEqual(v1 < v2);
            enum1.Equals(enum1).ShouldBeTrue();
            enum1.Equals(enum3).ShouldBeTrue();
            (enum1 == enum1).ShouldBeTrue();
            (enum1 == enum3).ShouldBeTrue();
            (enum1 != enum1).ShouldBeFalse();
            (enum1 != enum3).ShouldBeFalse();
            enum1.Equals(enum2).ShouldBeFalse();
            (enum1 == enum2).ShouldBeFalse();
            (enum1 != enum2).ShouldBeTrue();
            enum1.Equals(null).ShouldBeFalse();
            (enum1 == null).ShouldBeFalse();
            (enum1 != null).ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var v = 1;
            string name = "";
            var testEnum = new TestEnum(v, name);
            testEnum.Value.ShouldEqual(v);
            testEnum.Name.ShouldEqual(name);
            testEnum.ToString().ShouldEqual(name);
            testEnum.GetHashCode().ShouldEqual(v.GetHashCode());
        }

        [Fact]
        public void ParseTryParseByNameShouldBeValid()
        {
            const string value = "Test1";
            var testEnum = new TestEnum(1, value.ToUpper());
            TestEnum.SetEnums(new Dictionary<int, TestEnum>
            {
                {testEnum.Value, testEnum}
            });

            TestEnum.TryGetByName(value.ToLower(), out var result, true).ShouldBeTrue();
            result.ShouldEqual(testEnum);
            EnumBase.TryGetByName<TestEnum>(value.ToLower(), null, true).ShouldEqual(testEnum);
            EnumBase.TryGetByName(typeof(TestEnum), value.ToLower(), null, true).ShouldEqual(testEnum);

            TestEnum.TryGetByName(value.ToLower(), out result).ShouldBeFalse();
            EnumBase.TryGetByName<TestEnum>(value.ToLower()).ShouldBeNull();
            EnumBase.TryGetByName(typeof(TestEnum), value.ToLower()).ShouldBeNull();

            TestEnum.SetEnums(new Dictionary<int, TestEnum>());
            TestEnum.TryGetByName(value.ToLower(), out result, true).ShouldBeFalse();
            ShouldThrow<ArgumentException>(() => TestEnum.GetByName(value.ToLower()));
            ShouldThrow<ArgumentException>(() =>
            {
                var lower = (TestEnum) value.ToLower()!;
            });
        }

        [Fact]
        public void ParseTryParseShouldUseCachedValues()
        {
            ViewLifecycleState.TryGet(null, out var v).ShouldBeFalse();
            TestEnum.SetEnums(new Dictionary<int, TestEnum>());
            var v1 = -1;
            var v2 = -2;
            var v3 = -3;
            ShouldThrow<ArgumentException>(() => TestEnum.Get(v1));
            ShouldThrow<ArgumentException>(() =>
            {
                var @enum = (TestEnum) v1;
            });
            TestEnum.TryGet(v1, out var r).ShouldBeFalse();
            EnumBase.TryGet<TestEnum, int>(v1).ShouldBeNull();
            EnumBase.TryGet(typeof(TestEnum), v1).ShouldBeNull();
            TestEnum.Count.ShouldEqual(0);
            TestEnum.GetAll().ShouldBeEmpty();
            EnumBase.GetAll<TestEnum>().ShouldBeEmpty();
            EnumBase.GetAll(typeof(TestEnum)).ShouldBeEmpty();

            var testEnum = new TestEnum(v1, v1.ToString());
            TestEnum.GetAll().Single().ShouldEqual(testEnum);
            EnumBase.GetAll<TestEnum>().Single().ShouldEqual(testEnum);
            EnumBase.GetAll(typeof(TestEnum)).Single().ShouldEqual(testEnum);
            TestEnum.Count.ShouldEqual(1);

            EnumBase.TryGet<TestEnum, int>(v1).ShouldEqual(testEnum);
            EnumBase.TryGet(typeof(TestEnum), v1).ShouldEqual(testEnum);
            TestEnum.Get(v1).ShouldEqual(testEnum);
            ((TestEnum) v1!).ShouldEqual(testEnum);
            TestEnum.TryGet(v1, out r).ShouldBeTrue();
            r.ShouldEqual(testEnum);

            var testEnum2 = new TestEnum(v2, v2.ToString());
            TestEnum.SetEnum(v3, testEnum2);
            TestEnum.Count.ShouldEqual(3);
            TestEnum.GetAll().Length.ShouldEqual(3);
            TestEnum.GetAll().ShouldContain(testEnum, testEnum2);

            EnumBase.GetAll<TestEnum>().Length.ShouldEqual(3);
            EnumBase.GetAll<TestEnum>().ShouldContain(testEnum, testEnum2);
            EnumBase.GetAll(typeof(TestEnum)).Length.ShouldEqual(3);
            EnumBase.GetAll(typeof(TestEnum)).ShouldContain(testEnum, testEnum2);

            EnumBase.TryGet<TestEnum, int>(v2).ShouldEqual(testEnum2);
            EnumBase.TryGet(typeof(TestEnum), v2).ShouldEqual(testEnum2);
            TestEnum.Get(v3).ShouldEqual(testEnum2);
            ((TestEnum) v3).ShouldEqual(testEnum2);
            TestEnum.TryGet(v3, out r).ShouldBeTrue();
            r.ShouldEqual(testEnum2);
        }

        private class TestEnum : EnumBase<TestEnum, int>
        {
            public TestEnum(int value, string? name) : base(value, name)
            {
            }
        }
    }
}