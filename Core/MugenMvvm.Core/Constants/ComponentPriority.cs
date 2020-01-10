namespace MugenMvvm.Constants
{
    public static class ComponentPriority
    {
        #region Fields

        public const int Cache = int.MaxValue - 10;
        public const int Decorator = int.MaxValue - 50;
        public const int PreInitializer = int.MaxValue - 1000;
        public const int PostInitializer = int.MinValue + 500;

        #endregion
    }
}