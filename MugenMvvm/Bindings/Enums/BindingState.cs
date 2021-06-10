using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class BindingState : EnumBase<BindingState, int>
    {
        public static readonly BindingState Valid = new(1);
        public static readonly BindingState Disposed = new(2);
        public static readonly BindingState Invalid = new(3);

        public BindingState(int value, string? name = null) : base(value, name)
        {
        }

        [Preserve(Conditional = true)]
        protected BindingState()
        {
        }
    }
}