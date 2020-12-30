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