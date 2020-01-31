using MugenMvvm.Enums;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationCallback : IHasNavigationInfo
    {
        bool IsCompleted { get; }

        NavigationCallbackType CallbackType { get; }

        ActionToken RegisterCallback(INavigationCallbackListener callback);
    }
}