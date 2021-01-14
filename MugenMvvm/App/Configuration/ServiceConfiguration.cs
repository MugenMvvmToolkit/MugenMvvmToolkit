using System.Runtime.InteropServices;

namespace MugenMvvm.App.Configuration
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct ServiceConfiguration<TService> where TService : class
    {
        public readonly MugenApplicationConfiguration AppConfiguration;

        public ServiceConfiguration(MugenApplicationConfiguration appConfiguration)
        {
            AppConfiguration = appConfiguration;
        }
    }
}