using System;
using System.Runtime.Serialization;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class MemberType : FlagsEnumBase<MemberType, ushort>
    {
        #region Fields

        public static readonly MemberType Accessor = new MemberType(1 << 0);
        public static readonly MemberType Method = new MemberType(1 << 1);
        public static readonly MemberType Event = new MemberType(1 << 2);
        public static readonly EnumFlags<MemberType> All = Accessor | Method | Event;

        #endregion

        #region Constructors

        public MemberType(ushort value) : base(value)
        {
        }

        #endregion
    }
}