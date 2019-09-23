using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class BindingLifecycleState : EnumBase<BindingLifecycleState, string>
    {
        #region Fields

        public static readonly BindingLifecycleState Created = new BindingLifecycleState(nameof(Created));
        public static readonly BindingLifecycleState Attached = new BindingLifecycleState(nameof(Attached));
        public static readonly BindingLifecycleState Disposed = new BindingLifecycleState(nameof(Disposed));

        #endregion

        #region Constructors

        public BindingLifecycleState(string value)
            : base(value)
        {
        }

        [Preserve(Conditional = true)]
        internal BindingLifecycleState()
        {
        }

        #endregion
    }
}