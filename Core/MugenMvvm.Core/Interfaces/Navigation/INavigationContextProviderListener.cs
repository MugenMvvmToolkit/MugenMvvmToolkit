using System.Runtime.CompilerServices;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationContextProviderListener : IListener
    {
        void OnNavigationContextCreated(INavigationContextProvider provider, INavigationContext context, IReadOnlyMetadataContext metadata);

        bool TryGetLastNavigationEntry(INavigationContextProvider provider, NavigationType navigationType, IReadOnlyMetadataContext metadata, [NotNullWhenTrue] out INavigationEntry? entry);
    }
}