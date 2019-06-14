using MugenMvvm.Infrastructure;

namespace MugenMvvm
{
    public static class Service<TService>//todo check and use as dependency if possible
        where TService : class
    {
        #region Properties

        public static TService Instance => ServiceConfiguration<TService>.Instance;

        public static TService? InstanceOptional => ServiceConfiguration<TService>.InstanceOptional;

        #endregion
    }
}