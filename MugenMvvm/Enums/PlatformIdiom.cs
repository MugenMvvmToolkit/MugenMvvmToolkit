using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class PlatformIdiom : EnumBase<PlatformIdiom, string>
    {
        #region Fields

        public static readonly PlatformIdiom Desktop = new(nameof(Desktop));
        public static readonly PlatformIdiom Tablet = new(nameof(Tablet));
        public static readonly PlatformIdiom Phone = new(nameof(Phone));
        public static readonly PlatformIdiom TV = new(nameof(TV));
        public static readonly PlatformIdiom Watch = new(nameof(Watch));
        public static readonly PlatformIdiom Unknown = new(nameof(Unknown));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected PlatformIdiom()
        {
        }

        public PlatformIdiom(string id, string? name = null) : base(id, name)
        {
        }

        #endregion
    }
}