namespace MugenMvvm.Interfaces.App.Configuration
{
    public interface IMugenApplicationConfigurator
    {
        TService? GetService<TService>(bool optional) where TService : class;

        void InitializeService<TService>(TService service) where TService : class;
    }
}