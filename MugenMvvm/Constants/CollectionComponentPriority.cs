namespace MugenMvvm.Constants
{
    public static class CollectionComponentPriority
    {
        public const int DecoratorManager = ComponentPriority.PreInitializer + 1000;
        public const int BindableAdapter = ComponentPriority.PreInitializer;
        public const int BatchUpdateManager = 1000;
    }
}