using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Enums
{
    public class BinaryTokenType : EnumBase<BinaryTokenType, string>
    {
        #region Constructors

        protected BinaryTokenType()
        {
        }

        public BinaryTokenType(string value, int priority)
            : base(value)
        {
            Priority = priority;
        }

        #endregion

        #region Properties

        public int Priority { get; }

        #endregion
    }
}