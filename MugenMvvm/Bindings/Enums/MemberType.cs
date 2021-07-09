using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class MemberType : FlagsEnumBase<MemberType, ushort>
    {
        public static readonly MemberType Accessor = new(1 << 0, nameof(Accessor));
        public static readonly MemberType Method = new(1 << 1, nameof(Method));
        public static readonly MemberType Event = new(1 << 2, nameof(Event));

        public MemberType(ushort value, string? name = null, long? flag = null) : base(value, name, flag)
        {
        }

        [Preserve(Conditional = true)]
        protected MemberType()
        {
        }

        public static EnumFlags<MemberType> All
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetAllFlags();
        }
    }
}