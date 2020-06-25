namespace MugenMvvm.Interfaces.Models
{
    public interface IHasOptionalService<out TService> where TService : class
    {
        TService? Service { get; }
    }
}