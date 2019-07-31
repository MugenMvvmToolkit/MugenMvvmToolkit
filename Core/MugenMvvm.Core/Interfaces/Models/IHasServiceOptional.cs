namespace MugenMvvm.Interfaces.Models
{
    public interface IHasServiceOptional<out TService> where TService : class?
    {
        TService ServiceOptional { get; }
    }
}