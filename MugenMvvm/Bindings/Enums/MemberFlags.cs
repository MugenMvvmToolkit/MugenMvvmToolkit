﻿using System;
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

        public static readonly MemberFlags Public = new MemberFlags(1 << 0, 10);
        public static readonly MemberFlags NonPublic = new MemberFlags(1 << 1, 0);

        public static readonly MemberFlags Static = new MemberFlags(1 << 2, DefaultPriority);
        public static readonly MemberFlags Instance = new MemberFlags(1 << 3, DefaultPriority);
        public static readonly MemberFlags Attached = new MemberFlags(1 << 4, AttachedPriority);
        public static readonly MemberFlags Dynamic = new MemberFlags(1 << 5, DynamicPriority);
        public static readonly MemberFlags Extension = new MemberFlags((ushort) (Instance.Value | 1 << 6), ExtensionPriority);

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

        public MemberFlags(ushort value, int priority) : base(value)
        {
            Priority = priority;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion
    }
}