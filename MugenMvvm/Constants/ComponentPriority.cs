namespace MugenMvvm.Constants
{
    public static class ComponentPriority
    {
        #region Fields

        public const int Max = int.MaxValue - 1;
        public const int Min = int.MinValue + 1;
        public const int Cache = int.MaxValue - 30;
        public const int Decorator = int.MaxValue - 50;
        public const int PreInitializer = int.MaxValue - 1000;
        public const int PostInitializer = int.MinValue + 500;

        #endregion
    }
}