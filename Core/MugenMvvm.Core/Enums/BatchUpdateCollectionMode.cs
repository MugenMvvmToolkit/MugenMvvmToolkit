using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MugenMvvm.Attributes;

namespace MugenMvvm.Enums
{
    public class BatchUpdateCollectionMode : EnumBase<BatchUpdateCollectionMode, int>
    {
        #region Fields

        public static readonly BatchUpdateCollectionMode Listeners = new BatchUpdateCollectionMode(1);
        public static readonly BatchUpdateCollectionMode DecoratorListeners = new BatchUpdateCollectionMode(1 << 1);
        public static readonly BatchUpdateCollectionMode Both = Listeners | DecoratorListeners;

        #endregion

        #region Constructors

        static BatchUpdateCollectionMode()
        {
            SetIsFlagEnum(i => new BatchUpdateCollectionMode(i));
        }

        [Preserve(Conditional = true)]
        protected BatchUpdateCollectionMode()
        {
        }

        public BatchUpdateCollectionMode(int value) : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BatchUpdateCollectionMode? left, BatchUpdateCollectionMode? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BatchUpdateCollectionMode? left, BatchUpdateCollectionMode? right)
        {
            return !(left == right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BatchUpdateCollectionMode operator |(BatchUpdateCollectionMode left, BatchUpdateCollectionMode right)
        {
            Should.NotBeNull(left, nameof(left));
            Should.NotBeNull(right, nameof(right));
            return Parse(left.Value | right.Value);
        }

        [Pure]
        public bool HasFlag(BatchUpdateCollectionMode flag)
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