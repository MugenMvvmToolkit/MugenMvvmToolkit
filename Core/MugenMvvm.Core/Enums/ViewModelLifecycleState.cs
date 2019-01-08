using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Models;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class ViewModelLifecycleState : EnumBase<ViewModelLifecycleState, int>
    {
        #region Fields

        public static readonly ViewModelLifecycleState Created = new ViewModelLifecycleState(1, nameof(Created));
        public static readonly ViewModelLifecycleState Disposed = new ViewModelLifecycleState(2, nameof(Disposed));
        public static readonly ViewModelLifecycleState Finalized = new ViewModelLifecycleState(3, nameof(Finalized));
        public static readonly ViewModelLifecycleState Restoring = new ViewModelLifecycleState(4, nameof(Restoring));
        public static readonly ViewModelLifecycleState Restored = new ViewModelLifecycleState(5, nameof(Restored));

        #endregion

        #region Constructors

        public ViewModelLifecycleState(int value, string displayName) : base(value, displayName)
        {
        }

        [Preserve(Conditional = true)]
        internal ViewModelLifecycleState()
        {
        }

        #endregion
    }
}