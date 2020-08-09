using System;
using System.Threading;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.UnitTest.Presenters.Internal
{
    public class TestViewModelPresenterMediator : IViewModelPresenterMediator, INavigationProvider
    {
        #region Properties

        public Action<IViewModelBase, IViewMapping, IReadOnlyMetadataContext?>? Initialize { get; set; }

        public Func<object?, CancellationToken, IReadOnlyMetadataContext?, IPresenterResult?>? TryShow { get; set; }

        public Func<CancellationToken, IReadOnlyMetadataContext?, IPresenterResult?>? TryClose { get; set; }

        public string Id { get; set; } = null!;

        #endregion

        #region Implementation of interfaces

        void IViewModelPresenterMediator.Initialize(IViewModelBase viewModel, IViewMapping mapping, IReadOnlyMetadataContext? metadata) => Initialize?.Invoke(viewModel, mapping, metadata);

        IPresenterResult? IViewModelPresenterMediator.TryShow(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) => TryShow?.Invoke(view, cancellationToken, metadata);

        IPresenterResult? IViewModelPresenterMediator.TryClose(CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) => TryClose?.Invoke(cancellationToken, metadata);

        #endregion
    }
}