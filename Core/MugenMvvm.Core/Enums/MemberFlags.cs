using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MugenMvvm.Attributes;

namespace MugenMvvm.Enums
{
    public class MemberFlags : EnumBase<MemberFlags, int>
    {
        #region Fields

        public static readonly MemberFlags Static = new MemberFlags(1);
        public static readonly MemberFlags Instance = new MemberFlags(1 << 1);
        public static readonly MemberFlags Public = new MemberFlags(1 << 2);
        public static readonly MemberFlags NonPublic = new MemberFlags(1 << 3);

        public static readonly MemberFlags All = Static | Instance | Public | NonPublic;
        public static readonly MemberFlags InstancePublic = Instance | Public;
        public static readonly MemberFlags StaticPublic = Static | Public;
        public static readonly MemberFlags StaticOnly = StaticPublic | NonPublic;
        public static readonly MemberFlags InstanceOnly = InstancePublic | NonPublic;

        #endregion

        #region Constructors

        static MemberFlags()
        {
            SetIsFlagEnum(i => new MemberFlags(i));
        }
        
        [Preserve(Conditional = true)]
        protected MemberFlags()
        {
        }

        public MemberFlags(int value) : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(MemberFlags? left, MemberFlags? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(MemberFlags? left, MemberFlags? right)
        {
            return !(left == right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberFlags operator |(MemberFlags left, MemberFlags right)
        {
            Should.NotBeNull(left, nameof(left));
            Should.NotBeNull(right, nameof(right));
            return Parse(left.Value | right.Value);
        }

        [Pure]
        public bool HasFlag(MemberFlags flag)
        {
            return (Value & flag.Value) == flag.Value;
        }

        protected override bool Equals(int value)
        {
            return Value == value;
        }

        #endregion
    }
}