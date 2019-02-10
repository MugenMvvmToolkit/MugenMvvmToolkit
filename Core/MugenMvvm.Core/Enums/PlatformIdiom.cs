using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Models;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class PlatformIdiom : EnumBase<PlatformIdiom, string>
    {
        #region Fields

        public static readonly PlatformIdiom Desktop = new PlatformIdiom(nameof(Desktop));
        public static readonly PlatformIdiom Tablet = new PlatformIdiom(nameof(Tablet));
        public static readonly PlatformIdiom Phone = new PlatformIdiom(nameof(Phone));
        public static readonly PlatformIdiom Car = new PlatformIdiom(nameof(Car));
        public static readonly PlatformIdiom TV = new PlatformIdiom(nameof(TV));
        public static readonly PlatformIdiom Unknown = new PlatformIdiom(nameof(Unknown));

        #endregion

        #region Constructors

        public PlatformIdiom(string id) : base(id)
        {
        }

        [Preserve(Conditional = true)]
        protected PlatformIdiom()
        {
        }

        #endregion
    }
}