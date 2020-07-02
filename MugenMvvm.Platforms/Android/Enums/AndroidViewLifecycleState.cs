﻿using MugenMvvm.Enums;
using MugenMvvm.Android.Native.Constants;

namespace MugenMvvm.Android.Enums
{
    public abstract class AndroidViewLifecycleState : ViewLifecycleState
    {
        #region Fields

        public static readonly ViewLifecycleState Finishing = new ViewLifecycleState(nameof(Finishing));
        public static readonly ViewLifecycleState Finished = new ViewLifecycleState(nameof(Finished));
        public static readonly ViewLifecycleState FinishingAfterTransition = new ViewLifecycleState(nameof(FinishingAfterTransition));
        public static readonly ViewLifecycleState FinishedAfterTransition = new ViewLifecycleState(nameof(FinishedAfterTransition));
        public static readonly ViewLifecycleState BackPressing = new ViewLifecycleState(nameof(BackPressing));
        public static readonly ViewLifecycleState BackPressed = new ViewLifecycleState(nameof(BackPressed));
        public static readonly ViewLifecycleState NewIntentChanging = new ViewLifecycleState(nameof(NewIntentChanging));
        public static readonly ViewLifecycleState NewIntentChanged = new ViewLifecycleState(nameof(NewIntentChanged));
        public static readonly ViewLifecycleState ConfigurationChanging = new ViewLifecycleState(nameof(ConfigurationChanging));
        public static readonly ViewLifecycleState ConfigurationChanged = new ViewLifecycleState(nameof(ConfigurationChanged));
        public static readonly ViewLifecycleState Creating = new ViewLifecycleState(nameof(Creating));
        public static readonly ViewLifecycleState Created = new ViewLifecycleState(nameof(Created));
        public static readonly ViewLifecycleState Destroying = new ViewLifecycleState(nameof(Destroying));
        public static readonly ViewLifecycleState Destroyed = new ViewLifecycleState(nameof(Destroyed));
        public static readonly ViewLifecycleState Pausing = new ViewLifecycleState(nameof(Pausing));
        public static readonly ViewLifecycleState Paused = new ViewLifecycleState(nameof(Paused));
        public static readonly ViewLifecycleState Restarting = new ViewLifecycleState(nameof(Restarting));
        public static readonly ViewLifecycleState Restarted = new ViewLifecycleState(nameof(Restarted));
        public static readonly ViewLifecycleState Resuming = new ViewLifecycleState(nameof(Resuming));
        public static readonly ViewLifecycleState Resumed = new ViewLifecycleState(nameof(Resumed));
        public static readonly ViewLifecycleState SavingState = new ViewLifecycleState(nameof(SavingState));
        public static readonly ViewLifecycleState SavedState = new ViewLifecycleState(nameof(SavedState));
        public static readonly ViewLifecycleState Starting = new ViewLifecycleState(nameof(Starting));
        public static readonly ViewLifecycleState Started = new ViewLifecycleState(nameof(Started));
        public static readonly ViewLifecycleState Stopping = new ViewLifecycleState(nameof(Stopping));
        public static readonly ViewLifecycleState Stopped = new ViewLifecycleState(nameof(Stopped));
        public static readonly ViewLifecycleState PostCreating = new ViewLifecycleState(nameof(PostCreating));
        public static readonly ViewLifecycleState PostCreated = new ViewLifecycleState(nameof(PostCreated));

        public static readonly ViewLifecycleState ClearBackStack = new ViewLifecycleState(nameof(ClearBackStack));

        #endregion

        #region Methods

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
            }

            return null;
        }

        #endregion
    }
}