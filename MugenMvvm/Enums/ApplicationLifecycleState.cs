using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class ApplicationLifecycleState : EnumBase<ApplicationLifecycleState, string>
    {
        #region Fields

        public static readonly ApplicationLifecycleState Initializing = new(nameof(Initializing));
        public static readonly ApplicationLifecycleState Initialized = new(nameof(Initialized));
        public static readonly ApplicationLifecycleState Activating = new(nameof(Activating));
        public static readonly ApplicationLifecycleState Activated = new(nameof(Activated));
        public static readonly ApplicationLifecycleState Deactivating = new(nameof(Deactivating));
        public static readonly ApplicationLifecycleState Deactivated = new(nameof(Deactivated));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected ApplicationLifecycleState()
        {
        }

        public ApplicationLifecycleState(string value) : base(value)
        {
        }

        #endregion
    }
}