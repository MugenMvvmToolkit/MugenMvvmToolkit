using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationContext : IMetadataOwner<IMetadataContext>
    {
        string NavigationOperationId { get; }

        INavigationProvider NavigationProvider { get; }

        NavigationMode NavigationMode { get; }

        NavigationType NavigationType { get; }
    }
}