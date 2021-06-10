namespace MugenMvvm.Constants
{
    public static class ViewModelComponentPriority
    {
        public const int Provider = 0;
        public const int ServiceProvider = 0;
        public const int InheritParentServiceResolver = 10;
        public const int PostInitializer = -100;
        public const int PreInitializer = 100;
        public const int LifecycleTracker = -1000;
    }
}