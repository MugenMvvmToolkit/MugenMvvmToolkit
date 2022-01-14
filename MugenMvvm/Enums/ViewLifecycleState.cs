using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class ViewLifecycleState : EnumBase<ViewLifecycleState, string>
    {
        public static readonly ViewLifecycleState Initializing = new(nameof(Initializing));
        public static readonly ViewLifecycleState Initialized = new(nameof(Initialized));
        public static readonly ViewLifecycleState Appearing = new(nameof(Appearing));
        public static readonly ViewLifecycleState Appeared = new(nameof(Appeared));
        public static readonly ViewLifecycleState Disappearing = new(nameof(Disappearing));
        public static readonly ViewLifecycleState Disappeared = new(nameof(Disappeared));
        public static readonly ViewLifecycleState Clearing = new(nameof(Clearing));
        public static readonly ViewLifecycleState Cleared = new(nameof(Cleared));
        public static readonly ViewLifecycleState Closing = new(nameof(Closing));
        public static readonly ViewLifecycleState Closed = new(nameof(Closed));
        private NavigationMode? _navigationMode;
        private ViewLifecycleState? _baseState;

        public ViewLifecycleState(string value, string? name = null, bool register = true) : base(value, name, register)
        {
        }

        [Preserve(Conditional = true)]
        protected ViewLifecycleState()
        {
        }

        public ViewLifecycleState BaseState
        {
            get => _baseState ?? this;
            set => _baseState = value;
        }

        public NavigationMode? NavigationMode
        {
            get => _navigationMode ?? _baseState?.NavigationMode;
            set => _navigationMode = value;
        }
    }
}