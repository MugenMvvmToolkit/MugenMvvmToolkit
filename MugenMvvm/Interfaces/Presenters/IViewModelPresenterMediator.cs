using System.Threading;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Interfaces.Presenters
{
    public interface IViewModelPresenterMediator
    {
        void Initialize(IViewModelBase viewModel, IViewMapping mapping, IReadOnlyMetadataContext? metadata);

        IPresenterResult? TryShow(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);

        IPresenterResult? TryClose(CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);
    }
}