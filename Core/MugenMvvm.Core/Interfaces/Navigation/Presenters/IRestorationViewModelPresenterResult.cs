using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IRestorationViewModelPresenterResult : IHasMetadata<IReadOnlyMetadataContext>
    {
        NavigationType NavigationType { get; }

        bool IsRestored { get; }
    }
}