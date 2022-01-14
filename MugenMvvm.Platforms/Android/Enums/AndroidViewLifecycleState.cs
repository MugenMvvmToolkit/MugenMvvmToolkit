using MugenMvvm.Android.Native.Constants;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Enums;

namespace MugenMvvm.Android.Enums
{
    public sealed class AndroidViewLifecycleState : ViewLifecycleState
    {
        private static readonly ViewLifecycleState DestroyedCloseable = new(nameof(Destroyed), null, false) {BaseState = Closed};
        public static readonly ViewLifecycleState Finishing = new(nameof(Finishing)) {BaseState = Closing};
        public static readonly ViewLifecycleState Finished = new(nameof(Finished)) {BaseState = Closed};
        public static readonly ViewLifecycleState FinishingAfterTransition = new(nameof(FinishingAfterTransition)) {BaseState = Closing};
        public static readonly ViewLifecycleState FinishedAfterTransition = new(nameof(FinishedAfterTransition)) {BaseState = Closed};
        public static readonly ViewLifecycleState BackPressing = new(nameof(BackPressing)) {BaseState = Closing, NavigationMode = NavigationMode.Back};
        public static readonly ViewLifecycleState BackPressed = new(nameof(BackPressed)) {BaseState = Closed, NavigationMode = NavigationMode.Back};
        public static readonly ViewLifecycleState NewIntentChanging = new(nameof(NewIntentChanging));
        public static readonly ViewLifecycleState NewIntentChanged = new(nameof(NewIntentChanged));
        public static readonly ViewLifecycleState ConfigurationChanging = new(nameof(ConfigurationChanging));
        public static readonly ViewLifecycleState ConfigurationChanged = new(nameof(ConfigurationChanged));
        public static readonly ViewLifecycleState Creating = new(nameof(Creating));
        public static readonly ViewLifecycleState Created = new(nameof(Created));
        public static readonly ViewLifecycleState Destroying = new(nameof(Destroying));
        public static readonly ViewLifecycleState Destroyed = new(nameof(Destroyed));
        public static readonly ViewLifecycleState Pausing = new(nameof(Pausing)) {BaseState = Disappearing};
        public static readonly ViewLifecycleState Paused = new(nameof(Paused)) {BaseState = Disappeared};
        public static readonly ViewLifecycleState Restarting = new(nameof(Restarting));
        public static readonly ViewLifecycleState Restarted = new(nameof(Restarted));
        public static readonly ViewLifecycleState Resuming = new(nameof(Resuming)) {BaseState = Appearing};
        public static readonly ViewLifecycleState Resumed = new(nameof(Resumed)) {BaseState = Appeared};
        public static readonly ViewLifecycleState SavingState = new(nameof(SavingState));
        public static readonly ViewLifecycleState SavedState = new(nameof(SavedState));
        public static readonly ViewLifecycleState Starting = new(nameof(Starting));
        public static readonly ViewLifecycleState Started = new(nameof(Started));
        public static readonly ViewLifecycleState Stopping = new(nameof(Stopping));
        public static readonly ViewLifecycleState Stopped = new(nameof(Stopped));
        public static readonly ViewLifecycleState PostCreating = new(nameof(PostCreating));
        public static readonly ViewLifecycleState PostCreated = new(nameof(PostCreated));
        public static readonly ViewLifecycleState CreatingOptionsMenu = new(nameof(CreatingOptionsMenu));
        public static readonly ViewLifecycleState CreatedOptionsMenu = new(nameof(CreatedOptionsMenu));
        public static readonly ViewLifecycleState OptionsItemSelecting = new(nameof(OptionsItemSelecting));
        public static readonly ViewLifecycleState OptionsItemSelected = new(nameof(OptionsItemSelected));
        public static readonly ViewLifecycleState Dismissing = new(nameof(Dismissing)) {BaseState = Closing};
        public static readonly ViewLifecycleState Dismissed = new(nameof(Dismissed)) {BaseState = Closed};
        public static readonly ViewLifecycleState DismissingAllowingStateLoss = new(nameof(DismissingAllowingStateLoss)) {BaseState = Closing};
        public static readonly ViewLifecycleState DismissedAllowingStateLoss = new(nameof(DismissedAllowingStateLoss)) {BaseState = Closed};
        public static readonly ViewLifecycleState Canceling = new(nameof(Canceling)) {BaseState = Closing};
        public static readonly ViewLifecycleState Canceled = new(nameof(Canceled)) {BaseState = Closed};
        public static readonly ViewLifecycleState PendingInitialization = new(nameof(PendingInitialization));

