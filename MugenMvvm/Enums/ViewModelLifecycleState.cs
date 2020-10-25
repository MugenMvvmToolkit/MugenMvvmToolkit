using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class ViewModelLifecycleState : EnumBase<ViewModelLifecycleState, string>
    {
        #region Fields

        public static readonly ViewModelLifecycleState Created = new ViewModelLifecycleState(nameof(Created));
        public static readonly ViewModelLifecycleState Initializing = new ViewModelLifecycleState(nameof(Initializing));
        public static readonly ViewModelLifecycleState Initialized = new ViewModelLifecycleState(nameof(Initialized));
        public static readonly ViewModelLifecycleState Disposing = new ViewModelLifecycleState(nameof(Disposing));
        public static readonly ViewModelLifecycleState Disposed = new ViewModelLifecycleState(nameof(Disposed));
        public static readonly ViewModelLifecycleState Finalized = new ViewModelLifecycleState(nameof(Finalized));
        public static readonly ViewModelLifecycleState Preserving = new ViewModelLifecycleState(nameof(Preserving));
        public static readonly ViewModelLifecycleState Preserved = new ViewModelLifecycleState(nameof(Preserved));
        public static readonly ViewModelLifecycleState Restoring = new ViewModelLifecycleState(nameof(Restoring));
        public static readonly ViewModelLifecycleState Restored = new ViewModelLifecycleState(nameof(Restored));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected ViewModelLifecycleState()
        {
        }

        public ViewModelLifecycleState(string value)
            : base(value)
        {
        }

        #endregion
    }
}