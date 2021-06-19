using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IWrapper<out T> : IHasTarget<T> where T : class
    {
    }
}