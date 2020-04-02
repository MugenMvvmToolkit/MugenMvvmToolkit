namespace MugenMvvm.Interfaces.Models
{
    public interface IWrapper<out T>
    {
        T Target { get; }
    }
}