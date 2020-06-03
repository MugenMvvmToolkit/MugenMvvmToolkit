using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationContext : IMetadataOwner<IMetadataContext>, IHasNavigationInfo, IHasNavigationProvider, IHasTarget<object?>
    {
        NavigationMode NavigationMode { get; }
    }
}