namespace MugenMvvm.Constants
{
    public static class ComponentPriority
    {
        #region Fields

        public const int Cache = int.MaxValue - 10;
        public const int Decorator = int.MaxValue - 100;

        public const int DefaultPreInitializer = int.MaxValue - 1000;
        public const int DefaultPostInitializer = int.MinValue + 1000;

        #endregion
    }
}