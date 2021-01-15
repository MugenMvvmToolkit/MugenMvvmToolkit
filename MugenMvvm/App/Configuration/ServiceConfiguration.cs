using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MugenMvvm.App.Configuration
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct ServiceConfiguration<TService> where TService : class
    {
        public readonly TService Service;
        public readonly MugenApplicationConfiguration AppConfiguration;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceConfiguration(MugenApplicationConfiguration appConfiguration, TService service)
        {
            Should.NotBeNull(service, nameof(service));
            Service = service;
            AppConfiguration = appConfiguration;
        }
    }
}