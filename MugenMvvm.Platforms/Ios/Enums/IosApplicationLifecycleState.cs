using MugenMvvm.Enums;

namespace MugenMvvm.Ios.Enums
{
    public sealed class IosApplicationLifecycleState : ApplicationLifecycleState
    {
        public static readonly ApplicationLifecycleState RestoringViewController = new(nameof(RestoringViewController));
        public static readonly ApplicationLifecycleState RestoredViewController = new(nameof(RestoredViewController));
        public static readonly ApplicationLifecycleState Preserving = new(nameof(Preserving));
        public static readonly ApplicationLifecycleState Preserved = new(nameof(Preserved));
        public static readonly ApplicationLifecycleState Restoring = new(nameof(Restoring));
        public static readonly ApplicationLifecycleState Restored = new(nameof(Restored));

        private IosApplicationLifecycleState()
        {
        }
    }
}