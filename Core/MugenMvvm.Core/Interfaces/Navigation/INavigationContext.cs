using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationContext : IMetadataOwner<IMetadataContext>, IHasNavigationInfo
    {
        NavigationMode NavigationMode { get; }

        INavigationProvider NavigationProvider { get; }
    }
}