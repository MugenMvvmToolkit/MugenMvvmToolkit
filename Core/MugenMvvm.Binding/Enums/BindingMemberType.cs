using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Enums
{
    public class BindingMemberType : EnumBase<BindingMemberType, int>
    {
        #region Fields

        public static readonly BindingMemberType Field = new BindingMemberType(1);
        public static readonly BindingMemberType Property = new BindingMemberType(1 << 1);
        public static readonly BindingMemberType Method = new BindingMemberType(1 << 2);
        public static readonly BindingMemberType Event = new BindingMemberType(1 << 3);

        public static readonly BindingMemberType All = Field | Property | Method | Event;

        #endregion

        #region Constructors

        static BindingMemberType()
        {
            SetIsFlagEnum(i => new BindingMemberType(i));
        }

        [Preserve(Conditional = true)]
        protected BindingMemberType()
        {
        }

        public BindingMemberType(int value)
            : base(value)
        {
        }

        #endregion

        #region Methods

        public bool HasFlag(BindingMemberType flag)
        {
            return (Value & flag.Value) == flag.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BindingMemberType? left, BindingMemberType? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BindingMemberType? left, BindingMemberType? right)
        {
            return !(left == right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindingMemberType operator |(BindingMemberType left, BindingMemberType right)
        {
            Should.NotBeNull(left, nameof(left));
            Should.NotBeNull(right, nameof(right));
            return Parse(left.Value | right.Value);
        }

        protected override bool Equals(int value)
        {
            return Value == value;
        }

        #endregion
    }
}