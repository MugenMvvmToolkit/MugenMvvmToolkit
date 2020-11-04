﻿using MugenMvvm.UnitTests.Enums.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Enums
{
    public class FlagsEnumBaseTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void FlagsOperationsShouldBeValid()
        {
            var f1 = TestFlags.Flag1;
            var f2 = TestFlags.Flag2;

            f1.FlagValue.ShouldEqual(f1.Value);
            f2.FlagValue.ShouldEqual(f2.Value);
            (f1 | f2).Flags.ShouldEqual(f1.Value | f2.Value);
            (f1 & f2).Flags.ShouldEqual(f1.Value & f2.Value);
            (~f1).Flags.ShouldEqual(~f1.Value);
        }

        #endregion
    }
}