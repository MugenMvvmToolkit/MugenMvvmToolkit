using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.App.Configuration
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct MugenServiceConfiguration<TService> where TService : class
    {
        public readonly TService Service;
        private readonly MugenApplicationConfiguration _appConfiguration;

        public MugenServiceConfiguration(MugenApplicationConfiguration appConfiguration, TService service)
        {
            Should.NotBeNull(service, nameof(service));
            Service = service;
            _appConfiguration = appConfiguration;
        }

        public static implicit operator MugenApplicationConfiguration(MugenServiceConfiguration<TService> configuration) => configuration._appConfiguration;

        public MugenApplicationConfiguration AppConfiguration() => _appConfiguration;

        public MugenServiceConfiguration<TNewService> ServiceConfiguration<TNewService>() where TNewService : class => _appConfiguration.ServiceConfiguration<TNewService>();

        public MugenServiceConfiguration<TNewService> WithService<TNewService>(IComponentOwner<TNewService> service)
            where TNewService : class => _appConfiguration.InitializeService((TNewService) service);

        public MugenServiceConfiguration<TService> RemoveComponents<T>(IReadOnlyMetadataContext? metadata = null)
            where T : class, IComponent<TService>
        {
            ((IComponentOwner) Service).RemoveComponents<T>(metadata);
            return this;
        }
    }
}