using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationCallback : IHasNavigationInfo
    {
        NavigationCallbackType CallbackType { get; }

        Task<IReadOnlyMetadataContext> WaitAsync();
    }
}