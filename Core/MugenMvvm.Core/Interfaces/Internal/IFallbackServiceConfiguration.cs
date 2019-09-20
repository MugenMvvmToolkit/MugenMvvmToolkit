namespace MugenMvvm.Interfaces.Internal
{
    public interface IFallbackServiceConfiguration
    {
        TService Instance<TService>() where TService : class;

        TService? InstanceOptional<TService>() where TService : class;
    }
}