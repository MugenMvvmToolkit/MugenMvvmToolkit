using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class MemberFlags : FlagsEnumBase<MemberFlags, ushort>, IHasPriority
    {
        private const int AttachedPriority = 10000;
        private const int DefaultPriority = 1000;
        private const int ExtensionPriority = -1000;
        private const int DynamicPriority = -10000;

        public static readonly MemberFlags Public = new(1 << 0, 10, nameof(Public));
        public static readonly MemberFlags NonPublic = new(1 << 1, 0, nameof(NonPublic));
        public static readonly MemberFlags NonObservable = new(1 << 7, 0, nameof(NonObservable));

        public static readonly MemberFlags Static = new(1 << 2, DefaultPriority, nameof(Static));
        public static readonly MemberFlags Instance = new(1 << 3, DefaultPriority, nameof(Instance));
        public static readonly MemberFlags Attached = new(1 << 4, AttachedPriority, nameof(Attached));
        public static readonly MemberFlags Dynamic = new(1 << 5, DynamicPriority, nameof(Dynamic));
        public static readonly MemberFlags Extension = new((ushort) (Instance.Value | (1 << 6)), ExtensionPriority, nameof(Extension));

        public static readonly EnumFlags<MemberFlags> InstancePublic = Instance | Public;
        public static readonly EnumFlags<MemberFlags> InstanceNonPublic = Instance | NonPublic;

        public static readonly EnumFlags<MemberFlags> StaticPublic = Static | Public;
        public static readonly EnumFlags<MemberFlags> StaticNonPublic = Static | NonPublic;

        public MemberFlags(ushort value, int priority, string? name = null, long? flag = null, bool register = true) : base(value, name, flag, register)
        {
            Priority = priority;
        }

        [Preserve(Conditional = true)]
        protected MemberFlags()
        {
        }

        public static EnumFlags<MemberFlags> All
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetAllFlags();
        }

        public static EnumFlags<MemberFlags> InstancePublicAll
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => All & ~(Static | NonPublic);
        }

        public static EnumFlags<MemberFlags> StaticPublicAll
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => All & ~(Instance | NonPublic);
        }

        public static EnumFlags<MemberFlags> InstanceAll
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => All & ~Static;
        }

        public static EnumFlags<MemberFlags> StaticAll
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => All & ~Instance;
        }

        public int Priority { get; init; }
    }
}