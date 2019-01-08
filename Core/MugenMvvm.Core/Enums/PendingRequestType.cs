using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Models;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class NavigationCallbackType : EnumBase<NavigationCallbackType, int>
    {
        #region Fields

        public static readonly NavigationCallbackType Showing = new NavigationCallbackType(0, nameof(Showing));
        public static readonly NavigationCallbackType Closing = new NavigationCallbackType(1, nameof(Closing));
        public static readonly NavigationCallbackType Close = new NavigationCallbackType(2, nameof(Close));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        internal NavigationCallbackType()
        {
        }

        public NavigationCallbackType(int value, string displayName) : base(value, displayName)
        {
        }

        #endregion
    }
}