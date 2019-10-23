using System.Runtime.CompilerServices;

namespace MugenMvvm
{
    public static class Service<TService>
        where TService : class
    {
        #region Properties

        public static TService Instance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ServiceConfiguration.Configuration<TService>.Instance;
        }

        public static TService? InstanceOptional => ServiceConfiguration.Configuration<TService>.InstanceOptional;

        #endregion
    }
}