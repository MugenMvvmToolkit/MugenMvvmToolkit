using MugenMvvm.Enums;

namespace MugenMvvm.UnitTests.Enums.Internal
{
    public sealed class TestFlags : FlagsEnumBase<TestFlags, int>
    {
        #region Fields

        public static readonly TestFlags Flag1 = new TestFlags(1 << 1);
        public static readonly TestFlags Flag2 = new TestFlags(1 << 2);

        #endregion

        #region Constructors

        public TestFlags(int value) : base(value)
        {
        }

        #endregion
    }
}