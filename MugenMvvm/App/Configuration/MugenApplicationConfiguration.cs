using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Interfaces.App;

namespace MugenMvvm.App.Configuration
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct MugenApplicationConfiguration
    {
        public readonly IMugenApplication Application;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MugenApplicationConfiguration(IMugenApplication application)
        {
            Should.NotBeNull(application, nameof(application));
            Application = application;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MugenApplicationConfiguration Configure() => Configure(new MugenApplication());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MugenApplicationConfiguration Configure(IMugenApplication application) => new(application);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceConfiguration<TService> ServiceConfiguration<TService>() where TService : class => new(this, MugenService.Instance<TService>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceConfiguration<TService> ServiceConfiguration<TService>(TService service) where TService : class => new(this, service);
    }
}