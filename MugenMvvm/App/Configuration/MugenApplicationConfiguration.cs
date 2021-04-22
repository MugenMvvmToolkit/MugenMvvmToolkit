using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Configuration;

namespace MugenMvvm.App.Configuration
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct MugenApplicationConfiguration
    {
        public readonly IMugenApplicationConfigurator? Configurator;
        public readonly IMugenApplication Application;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MugenApplicationConfiguration(IMugenApplication application, IMugenApplicationConfigurator? configurator)
        {
            Should.NotBeNull(application, nameof(application));
            Application = application;
            Configurator = configurator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MugenApplicationConfiguration Configure(IMugenApplicationConfigurator? configurator = null) => Configure(new MugenApplication(), configurator);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MugenApplicationConfiguration Configure(IMugenApplication application, IMugenApplicationConfigurator? configurator = null)
        {
            var configuration = new MugenApplicationConfiguration(application, configurator);
            configuration.InitializeService(application);
            return configuration;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasService<TService>() where TService : class => GetServiceOptional<TService>() != null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TService GetService<TService>() where TService : class
        {
            if (Configurator == null)
                return MugenService.Instance<TService>();
            return Configurator.GetService<TService>(false)!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TService? GetServiceOptional<TService>() where TService : class
        {
            if (Configurator == null)
                return MugenService.Optional<TService>();
            return Configurator.GetService<TService>(true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceConfiguration<TService> InitializeService<TService>(TService service) where TService : class
        {
            if (Configurator == null)
                MugenService.Configuration.InitializeInstance(service);
            else
                Configurator.InitializeService(service);
            return new ServiceConfiguration<TService>(this, service);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceConfiguration<TService> ServiceConfiguration<TService>() where TService : class => new(this, GetService<TService>());
    }
}