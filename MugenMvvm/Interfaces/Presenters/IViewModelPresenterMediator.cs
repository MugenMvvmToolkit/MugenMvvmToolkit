using System.Threading;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Interfaces.Presenters
{
    public interface IViewModelPresenterMediator
    {
        IViewMapping Mapping { get; }

        IViewModelBase ViewModel { get; }

        IView? View { get; }

        IPresenterResult? TryShow(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);

        IPresenterResult? TryClose(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);
    }
}