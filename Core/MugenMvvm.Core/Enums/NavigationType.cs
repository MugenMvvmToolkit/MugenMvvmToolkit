using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Models;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class NavigationType : EnumBase<NavigationType, int>
    {
        #region Fields

        public static readonly NavigationType Undefined = new NavigationType(0, nameof(Undefined));
        public static readonly NavigationType Tab = new NavigationType(1, nameof(Tab));
        public static readonly NavigationType Window = new NavigationType(2, nameof(Window));
        public static readonly NavigationType Page = new NavigationType(3, nameof(Page));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        internal NavigationType()
        {
        }


        public NavigationType(int value, string displayName) : base(value, displayName)
        {
        }

        #endregion
    }
}