        private AndroidViewLifecycleState()
        {
        }

        public static ViewLifecycleState? TryParseNativeChanging(object view, int state)
        {
            switch (state)
            {
                case NativeLifecycleState.Finish:
                    return Finishing;
                case NativeLifecycleState.FinishAfterTransition:
                    return FinishingAfterTransition;
                case NativeLifecycleState.BackPressed:
                    return BackPressing;
                case NativeLifecycleState.NewIntent:
                    return NewIntentChanging;
                case NativeLifecycleState.ConfigurationChanged:
                    return ConfigurationChanging;
                case NativeLifecycleState.Create:
                    return Creating;
                case NativeLifecycleState.Destroy:
                    return Destroying;
                case NativeLifecycleState.Pause:
                    return Pausing;
                case NativeLifecycleState.Restart:
                    return Restarting;
                case NativeLifecycleState.Resume:
                    return Resuming;
                case NativeLifecycleState.SaveState:
                    return SavingState;
                case NativeLifecycleState.Start:
                    return Starting;
                case NativeLifecycleState.Stop:
                    return Stopping;
                case NativeLifecycleState.PostCreate:
                    return PostCreating;
                case NativeLifecycleState.OptionsItemSelected:
                    return OptionsItemSelecting;
                case NativeLifecycleState.CreateOptionsMenu:
                    return CreatingOptionsMenu;
                case NativeLifecycleState.Dismiss:
                    return Dismissing;
                case NativeLifecycleState.DismissAllowingStateLoss:
                    return DismissingAllowingStateLoss;
                case NativeLifecycleState.Cancel:
                    return Canceling;
                case NativeLifecycleState.Appear:
                    return Appearing;
                case NativeLifecycleState.Disappear:
                    return Disappearing;
            }

            return null;
        }

        public static ViewLifecycleState? TryParseNativeChanged(object view, int state)
        {
            switch (state)
            {
                case NativeLifecycleState.Finish:
                    return Finished;
                case NativeLifecycleState.FinishAfterTransition:
                    return FinishedAfterTransition;
                case NativeLifecycleState.BackPressed:
                    return BackPressed;
                case NativeLifecycleState.NewIntent:
                    return NewIntentChanged;
                case NativeLifecycleState.ConfigurationChanged:
                    return ConfigurationChanged;
                case NativeLifecycleState.Create:
                    return Created;
                case NativeLifecycleState.Destroy:
                    return view is IActivityView activityView && activityView.IsFinishing ? DestroyedCloseable : Destroyed;
                case NativeLifecycleState.Pause:
                    return Paused;
                case NativeLifecycleState.Restart:
                    return Restarted;
                case NativeLifecycleState.Resume:
                    return Resumed;
                case NativeLifecycleState.SaveState:
                    return SavedState;
                case NativeLifecycleState.Start:
                    return Started;
                case NativeLifecycleState.Stop:
                    return Stopped;
                case NativeLifecycleState.PostCreate:
                    return PostCreated;
                case NativeLifecycleState.OptionsItemSelected:
                    return OptionsItemSelected;
                case NativeLifecycleState.CreateOptionsMenu:
                    return CreatedOptionsMenu;
                case NativeLifecycleState.Dismiss:
                    return Dismissed;
                case NativeLifecycleState.DismissAllowingStateLoss:
                    return DismissedAllowingStateLoss;
                case NativeLifecycleState.Cancel:
                    return Canceled;
                case NativeLifecycleState.Appear:
                    return Appeared;
                case NativeLifecycleState.Disappear:
                    return Disappeared;
            }

            return null;
        }
    }
}