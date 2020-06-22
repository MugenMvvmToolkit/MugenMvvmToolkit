using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
#pragma warning disable 660,661
    public class EntityState : EnumBase<EntityState, int>
#pragma warning restore 660,661
    {
        #region Fields

        public static readonly EntityState Unchanged = new EntityState(1);
        public static readonly EntityState Added = new EntityState(2);
        public static readonly EntityState Deleted = new EntityState(3);
        public static readonly EntityState Modified = new EntityState(4);
        public static readonly EntityState Detached = new EntityState(5);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected EntityState()
        {
        }

        public EntityState(int value) : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(EntityState? left, EntityState? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(EntityState? left, EntityState? right)
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