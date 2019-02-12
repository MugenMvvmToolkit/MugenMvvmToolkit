namespace MugenMvvm.Interfaces.Models
{
    public interface IHasService<out TService> where TService : class
    {
        TService Service { get; }
    }
}