namespace MugenMvvm.Interfaces.Models
{
    public interface IHasTarget<out T>
    {
        T Target { get; }
    }
}