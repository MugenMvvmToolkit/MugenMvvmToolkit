using System.Runtime.CompilerServices;
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

        [Preserve(Conditional = true)]
        protected BindingState()
        {
        }

        public BindingState(int value)
            : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BindingState? left, BindingState? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BindingState? left, BindingState? right)
        {
            return !(left == right);
        }

        protected override bool Equals(int value)
        {
            return Value == value;
        }

        #endregion
    }
}