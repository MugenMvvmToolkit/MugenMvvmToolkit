using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class NavigationCallbackType : EnumBase<NavigationCallbackType, int>
    {
        #region Fields

        public static readonly NavigationCallbackType Showing = new(1);
        public static readonly NavigationCallbackType Closing = new(2);
        public static readonly NavigationCallbackType Close = new(3);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected NavigationCallbackType()
        {
        }

        public NavigationCallbackType(int value) : base(value)
        {
        }

        #endregion
    }
}