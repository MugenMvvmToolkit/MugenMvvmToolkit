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

        public static MugenApplicationConfiguration Configure() => Configure(MugenService.Optional<IMugenApplication>() ?? new MugenApplication());

        public static MugenApplicationConfiguration Configure(IMugenApplication application) => new(application);

        public ServiceConfiguration<TService> ServiceConfiguration<TService>() where TService : class => new(this);

        #endregion
    }
}