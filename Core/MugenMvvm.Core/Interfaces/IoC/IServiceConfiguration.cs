namespace MugenMvvm.Interfaces.IoC
{
    public interface IServiceConfiguration<out TService>
        where TService : class
    {
        TService Instance { get; }

        TService InstanceOptional { get; }
    }
}