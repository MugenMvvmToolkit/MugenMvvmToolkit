namespace MugenMvvm.Constants
{
    public static class ViewComponentPriority
    {
        #region Fields

        public const int Initializer = 0;
        public const int StateManager = 0;
        public const int MappingProvider = 0;
        public const int ExecutionModeDecorator = 100;
        public const int ViewModelViewProviderDecorator = 50;
        public const int PreInitializer = 10;
        public const int PostInitializer = -10;
        public const int LifecycleTracker = -100;

        #endregion
    }
}