using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Enums.Internal
{
    public class EnumFlagsTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void FlagsOperationsShouldBeValid()
        {
            EnumFlags<TestFlags> f1 = TestFlags.Flag1;
            EnumFlags<TestFlags> f2 = TestFlags.Flag2;

            f1.Flags.ShouldEqual(TestFlags.Flag1.Flag);
            f2.Flags.ShouldEqual(TestFlags.Flag2.Flag);
            (f1 >= f2).ShouldEqual(f1.Flags >= f2.Flags);
            (f1 <= f2).ShouldEqual(f1.Flags <= f2.Flags);
            (f1 > f2).ShouldEqual(f1.Flags > f2.Flags);
            (f1 < f2).ShouldEqual(f1.Flags < f2.Flags);
            (f1 == f2).ShouldEqual(f1.Flags == f2.Flags);
            (f1 != f2).ShouldEqual(f1.Flags != f2.Flags);
            (f1 | f2).Flags.ShouldEqual(f1.Flags | f2.Flags);
            (f1 & f2).Flags.ShouldEqual(f1.Flags & f2.Flags);
            (~f1).Flags.ShouldEqual(~f1.Flags);
            f1.HasFlag(TestFlags.Flag1).ShouldBeTrue();
            f1.HasFlag(TestFlags.Flag2).ShouldBeFalse();
            (f1 | f2).HasFlag(TestFlags.Flag1).ShouldBeTrue();
            (f1 | f2).HasFlag(TestFlags.Flag2).ShouldBeTrue();

            f1.HasFlag(TestFlags.Flag1.Flag).ShouldBeTrue();
            f1.HasFlag(TestFlags.Flag2.Flag).ShouldBeFalse();
            (f1 | f2).HasFlag(TestFlags.Flag1.Flag).ShouldBeTrue();
            (f1 | f2).HasFlag(TestFlags.Flag2.Flag).ShouldBeTrue();

            f1.Equals(f1).ShouldBeTrue();
            f1.Equals(f2).ShouldBeFalse();
            f1.GetHashCode().ShouldEqual(f1.Flags.GetHashCode());
            f1.ToString().ShouldEqual(TestFlags.Flag1.Name);
            (f1 | f2).ToString().ShouldEqual($"{TestFlags.Flag1.Name} | {TestFlags.Flag2.Name}");
        }

        [Fact]
        public void ExtensionOperationsShouldBeValid()
        {
            EnumFlags<TestFlags> f1 = TestFlags.Flag1;
            EnumFlags<TestFlags> f2 = TestFlags.Flag2;

            f1.Value().ShouldEqual(TestFlags.Flag1.Value);
            f2.Value().ShouldEqual(TestFlags.Flag2.Value);

            var flags = f1.GetFlags();
            flags.Count.ShouldEqual(1);
            flags.Item.ShouldEqual(TestFlags.Flag1);

            flags = f2.GetFlags();
            flags.Count.ShouldEqual(1);
            flags.Item.ShouldEqual(TestFlags.Flag2);

            flags = (f1 | f2).GetFlags();
            flags.Count.ShouldEqual(2);
            flags.List!.ShouldContain(TestFlags.Flag1);
            flags.List!.ShouldContain(TestFlags.Flag2);
        }

#if SPAN_API
        [Fact]
        public void ExtensionOperationsShouldBeValidSpan()
        {
            EnumFlags<TestFlags> f1 = TestFlags.Flag1;
            EnumFlags<TestFlags> f2 = TestFlags.Flag2;

            var span = new TestFlags[2].AsSpan();

            f1.GetFlags(span).ShouldEqual(1);
            span.Slice(0, 1).ToArray().ShouldContain(TestFlags.Flag1);

            f2.GetFlags(span).ShouldEqual(1);
            span.Slice(0, 1).ToArray().ShouldContain(TestFlags.Flag2);

            (f1 | f2).GetFlags(span).ShouldEqual(2);
            var flags = span.Slice(0, 2).ToArray();
            flags.Length.ShouldEqual(2);
            flags.ShouldContain(TestFlags.Flag1);
            flags.ShouldContain(TestFlags.Flag2);
        }
#endif

        #endregion
    }
}