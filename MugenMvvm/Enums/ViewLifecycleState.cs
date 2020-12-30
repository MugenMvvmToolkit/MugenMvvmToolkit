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