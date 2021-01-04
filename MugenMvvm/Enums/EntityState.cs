using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class EntityState : FlagsEnumBase<EntityState, int>
    {
        #region Fields

        public static readonly EntityState Unchanged = new(1 << 0, nameof(Unchanged));
        public static readonly EntityState Added = new(1 << 1, nameof(Added));
        public static readonly EntityState Deleted = new(1 << 2, nameof(Deleted));
        public static readonly EntityState Modified = new(1 << 3, nameof(Modified));
        public static readonly EntityState Detached = new(1 << 4, nameof(Detached));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected EntityState()
        {
        }

        public EntityState(int value, string? name = null, long? flag = null) : base(value, name, flag)
        {
        }

        #endregion
    }
}