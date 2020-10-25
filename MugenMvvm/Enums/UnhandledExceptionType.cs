using MugenMvvm.Attributes;

namespace MugenMvvm.Enums
{
    public class UnhandledExceptionType : EnumBase<UnhandledExceptionType, int>
    {
        #region Fields

        public static readonly UnhandledExceptionType Binding = new UnhandledExceptionType(1);
        public static readonly UnhandledExceptionType System = new UnhandledExceptionType(2);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected UnhandledExceptionType()
        {
        }

        public UnhandledExceptionType(int value) : base(value)
        {
        }

        #endregion
    }
}