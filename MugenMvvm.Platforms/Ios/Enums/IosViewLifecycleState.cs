using MugenMvvm.Enums;

namespace MugenMvvm.Ios.Enums
{
    public abstract class IosViewLifecycleState : ViewLifecycleState
    {
        #region Fields

        public static readonly ViewLifecycleState DecodingRestorableState = new ViewLifecycleState(nameof(DecodingRestorableState));
        public static readonly ViewLifecycleState DecodedRestorableState = new ViewLifecycleState(nameof(DecodedRestorableState));
        public static readonly ViewLifecycleState EncodingRestorableState = new ViewLifecycleState(nameof(EncodingRestorableState));
        public static readonly ViewLifecycleState EncodedRestorableState = new ViewLifecycleState(nameof(EncodedRestorableState));
        public static readonly ViewLifecycleState DidLoading = new ViewLifecycleState(nameof(DidLoading));
        public static readonly ViewLifecycleState DidLoaded = new ViewLifecycleState(nameof(DidLoaded));
        public static readonly ViewLifecycleState WillAppearing = new ViewLifecycleState(nameof(WillAppearing));
        public static readonly ViewLifecycleState WillAppeared = new ViewLifecycleState(nameof(WillAppeared));
        public static readonly ViewLifecycleState WillDisappearing = new ViewLifecycleState(nameof(WillDisappearing));
        public static readonly ViewLifecycleState WillDisappeared = new ViewLifecycleState(nameof(WillDisappeared));
        public static readonly ViewLifecycleState DidMovingToParentViewController = new ViewLifecycleState(nameof(DidMovingToParentViewController));
        public static readonly ViewLifecycleState DidMovedToParentViewController = new ViewLifecycleState(nameof(DidMovedToParentViewController));
        public static readonly ViewLifecycleState WillMovingToParentViewController = new ViewLifecycleState(nameof(WillMovingToParentViewController));
        public static readonly ViewLifecycleState WillMovedToParentViewController = new ViewLifecycleState(nameof(WillMovedToParentViewController));
        public static readonly ViewLifecycleState RemovingFromParentViewController = new ViewLifecycleState(nameof(RemovingFromParentViewController));
        public static readonly ViewLifecycleState RemovedFromParentViewController = new ViewLifecycleState(nameof(RemovedFromParentViewController));

        #endregion

        #region Constructors

        private IosViewLifecycleState()
        {
        }

        #endregion
    }
}