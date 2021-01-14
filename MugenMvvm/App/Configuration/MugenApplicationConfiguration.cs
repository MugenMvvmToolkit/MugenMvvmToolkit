using System.Runtime.InteropServices;
using MugenMvvm.Interfaces.App;

namespace MugenMvvm.App.Configuration
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct MugenApplicationConfiguration
    {
        public readonly IMugenApplication Application;

        public MugenApplicationConfiguration(IMugenApplication application)
        {
            Should.NotBeNull(application, nameof(application));
            Application = application;
        }

        public static MugenApplicationConfiguration Configure() => Configure(MugenService.Optional<IMugenApplication>() ?? new MugenApplication());

        public static MugenApplicationConfiguration Configure(IMugenApplication application) => new(application);

        public ServiceConfiguration<TService> ServiceConfiguration<TService>() where TService : class => new(this);
    }
}