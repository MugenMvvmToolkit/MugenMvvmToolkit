using System.Runtime.InteropServices;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Configuration;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.App.Configuration
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct MugenApplicationConfiguration
    {
        public readonly IMugenApplicationConfigurator? Configurator;
        public readonly IMugenApplication Application;

        public MugenApplicationConfiguration(IMugenApplication application, IMugenApplicationConfigurator? configurator)
        {
            Should.NotBeNull(application, nameof(application));
            Application = application;
            Configurator = configurator;
        }

        public static MugenApplicationConfiguration Configure(IMugenApplicationConfigurator? configurator = null) => Configure(new MugenApplication(), configurator);

        public static MugenApplicationConfiguration Configure(IMugenApplication application, IMugenApplicationConfigurator? configurator = null)
        {
            var configuration = new MugenApplicationConfiguration(application, configurator);
            configuration.InitializeService(application);
            return configuration;
        }

        public bool HasService<TService>() where TService : class => GetServiceOptional<TService>() != null;

        public TService GetService<TService>() where TService : class
        {
            if (Configurator == null)
                return MugenService.Instance<TService>();
            return Configurator.GetService<TService>(false)!;
        }

        public TService? GetServiceOptional<TService>() where TService : class
        {
            if (Configurator == null)
                return MugenService.Optional<TService>();
            return Configurator.GetService<TService>(true);
        }

        public ServiceConfiguration<TService> InitializeService<TService>(TService service) where TService : class
        {
            if (Configurator == null)
                MugenService.Configuration.InitializeInstance(service);
            else
                Configurator.InitializeService(service);
            return new ServiceConfiguration<TService>(this, service);
        }

        public MugenApplicationConfiguration Initialize(IPlatformInfo platformInfo, object? state = null, EnumFlags<ApplicationFlags> flags = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Application.Initialize(platformInfo, state, flags, metadata);
            return this;
        }

        public ServiceConfiguration<TService> ServiceConfiguration<TService>() where TService : class => new(this, GetService<TService>());
    }
}