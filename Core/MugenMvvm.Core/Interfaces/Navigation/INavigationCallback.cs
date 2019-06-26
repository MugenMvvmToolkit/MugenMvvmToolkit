using System.Threading.Tasks;
using MugenMvvm.Enums;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationCallback
    {
        NavigationCallbackType CallbackType { get; }

        NavigationType NavigationType { get; }

        Task WaitAsync();
    }
}