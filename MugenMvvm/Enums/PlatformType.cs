using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class PlatformType : EnumBase<PlatformType, string>
    {
        public static readonly PlatformType Android = new(nameof(Android));
        public static readonly PlatformType iOS = new(nameof(iOS));
        public static readonly PlatformType WinForms = new(nameof(WinForms));
        public static readonly PlatformType UWP = new(nameof(UWP));
        public static readonly PlatformType WPF = new(nameof(WPF));
        public static readonly PlatformType WinPhone = new(nameof(WinPhone));
        public static readonly PlatformType Avalonia = new(nameof(Avalonia));
        public static readonly PlatformType UnitTest = new(nameof(UnitTest));

        public PlatformType(string id, string? name = null)
            : base(id, name)
        {
        }

        [Preserve(Conditional = true)]
        protected PlatformType()
        {
        }
    }
}