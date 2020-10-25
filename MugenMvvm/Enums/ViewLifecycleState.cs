using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class ViewLifecycleState : EnumBase<ViewLifecycleState, string>
    {
        #region Fields

        public static readonly ViewLifecycleState Initializing = new ViewLifecycleState(nameof(Initializing));
        public static readonly ViewLifecycleState Initialized = new ViewLifecycleState(nameof(Initialized));
        public static readonly ViewLifecycleState Appearing = new ViewLifecycleState(nameof(Appearing));
        public static readonly ViewLifecycleState Appeared = new ViewLifecycleState(nameof(Appeared));
        public static readonly ViewLifecycleState Disappearing = new ViewLifecycleState(nameof(Disappearing));
        public static readonly ViewLifecycleState Disappeared = new ViewLifecycleState(nameof(Disappeared));
        public static readonly ViewLifecycleState Clearing = new ViewLifecycleState(nameof(Clearing));
        public static readonly ViewLifecycleState Cleared = new ViewLifecycleState(nameof(Cleared));
        public static readonly ViewLifecycleState Closing = new ViewLifecycleState(nameof(Closing));
        public static readonly ViewLifecycleState Closed = new ViewLifecycleState(nameof(Closed));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected ViewLifecycleState()
        {
        }

        public ViewLifecycleState(string value)
            : base(value)
        {
        }

        #endregion
    }
}