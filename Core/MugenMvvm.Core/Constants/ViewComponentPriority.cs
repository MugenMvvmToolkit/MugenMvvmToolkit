namespace MugenMvvm.Constants
{
    public static class ViewComponentPriority
    {
        #region Fields

        public const int Initializer = 0;
        public const int MappingProvider = 0;
        public const int InitializeLifecycle = 1;
        public const int CleanupLifecycle = -1;

        #endregion
    }
}