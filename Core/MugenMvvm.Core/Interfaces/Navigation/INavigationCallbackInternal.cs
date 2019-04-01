using System;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationCallbackInternal : INavigationCallback
    {
        bool IsSerializable { get; }

        void SetResult(object? result, INavigationContext? navigationContext);

        void SetException(Exception exception, INavigationContext? navigationContext);

        void SetCanceled(INavigationContext? navigationContext);
    }
}