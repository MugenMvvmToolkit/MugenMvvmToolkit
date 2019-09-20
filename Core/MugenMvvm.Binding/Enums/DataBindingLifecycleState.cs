using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class DataBindingLifecycleState : EnumBase<DataBindingLifecycleState, string>
    {
        #region Fields

        public static readonly DataBindingLifecycleState Created = new DataBindingLifecycleState(nameof(Created));
        public static readonly DataBindingLifecycleState Attached = new DataBindingLifecycleState(nameof(Attached));
        public static readonly DataBindingLifecycleState Disposed = new DataBindingLifecycleState(nameof(Disposed));

        #endregion

        #region Constructors

        public DataBindingLifecycleState(string value)
            : base(value)
        {
        }

        [Preserve(Conditional = true)]
        internal DataBindingLifecycleState()
        {
        }

        #endregion
    }
}