using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationEntry
    {
        NavigationType NavigationType { get; }

        IViewModel ViewModel { get; }

        object? Provider { get; }

        IReadOnlyList<INavigationCallback> GetCallbacks(NavigationCallbackType? callbackType, IReadOnlyMetadataContext metadata);
    }
}