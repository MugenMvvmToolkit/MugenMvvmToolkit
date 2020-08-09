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
    public class PlatformIdiom : EnumBase<PlatformIdiom, string>
#pragma warning restore 660,661
    {
        #region Fields

        public static readonly PlatformIdiom Desktop = new PlatformIdiom(nameof(Desktop));
        public static readonly PlatformIdiom Tablet = new PlatformIdiom(nameof(Tablet));
        public static readonly PlatformIdiom Phone = new PlatformIdiom(nameof(Phone));
        public static readonly PlatformIdiom TV = new PlatformIdiom(nameof(TV));
        public static readonly PlatformIdiom Watch = new PlatformIdiom(nameof(Watch));
        public static readonly PlatformIdiom Unknown = new PlatformIdiom(nameof(Unknown));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected PlatformIdiom()
        {
        }

        public PlatformIdiom(string id) : base(id)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(PlatformIdiom? left, PlatformIdiom? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(PlatformIdiom? left, PlatformIdiom? right) => !(left == right);

        protected override bool Equals(string value) => Value.Equals(value);

        #endregion
    }
}