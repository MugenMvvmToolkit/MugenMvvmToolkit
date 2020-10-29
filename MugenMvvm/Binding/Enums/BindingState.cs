using MugenMvvm.Attributes;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    public class BindingState : EnumBase<BindingState, int>
    {
        #region Fields

        public static readonly BindingState Valid = new BindingState(1);
        public static readonly BindingState Disposed = new BindingState(2);
        public static readonly BindingState Invalid = new BindingState(3);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected BindingState()
        {
        }

        public BindingState(int value)
            : base(value)
        {
        }

        #endregion
    }
}