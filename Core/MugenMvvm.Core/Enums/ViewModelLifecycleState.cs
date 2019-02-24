using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Models;

namespace MugenMvvm.Enums
{
    [Serializable, DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class ViewModelLifecycleState : EnumBase<ViewModelLifecycleState, string>
    {
        #region Fields

        public static readonly ViewModelLifecycleState Created = new ViewModelLifecycleState(nameof(Created));
        public static readonly ViewModelLifecycleState Initialized = new ViewModelLifecycleState(nameof(Initialized));
        public static readonly ViewModelLifecycleState Disposing = new ViewModelLifecycleState(nameof(Disposing)) { IsDispose = true };
        public static readonly ViewModelLifecycleState Disposed = new ViewModelLifecycleState(nameof(Disposed)) { IsDispose = true };
        public static readonly ViewModelLifecycleState Finalized = new ViewModelLifecycleState(nameof(Finalized)) { IsDispose = true };
        public static readonly ViewModelLifecycleState Restoring = new ViewModelLifecycleState(nameof(Restoring)) { IsRestore = true };
        public static readonly ViewModelLifecycleState Restored = new ViewModelLifecycleState(nameof(Restored)) { IsRestore = true };

        #endregion

        #region Constructors

        public ViewModelLifecycleState(string value)
            : base(value)
        {
        }

        [Preserve(Conditional = true)]
        internal ViewModelLifecycleState()
        {
        }

        #endregion

        #region Properties

        [DataMember(Name = "d")]
        public bool IsDispose { get; set; }

        [DataMember(Name = "r")]
        public bool IsRestore { get; set; }

        #endregion
    }
}