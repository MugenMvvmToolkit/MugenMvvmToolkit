namespace MugenMvvm.Constants
{
    public static class ViewModelComponentPriority
    {
        public const int Provider = 0;
        public const int ServiceResolver = 0;
        public const int PostInitializer = -10;
        public const int LifecycleTracker = -100;
    }
}