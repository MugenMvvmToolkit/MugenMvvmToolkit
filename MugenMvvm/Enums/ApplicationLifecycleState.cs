using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class ApplicationLifecycleState : EnumBase<ApplicationLifecycleState, string>
    {
        public static readonly ApplicationLifecycleState Initializing = new(nameof(Initializing));
        public static readonly ApplicationLifecycleState Initialized = new(nameof(Initialized));
        public static readonly ApplicationLifecycleState Activating = new(nameof(Activating));
        public static readonly ApplicationLifecycleState Activated = new(nameof(Activated));
        public static readonly ApplicationLifecycleState Deactivating = new(nameof(Deactivating));
        public static readonly ApplicationLifecycleState Deactivated = new(nameof(Deactivated));
        private ApplicationLifecycleState? _baseState;

        public ApplicationLifecycleState(string value, string? name = null, bool register = true) : base(value, name, register)
        {
        }

        [Preserve(Conditional = true)]
        protected ApplicationLifecycleState()
        {
        }

        public ApplicationLifecycleState BaseState
        {
            get => _baseState ?? this;
            set => _baseState = value;
        }
    }
}