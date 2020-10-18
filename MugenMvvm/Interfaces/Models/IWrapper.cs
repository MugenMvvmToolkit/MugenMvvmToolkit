namespace MugenMvvm.Interfaces.Models
{
    public interface IWrapper<out T> : IHasTarget<T> where T : class
    {
    }
}