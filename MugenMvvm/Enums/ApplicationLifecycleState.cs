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

        public static readonly ApplicationLifecycleState Initializing = new ApplicationLifecycleState(nameof(Initializing));
        public static readonly ApplicationLifecycleState Initialized = new ApplicationLifecycleState(nameof(Initialized));
        public static readonly ApplicationLifecycleState Activating = new ApplicationLifecycleState(nameof(Activating));
        public static readonly ApplicationLifecycleState Activated = new ApplicationLifecycleState(nameof(Activated));
        public static readonly ApplicationLifecycleState Deactivating = new ApplicationLifecycleState(nameof(Deactivating));
        public static readonly ApplicationLifecycleState Deactivated = new ApplicationLifecycleState(nameof(Deactivated));

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