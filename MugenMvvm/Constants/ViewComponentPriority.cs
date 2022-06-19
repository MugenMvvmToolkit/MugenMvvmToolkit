namespace MugenMvvm.Constants
{
    public static class ViewComponentPriority
    {
        public const int Initializer = 0;
        public const int StateManager = 0;
        public const int MappingProvider = 0;
        public const int ViewCollectionManager = 0;
        public const int ExecutionModeDecorator = 100;
        public const int ViewModelViewProviderDecorator = 50;
        public const int LifecycleTracker = ComponentPriority.PreInitializer;
        public const int PreInitializer = 10;
        public const int PostInitializer = -10;
        public const int UndefinedMappingDecorator = ViewModelViewProviderDecorator + 10;
        public const int RawViewDispatcher = ComponentPriority.Max;
#if ANDROID
        public const int ActivityRequestDecorator = ExecutionModeDecorator + 10;
        public const int ResourceMappingDecorator = MappingProvider + 10;
        public const int ResourceRequestDecorator = UndefinedMappingDecorator + 10;
#endif
    }
}