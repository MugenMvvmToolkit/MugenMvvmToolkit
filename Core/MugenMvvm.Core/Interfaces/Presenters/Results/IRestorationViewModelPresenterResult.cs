using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Presenters.Results
{
    public interface IRestorationViewModelPresenterResult : IHasMetadata<IReadOnlyMetadataContext>
    {
        bool IsRestored { get; }
    }
}