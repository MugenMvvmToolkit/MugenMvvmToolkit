namespace MugenMvvm.Interfaces.Models
{
    public interface IHasTarget<out T> where T : class?
    {
        T Target { get; }
    }
}