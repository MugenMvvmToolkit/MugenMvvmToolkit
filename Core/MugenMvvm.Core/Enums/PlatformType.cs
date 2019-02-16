using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Models;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
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

        public PlatformType(string id)
            : base(id)
        {
        }

        [Preserve(Conditional = true)]
        protected PlatformType()
        {
        }

        #endregion

        #region Properties

        [DataMember(Name = "x")]
        public bool IsXamForms { get; private set; }

        #endregion

        #region Methods

        public PlatformType ToXamForms()
        {
            return new PlatformType(Value) {IsXamForms = true};
        }

        #endregion
    }
}