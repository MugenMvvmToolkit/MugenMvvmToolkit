using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
#pragma warning disable 660,661
    public class PlatformType : EnumBase<PlatformType, string>
#pragma warning restore 660,661
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

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(PlatformType? left, PlatformType? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(PlatformType? left, PlatformType? right)
        {
            return !(left == right);
        }

        protected override bool Equals(string value)
        {
            return Value.Equals(value);
        }

        #endregion
    }
}