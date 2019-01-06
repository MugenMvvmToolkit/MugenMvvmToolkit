using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.Presenters.Results
{
    public interface IChildViewModelPresenterResult : IHasMetadata<IReadOnlyMetadataContext>
    {
        NavigationType NavigationType { get; }

        bool IsRestorable { get; }
    }
}