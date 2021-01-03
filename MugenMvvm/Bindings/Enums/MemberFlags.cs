using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class MemberFlags : FlagsEnumBase<MemberFlags, ushort>, IHasPriority
    {
        #region Fields

        private const int AttachedPriority = 1000000;
        private const int DefaultPriority = 100000;
        private const int ExtensionPriority = -1000;
        private const int DynamicPriority = -10000;

        public static readonly MemberFlags Public = new(1 << 0, nameof(Public), 10);
        public static readonly MemberFlags NonPublic = new(1 << 1, nameof(NonPublic), 0);

        public static readonly MemberFlags Static = new(1 << 2, nameof(Static), DefaultPriority);
        public static readonly MemberFlags Instance = new(1 << 3, nameof(Instance), DefaultPriority);
        public static readonly MemberFlags Attached = new(1 << 4, nameof(Attached), AttachedPriority);
        public static readonly MemberFlags Dynamic = new(1 << 5, nameof(Dynamic), DynamicPriority);
        public static readonly MemberFlags Extension = new((ushort) (Instance.Value | 1 << 6), nameof(Extension), ExtensionPriority);

        public static readonly EnumFlags<MemberFlags> InstancePublic = Instance | Public;
        public static readonly EnumFlags<MemberFlags> InstanceNonPublic = Instance | NonPublic;

        public static readonly EnumFlags<MemberFlags> StaticPublic = Static | Public;
        public static readonly EnumFlags<MemberFlags> StaticNonPublic = Static | NonPublic;

        public static readonly EnumFlags<MemberFlags> StaticOnly = StaticPublic | StaticNonPublic;
        public static readonly EnumFlags<MemberFlags> InstanceOnly = InstancePublic | InstanceNonPublic;

        public static readonly EnumFlags<MemberFlags> All = Static | Instance | Public | NonPublic | Attached | Dynamic | Extension;

        public static readonly EnumFlags<MemberFlags> InstancePublicAll = All & ~(Static | NonPublic);
        public static readonly EnumFlags<MemberFlags> StaticPublicAll = All & ~(Instance | NonPublic);
        public static readonly EnumFlags<MemberFlags> InstanceAll = All & ~Static;
        public static readonly EnumFlags<MemberFlags> StaticAll = All & ~Instance;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected MemberFlags()
        {
        }

        public MemberFlags(ushort value, string name, int priority) : base(value, name)
        {
            Priority = priority;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion
    }
}