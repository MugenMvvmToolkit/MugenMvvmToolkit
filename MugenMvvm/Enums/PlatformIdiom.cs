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
    }
}