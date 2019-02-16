using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Models;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class NavigationType : EnumBase<NavigationType, string>
    {
        #region Fields

        public static readonly NavigationType Undefined = new NavigationType(nameof(Undefined));
        public static readonly NavigationType Tab = new NavigationType(nameof(Tab));
        public static readonly NavigationType Generic = new NavigationType(nameof(Generic));
        public static readonly NavigationType Page = new NavigationType(nameof(Page));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected NavigationType()
        {
        }


        public NavigationType(string value) : base(value)
        {
        }

        #endregion
    }
}