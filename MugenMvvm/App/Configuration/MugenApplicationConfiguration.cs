using System.Runtime.InteropServices;
using MugenMvvm.Interfaces.App;

namespace MugenMvvm.App.Configuration
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct MugenApplicationConfiguration
    {
        #region Fields

        public readonly IMugenApplication Application;

        #endregion

        #region Constructors

        public MugenApplicationConfiguration(IMugenApplication application)
        {
            Should.NotBeNull(application, nameof(application));
            Application = application;
        }

        #endregion

        #region Methods

        public static MugenApplicationConfiguration Get()
        {
            return Get(MugenService.Optional<IMugenApplication>() ?? new MugenApplication());
        }

        public static MugenApplicationConfiguration Get(IMugenApplication application)
        {
            return new MugenApplicationConfiguration(application);
        }

        public ServiceConfiguration<TService> ServiceConfiguration<TService>() where TService : class
        {
            return new ServiceConfiguration<TService>(this);
        }

        #endregion
    }
}