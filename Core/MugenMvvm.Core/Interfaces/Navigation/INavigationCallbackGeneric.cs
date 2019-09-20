using System.Threading.Tasks;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationCallback<T> : INavigationCallback
    {
        new Task<T> WaitAsync();
    }
}