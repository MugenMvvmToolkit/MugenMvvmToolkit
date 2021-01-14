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
        public static readonly NavigationCallbackType Showing = new(1);
        public static readonly NavigationCallbackType Closing = new(2);
        public static readonly NavigationCallbackType Close = new(3);

        public NavigationCallbackType(int value, string? name = null) : base(value, name)
        {
        }

        [Preserve(Conditional = true)]
        protected NavigationCallbackType()
        {
        }
    }
}