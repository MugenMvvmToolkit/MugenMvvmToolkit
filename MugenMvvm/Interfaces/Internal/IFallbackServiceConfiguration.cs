namespace MugenMvvm.Interfaces.Internal
{
    public interface IFallbackServiceConfiguration
    {
        TService Instance<TService>() where TService : class;

        TService? Optional<TService>() where TService : class;
    }
}