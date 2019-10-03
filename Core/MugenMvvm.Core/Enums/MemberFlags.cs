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
        public static readonly MemberFlags Attached = new MemberFlags(1 << 4);

        public static readonly MemberFlags All = null;//Static | Instance | Public | NonPublic | Attached;
        public static readonly MemberFlags InstancePublic = null;//Instance | Public;
        public static readonly MemberFlags StaticPublic = null;//Static | Public;
        public static readonly MemberFlags StaticOnly = null;//StaticPublic | NonPublic;
        public static readonly MemberFlags InstanceOnly = null;//InstancePublic | NonPublic;

        #endregion

        #region Constructors

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