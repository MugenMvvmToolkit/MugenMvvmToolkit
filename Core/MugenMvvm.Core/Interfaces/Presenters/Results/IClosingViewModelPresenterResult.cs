using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Presenters.Results
{
    public interface IClosingViewModelPresenterResult : IHasMetadata<IReadOnlyMetadataContext>
    {
        Task<bool> ClosingTask { get; }
    }
}