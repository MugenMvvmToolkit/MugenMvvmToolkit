using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class BindingLifecycleState : EnumBase<BindingLifecycleState, string>
    {
        public static readonly BindingLifecycleState Initialized = new(nameof(Initialized));
        public static readonly BindingLifecycleState Disposed = new(nameof(Disposed));

        public BindingLifecycleState(string value, string? name = null)
            : base(value, name)
        {
        }

        [Preserve(Conditional = true)]
        protected BindingLifecycleState()
        {
        }
    }
}