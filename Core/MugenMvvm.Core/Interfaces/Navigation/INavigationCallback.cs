using System.Threading.Tasks;
using MugenMvvm.Enums;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationCallback//todo review
    {
        NavigationCallbackType CallbackType { get; }

        NavigationType NavigationType { get; }

        Task WaitAsync();
    }

    public interface INavigationCallback<T> : INavigationCallback
    {
        new Task<T> WaitAsync();
    }
}