using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class MemberType : FlagsEnumBase<MemberType, ushort>
    {
        #region Fields

        public static readonly MemberType Accessor = new(1 << 0, nameof(Accessor));
        public static readonly MemberType Method = new(1 << 1, nameof(Method));
        public static readonly MemberType Event = new(1 << 2, nameof(Event));
        public static readonly EnumFlags<MemberType> All = Accessor | Method | Event;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected MemberType()
        {
        }

        public MemberType(ushort value, string? name = null, long? flag = null) : base(value, name, flag)
        {
        }

        #endregion
    }
}