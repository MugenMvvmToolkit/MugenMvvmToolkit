using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationContext : IMetadataOwner<IMetadataContext>
    {
        string NavigationOperationId { get; }

        NavigationMode NavigationMode { get; }

        NavigationType NavigationType { get; }

        INavigationProvider NavigationProvider { get; }
    }
}