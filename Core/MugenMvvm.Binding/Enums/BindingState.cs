using MugenMvvm.Attributes;
using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Enums
{
    public class BindingState : EnumBase<BindingState, int>
    {
        #region Fields

        public static readonly BindingState Attached = new BindingState(1);
        public static readonly BindingState Disposed = new BindingState(2);
        public static readonly BindingState Invalid = new BindingState(3);

        #endregion

        #region Constructors

        public BindingState(int value)
            : base(value)
        {
        }

        [Preserve(Conditional = true)]
        protected BindingState()
        {
        }

        #endregion
    }
}