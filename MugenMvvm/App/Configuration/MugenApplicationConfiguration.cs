using System.Runtime.InteropServices;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Configuration;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.App.Configuration
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct MugenApplicationConfiguration
    {
        public readonly IMugenApplicationConfigurator? Configurator;
        public readonly IMugenApplication Application;
        public readonly IReadOnlyMetadataContext? Metadata;

        public MugenApplicationConfiguration(IMugenApplication application, IMugenApplicationConfigurator? configurator, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(application, nameof(application));
            Application = application;
            Configurator = configurator;
            Metadata = metadata;
        }

        public static MugenApplicationConfiguration Configure(IMugenApplicationConfigurator? configurator = null, IReadOnlyMetadataContext? metadata = null) =>
            Configure(new MugenApplication(), configurator, metadata);

        public static MugenApplicationConfiguration Configure(IMugenApplication application, IMugenApplicationConfigurator? configurator = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            var configuration = new MugenApplicationConfiguration(application, configurator, metadata);
            configuration.InitializeService(application);
            return configuration;
        }

        public MugenServiceConfiguration<TService> WithService<TService>(IComponentOwner<TService> service) where TService : class => InitializeService((TService) service);

        public MugenApplicationConfiguration WithMetadata(IReadOnlyMetadataContext? metadata) => new(Application, Configurator, metadata);

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

        public MugenServiceConfiguration<TService> InitializeService<TService>(TService service) where TService : class
        {
            if (Configurator == null)
                MugenService.Configuration.InitializeInstance(service);
            else
                Configurator.InitializeService(service);
            return new MugenServiceConfiguration<TService>(this, service);
        }

        public MugenApplicationConfiguration Initialize(IPlatformInfo platformInfo, object? state = null, EnumFlags<ApplicationFlags> flags = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            if (!Metadata.IsNullOrEmpty() && !metadata.IsNullOrEmpty())
                metadata = metadata.ToNonReadonly().Merge(Metadata);
            Application.Initialize(platformInfo, state, flags, metadata);
            return this;
        }

        public MugenServiceConfiguration<TService> ServiceConfiguration<TService>() where TService : class => new(this, GetService<TService>());
    }
}