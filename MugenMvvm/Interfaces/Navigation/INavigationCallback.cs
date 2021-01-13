using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Collections;
using MugenMvvm.Enums;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationCallback : IHasNavigationInfo
    {
        bool IsCompleted { get; }

        NavigationCallbackType CallbackType { get; }

        bool TryGetResult([NotNullWhen(true)] out INavigationContext? navigationContext);

        void AddCallback(INavigationCallbackListener callback);

        void RemoveCallback(INavigationCallbackListener callback);

        ItemOrIReadOnlyList<INavigationCallbackListener> GetCallbacks();
    }
}