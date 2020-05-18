using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationContext : IMetadataOwner<IMetadataContext>, IHasNavigationInfo, IHasNavigationProvider
    {
        NavigationMode NavigationMode { get; }
    }
}