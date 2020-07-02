using System.Runtime.InteropServices;

namespace MugenMvvm.App.Configuration
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct ServiceConfiguration<TService> where TService : class
    {
        #region Fields

        public readonly MugenApplicationConfiguration AppConfiguration;

        #endregion

        #region Constructors

        public ServiceConfiguration(MugenApplicationConfiguration appConfiguration)
        {
            AppConfiguration = appConfiguration;
        }

        #endregion
    }
}