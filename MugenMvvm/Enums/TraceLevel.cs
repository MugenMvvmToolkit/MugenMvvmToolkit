using MugenMvvm.Attributes;

namespace MugenMvvm.Enums
{
    public class TraceLevel : EnumBase<TraceLevel, string>
    {
        #region Fields

        public static readonly TraceLevel Information = new TraceLevel(nameof(Information));
        public static readonly TraceLevel Warning = new TraceLevel(nameof(Warning));
        public static readonly TraceLevel Error = new TraceLevel(nameof(Error));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected TraceLevel()
        {
        }

        public TraceLevel(string value) : base(value)
        {
        }

        #endregion
    }
}