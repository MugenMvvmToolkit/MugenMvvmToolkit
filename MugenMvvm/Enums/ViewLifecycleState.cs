﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
#pragma warning disable 660,661
    public class ViewLifecycleState : EnumBase<ViewLifecycleState, string>
#pragma warning restore 660,661
    {
        #region Fields

        public static readonly ViewLifecycleState Initializing = new ViewLifecycleState(nameof(Initializing));
        public static readonly ViewLifecycleState Initialized = new ViewLifecycleState(nameof(Initialized));
        public static readonly ViewLifecycleState Appearing = new ViewLifecycleState(nameof(Appearing));
        public static readonly ViewLifecycleState Appeared = new ViewLifecycleState(nameof(Appeared));
        public static readonly ViewLifecycleState Disappearing = new ViewLifecycleState(nameof(Disappearing));
        public static readonly ViewLifecycleState Disappeared = new ViewLifecycleState(nameof(Disappeared));
        public static readonly ViewLifecycleState Preserving = new ViewLifecycleState(nameof(Preserving));
        public static readonly ViewLifecycleState Preserved = new ViewLifecycleState(nameof(Preserved));
        public static readonly ViewLifecycleState Restoring = new ViewLifecycleState(nameof(Restoring));
        public static readonly ViewLifecycleState Restored = new ViewLifecycleState(nameof(Restored));
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

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ViewLifecycleState? left, ViewLifecycleState? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ViewLifecycleState? left, ViewLifecycleState? right) => !(left == right);

        protected override bool Equals(string value) => Value.Equals(value);

        #endregion
    }
}