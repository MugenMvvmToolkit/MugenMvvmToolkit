using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationCallback : IHasNavigationInfo
    {
        bool IsCompleted { get; }

        NavigationCallbackType CallbackType { get; }

        void AddCallback(INavigationCallbackListener callback);

        void RemoveCallback(INavigationCallbackListener callback);

        ItemOrIReadOnlyList<INavigationCallbackListener> GetCallbacks();
    }
}