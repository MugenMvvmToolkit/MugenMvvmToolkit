#pragma warning disable CS1718
using MugenMvvm.Enums;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Enums
{
    public class UnhandledExceptionTypeTest
    {
        #region Methods

        [Fact]
        public void CompareToEqualsShouldBeValid()
        {
            var enum1 = UnhandledExceptionType.System;
            var enum2 = UnhandledExceptionType.Binding;
            var enum3 = new UnhandledExceptionType(enum1.Value);
            var v1 = enum1.Value;
            var v2 = enum2.Value;
            enum1.CompareTo(null).ShouldEqual(1);
            enum1.CompareTo(enum2).ShouldEqual(v1.CompareTo(v2));
            enum2.CompareTo(enum1).ShouldEqual(v2.CompareTo(v1));

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

        #endregion
    }
}