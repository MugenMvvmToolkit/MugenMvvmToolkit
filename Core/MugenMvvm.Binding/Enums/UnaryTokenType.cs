using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Enums
{
    public class UnaryTokenType : EnumBase<BinaryTokenType, string>
    {
        #region Constructors

        protected UnaryTokenType()
        {
        }

        public UnaryTokenType(string value)
            : base(value)
        {
        }

        #endregion
    }
}