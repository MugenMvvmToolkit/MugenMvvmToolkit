using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class PlatformType : EnumBase<PlatformType, string>
    {
        #region Fields

        public static readonly PlatformType Android = new PlatformType(nameof(Android));
        public static readonly PlatformType iOS = new PlatformType(nameof(iOS));
        public static readonly PlatformType WinForms = new PlatformType(nameof(WinForms));
        public static readonly PlatformType UWP = new PlatformType(nameof(UWP));
        public static readonly PlatformType WPF = new PlatformType(nameof(WPF));
        public static readonly PlatformType WinPhone = new PlatformType(nameof(WinPhone));
        public static readonly PlatformType UnitTest = new PlatformType(nameof(UnitTest));
        public static readonly PlatformType Unknown = new PlatformType(nameof(Unknown));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected PlatformType()
        {
        }

        public PlatformType(string id)
            : base(id)
        {
        }

        #endregion
    }
}