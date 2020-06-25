namespace MugenMvvm.Interfaces.Models
{
    public interface IWrapper<out T> where T : class
    {
        T Target { get; }
    }
}