using System.Runtime.CompilerServices;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationContextFactoryListener : IListener
    {
        void OnNavigationContextCreated(INavigationContextFactory factory, INavigationContext context, IReadOnlyMetadataContext metadata);

        bool TryGetLastNavigationEntry(NavigationType navigationType, IReadOnlyMetadataContext metadata, [NotNullWhenTrue] out INavigationEntry? entry);
    }
}