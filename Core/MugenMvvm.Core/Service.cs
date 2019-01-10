namespace MugenMvvm
{
    public static class Service<TService>
        where TService : class
    {
        #region Properties

        public static TService Instance => ServiceConfiguration<TService>.Instance;

        public static TService? InstanceOptional => ServiceConfiguration<TService>.InstanceOptional;

        #endregion
    }
}