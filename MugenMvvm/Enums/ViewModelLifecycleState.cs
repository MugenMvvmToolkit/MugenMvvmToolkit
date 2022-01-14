using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class ViewModelLifecycleState : EnumBase<ViewModelLifecycleState, string>
    {
        public static readonly ViewModelLifecycleState Created = new(nameof(Created));
        public static readonly ViewModelLifecycleState Initializing = new(nameof(Initializing));
        public static readonly ViewModelLifecycleState Initialized = new(nameof(Initialized));
        public static readonly ViewModelLifecycleState Disposing = new(nameof(Disposing));
        public static readonly ViewModelLifecycleState Disposed = new(nameof(Disposed));
        public static readonly ViewModelLifecycleState Finalized = new(nameof(Finalized));
        public static readonly ViewModelLifecycleState Preserving = new(nameof(Preserving));
        public static readonly ViewModelLifecycleState Preserved = new(nameof(Preserved));
        public static readonly ViewModelLifecycleState Restoring = new(nameof(Restoring));
        public static readonly ViewModelLifecycleState Restored = new(nameof(Restored));
        private ViewModelLifecycleState? _baseState;

        public ViewModelLifecycleState(string value, string? name = null, bool register = true) : base(value, name, register)
        {
        }

        [Preserve(Conditional = true)]
        protected ViewModelLifecycleState()
        {
        }

        public ViewModelLifecycleState BaseState
        {
            get => _baseState ?? this;
            set => _baseState = value;
        }
    }
}