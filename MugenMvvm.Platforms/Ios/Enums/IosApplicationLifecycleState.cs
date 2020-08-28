using MugenMvvm.Enums;

namespace MugenMvvm.Ios.Enums
{
    public sealed class IosApplicationLifecycleState : ApplicationLifecycleState
    {
        #region Fields

        public static readonly ApplicationLifecycleState RestoringViewController = new ApplicationLifecycleState(nameof(RestoringViewController));
        public static readonly ApplicationLifecycleState RestoredViewController = new ApplicationLifecycleState(nameof(RestoredViewController));
        public static readonly ApplicationLifecycleState Preserving = new ApplicationLifecycleState(nameof(Preserving));
        public static readonly ApplicationLifecycleState Preserved = new ApplicationLifecycleState(nameof(Preserved));
        public static readonly ApplicationLifecycleState Restoring = new ApplicationLifecycleState(nameof(Restoring));
        public static readonly ApplicationLifecycleState Restored = new ApplicationLifecycleState(nameof(Restored));

        #endregion

        #region Constructors

        private IosApplicationLifecycleState()
        {
        }

        #endregion
    }
}