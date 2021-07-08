using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class NavigationCallbackType : EnumBase<NavigationCallbackType, int>
    {
        public static readonly NavigationCallbackType Showing = new(1, nameof(Showing));
        public static readonly NavigationCallbackType Closing = new(2, nameof(Closing));
        public static readonly NavigationCallbackType Close = new(3, nameof(Close));

        public NavigationCallbackType(int value, string? name = null) : base(value, name)
        {
        }

        [Preserve(Conditional = true)]
        protected NavigationCallbackType()
        {
        }
    }
}