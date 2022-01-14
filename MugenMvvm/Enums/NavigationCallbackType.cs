using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class NavigationCallbackType : EnumBase<NavigationCallbackType, string>
    {
        public static readonly NavigationCallbackType Show = new(nameof(Show));
        public static readonly NavigationCallbackType Closing = new(nameof(Closing));
        public static readonly NavigationCallbackType Close = new(nameof(Close));

        public NavigationCallbackType(string value, string? name = null, bool register = true) : base(value, name, register)
        {
        }

        [Preserve(Conditional = true)]
        protected NavigationCallbackType()
        {
        }
    }
}