using MugenMvvm.Attributes;
using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Enums
{
    public class DataBindingState : EnumBase<DataBindingState, int>
    {
        #region Fields

        public static readonly DataBindingState Detached = new DataBindingState(1);
        public static readonly DataBindingState Attached = new DataBindingState(2);
        public static readonly DataBindingState Disposed = new DataBindingState(3);

        #endregion

        #region Constructors

        public DataBindingState(int value)
            : base(value)
        {
        }

        [Preserve(Conditional = true)]
        protected DataBindingState()
        {
        }

        #endregion
    }
}