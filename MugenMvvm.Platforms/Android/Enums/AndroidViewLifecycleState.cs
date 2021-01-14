using MugenMvvm.Android.Native.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Android.Enums
{
    public sealed class AndroidViewLifecycleState : ViewLifecycleState
    {
        public static readonly ViewLifecycleState Finishing = new(nameof(Finishing));
        public static readonly ViewLifecycleState Finished = new(nameof(Finished));
        public static readonly ViewLifecycleState FinishingAfterTransition = new(nameof(FinishingAfterTransition));
        public static readonly ViewLifecycleState FinishedAfterTransition = new(nameof(FinishedAfterTransition));
        public static readonly ViewLifecycleState BackPressing = new(nameof(BackPressing));
        public static readonly ViewLifecycleState BackPressed = new(nameof(BackPressed));
        public static readonly ViewLifecycleState NewIntentChanging = new(nameof(NewIntentChanging));
        public static readonly ViewLifecycleState NewIntentChanged = new(nameof(NewIntentChanged));
        public static readonly ViewLifecycleState ConfigurationChanging = new(nameof(ConfigurationChanging));
        public static readonly ViewLifecycleState ConfigurationChanged = new(nameof(ConfigurationChanged));
        public static readonly ViewLifecycleState Creating = new(nameof(Creating));
        public static readonly ViewLifecycleState Created = new(nameof(Created));
        public static readonly ViewLifecycleState Destroying = new(nameof(Destroying));
        public static readonly ViewLifecycleState Destroyed = new(nameof(Destroyed));
        public static readonly ViewLifecycleState Pausing = new(nameof(Pausing));
        public static readonly ViewLifecycleState Paused = new(nameof(Paused));
        public static readonly ViewLifecycleState Restarting = new(nameof(Restarting));
        public static readonly ViewLifecycleState Restarted = new(nameof(Restarted));
        public static readonly ViewLifecycleState Resuming = new(nameof(Resuming));
        public static readonly ViewLifecycleState Resumed = new(nameof(Resumed));
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
        public static readonly ViewLifecycleState Dismissing = new(nameof(Dismissing));
        public static readonly ViewLifecycleState Dismissed = new(nameof(Dismissed));
        public static readonly ViewLifecycleState DismissingAllowingStateLoss = new(nameof(DismissingAllowingStateLoss));
        public static readonly ViewLifecycleState DismissedAllowingStateLoss = new(nameof(DismissedAllowingStateLoss));
        public static readonly ViewLifecycleState Canceling = new(nameof(Canceling));
        public static readonly ViewLifecycleState Canceled = new(nameof(Canceled));
        public static readonly ViewLifecycleState PendingInitialization = new(nameof(PendingInitialization));

        private AndroidViewLifecycleState()
        {
        }

        public static ViewLifecycleState? TryParseNativeChanging(int state)
        {
            switch (state)
            {
                case LifecycleState.Finish:
                    return Finishing;
                case LifecycleState.FinishAfterTransition:
                    return FinishingAfterTransition;
                case LifecycleState.BackPressed:
                    return BackPressing;
                case LifecycleState.NewIntent:
                    return NewIntentChanging;
                case LifecycleState.ConfigurationChanged:
                    return ConfigurationChanging;
                case LifecycleState.Create:
                    return Creating;
                case LifecycleState.Destroy:
                    return Destroying;
                case LifecycleState.Pause:
                    return Pausing;
                case LifecycleState.Restart:
                    return Restarting;
                case LifecycleState.Resume:
                    return Resuming;
                case LifecycleState.SaveState:
                    return SavingState;
                case LifecycleState.Start:
                    return Starting;
                case LifecycleState.Stop:
                    return Stopping;
                case LifecycleState.PostCreate:
                    return PostCreating;
                case LifecycleState.OptionsItemSelected:
                    return OptionsItemSelecting;
                case LifecycleState.CreateOptionsMenu:
                    return CreatingOptionsMenu;
                case LifecycleState.Dismiss:
                    return Dismissing;
                case LifecycleState.DismissAllowingStateLoss:
                    return DismissingAllowingStateLoss;
                case LifecycleState.Cancel:
                    return Canceling;
            }

            return null;
        }

        public static ViewLifecycleState? TryParseNativeChanged(int state)
        {
            switch (state)
            {
                case LifecycleState.Finish:
                    return Finished;
                case LifecycleState.FinishAfterTransition:
                    return FinishedAfterTransition;
                case LifecycleState.BackPressed:
                    return BackPressed;
                case LifecycleState.NewIntent:
                    return NewIntentChanged;
                case LifecycleState.ConfigurationChanged:
                    return ConfigurationChanged;
                case LifecycleState.Create:
                    return Created;
                case LifecycleState.Destroy:
                    return Destroyed;
                case LifecycleState.Pause:
                    return Paused;
                case LifecycleState.Restart:
                    return Restarted;
                case LifecycleState.Resume:
                    return Resumed;
                case LifecycleState.SaveState:
                    return SavedState;
                case LifecycleState.Start:
                    return Started;
                case LifecycleState.Stop:
                    return Stopped;
                case LifecycleState.PostCreate:
                    return PostCreated;
                case LifecycleState.OptionsItemSelected:
                    return OptionsItemSelected;
                case LifecycleState.CreateOptionsMenu:
                    return CreatedOptionsMenu;
                case LifecycleState.Dismiss:
                    return Dismissed;
                case LifecycleState.DismissAllowingStateLoss:
                    return DismissedAllowingStateLoss;
                case LifecycleState.Cancel:
                    return Canceled;
            }

            return null;
        }
    }
}