using MugenMvvm.Enums;

namespace MugenMvvm.Ios.Enums
{
    public sealed class IosViewLifecycleState : ViewLifecycleState
    {
        #region Fields

        public static readonly ViewLifecycleState DecodingRestorableState = new(nameof(DecodingRestorableState));
        public static readonly ViewLifecycleState DecodedRestorableState = new(nameof(DecodedRestorableState));
        public static readonly ViewLifecycleState EncodingRestorableState = new(nameof(EncodingRestorableState));
        public static readonly ViewLifecycleState EncodedRestorableState = new(nameof(EncodedRestorableState));
        public static readonly ViewLifecycleState DidLoading = new(nameof(DidLoading));
        public static readonly ViewLifecycleState DidLoaded = new(nameof(DidLoaded));
        public static readonly ViewLifecycleState WillAppearing = new(nameof(WillAppearing));
        public static readonly ViewLifecycleState WillAppeared = new(nameof(WillAppeared));
        public static readonly ViewLifecycleState WillDisappearing = new(nameof(WillDisappearing));
        public static readonly ViewLifecycleState WillDisappeared = new(nameof(WillDisappeared));
        public static readonly ViewLifecycleState DidMovingToParentViewController = new(nameof(DidMovingToParentViewController));
        public static readonly ViewLifecycleState DidMovedToParentViewController = new(nameof(DidMovedToParentViewController));
        public static readonly ViewLifecycleState WillMovingToParentViewController = new(nameof(WillMovingToParentViewController));
        public static readonly ViewLifecycleState WillMovedToParentViewController = new(nameof(WillMovedToParentViewController));
        public static readonly ViewLifecycleState RemovingFromParentViewController = new(nameof(RemovingFromParentViewController));
        public static readonly ViewLifecycleState RemovedFromParentViewController = new(nameof(RemovedFromParentViewController));

        #endregion

        #region Constructors

        private IosViewLifecycleState()
        {
        }

        #endregion
    }
}