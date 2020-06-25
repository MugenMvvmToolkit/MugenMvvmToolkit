namespace MugenMvvm.Interfaces.Models
{
    public interface IHasId<out TType>
    {
        TType Id { get; }
    }
}