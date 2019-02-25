using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views.Infrastructure;

namespace MugenMvvm.Infrastructure.Navigation
{
    public class DelegateNavigationMediatorFactory : INavigationMediatorViewModelPresenterManager
    {
        #region Fields

        private readonly Func<IViewModelBase, IViewInitializer, IReadOnlyMetadataContext, INavigationMediator?> _factory;

        #endregion

        #region Constructors

        public DelegateNavigationMediatorFactory(Func<IViewModelBase, IViewInitializer, IReadOnlyMetadataContext, INavigationMediator?> factory, int priority = 0)
        {
            _factory = factory;
            Priority = priority;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public INavigationMediator? TryGetMediator(INavigationMediatorViewModelPresenter presenter, IViewModelBase viewModel, IViewInitializer viewInitializer,
            IReadOnlyMetadataContext metadata)
        {
            return _factory(viewModel, viewInitializer, metadata);
        }

        public IReadOnlyList<IChildViewModelPresenterResult>? TryCloseInternal(INavigationMediatorViewModelPresenter presenter, IViewModelBase viewModel,
            IReadOnlyList<INavigationMediator> mediators, IReadOnlyMetadataContext metadata)
        {
            return null;
        }

        #endregion
    }
}