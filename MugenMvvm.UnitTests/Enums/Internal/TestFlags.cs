using MugenMvvm.Enums;

namespace MugenMvvm.UnitTests.Enums.Internal
{
    public sealed class TestFlags : FlagsEnumBase<TestFlags, int>
    {
        #region Fields

        public static readonly TestFlags Flag1 = new(1 << 1, nameof(Flag1));
        public static readonly TestFlags Flag2 = new(1 << 2, nameof(Flag2));

        #endregion

        #region Constructors

        public TestFlags(int value, string name) : base(value, name)
        {
        }

        #endregion
    }
}