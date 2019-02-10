using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewManagerCleanupResult : IHasMetadata<IReadOnlyMetadataContext>
    {
        Task WaitAsync();
    }
}