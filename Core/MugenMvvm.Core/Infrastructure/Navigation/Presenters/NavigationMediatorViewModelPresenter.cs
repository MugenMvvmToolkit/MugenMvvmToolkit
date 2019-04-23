using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Infrastructure.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views.Infrastructure;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class NavigationMediatorViewModelPresenter : AttachableComponentBase<IViewModelPresenter>, INavigationMediatorViewModelPresenter, INavigationDispatcherListener
    {
        #region Fields

        private IComponentCollection<INavigationMediatorViewModelPresenterManager>? _managers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationMediatorViewModelPresenter(IViewManager viewManager, INavigationDispatcher navigationDispatcher,
            IComponentCollectionProvider componentCollectionProvider, IMetadataContextProvider metadataContextProvider)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            Should.NotBeNull(metadataContextProvider, nameof(metadataContextProvider));
            ComponentCollectionProvider = componentCollectionProvider;
            MetadataContextProvider = metadataContextProvider;
            ViewManager = viewManager;
            NavigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        protected IComponentCollectionProvider ComponentCollectionProvider { get; }

        protected IMetadataContextProvider MetadataContextProvider { get; }

        protected IViewManager ViewManager { get; }

        protected INavigationDispatcher NavigationDispatcher { get; }

        public IComponentCollection<INavigationMediatorViewModelPresenterManager> Managers
        {
            get
            {
                if (_managers == null)
                    ComponentCollectionProvider.LazyInitialize(ref _managers, this);
                return _managers;
            }
        }

        public int Priority { get; set; } = PresenterConstants.GenericNavigationPriority;

        public int NavigationDispatcherListenerPriority { get; set; }

        #endregion

        #region Implementation of interfaces

        int IListener.GetPriority(object source)
        {
            return NavigationDispatcherListenerPriority;
        }

        Task<bool>? INavigationDispatcherListener.OnNavigatingAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            return null;
        }

        void INavigationDispatcherListener.OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            if (navigationContext.NavigationMode.IsClose && navigationContext.NavigationProvider is INavigationMediator mediator)
            {
                var mediators = mediator.ViewModel.Metadata.Get(NavigationInternalMetadata.NavigationMediators);
                if (mediators == null)
                    return;
                lock (mediators)
                {
                    mediators.Remove(mediator);
                }
            }
        }

        void INavigationDispatcherListener.OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception)
        {
        }

        void INavigationDispatcherListener.OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
        }

        void INavigationDispatcherListener.OnNavigatingCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
        }

        public IChildViewModelPresenterResult? TryShow(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(parentPresenter, nameof(parentPresenter));
            Should.NotBeNull(metadata, nameof(metadata));
            return TryShowInternal(parentPresenter, metadata);
        }

        public IReadOnlyList<IChildViewModelPresenterResult> TryClose(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(parentPresenter, nameof(parentPresenter));
            Should.NotBeNull(metadata, nameof(metadata));
            return TryCloseInternal(parentPresenter, metadata);
        }

        public IChildViewModelPresenterResult? TryRestore(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(parentPresenter, nameof(parentPresenter));
            Should.NotBeNull(metadata, nameof(metadata));
            return TryRestoreInternal(parentPresenter, metadata);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IViewModelPresenter owner, IReadOnlyMetadataContext metadata)
        {
            NavigationDispatcher.AddListener(this);
        }

        protected override void OnDetachedInternal(IViewModelPresenter owner, IReadOnlyMetadataContext metadata)
        {
            NavigationDispatcher.RemoveListener(this);
        }

        protected virtual IChildViewModelPresenterResult? TryShowInternal(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata)
        {
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            if (viewModel == null)
                return null;

            var initializers = ViewManager.GetInitializersByViewModel(viewModel, metadata);
            for (var i = 0; i < initializers.Count; i++)
            {
                var mediator = TryGetMediator(viewModel, initializers[i], metadata);
                if (mediator != null)
                    return ChildViewModelPresenterResult.CreateShowResult(mediator, mediator.NavigationType, mediator.Show(metadata), this, MetadataContextProvider, true);
            }

            return null;
        }

        protected virtual IReadOnlyList<IChildViewModelPresenterResult> TryCloseInternal(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata)
        {
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            var mediators = viewModel?.Metadata.Get(NavigationInternalMetadata.NavigationMediators);
            if (mediators == null || mediators.Count == 0)
                return Default.EmptyArray<IChildViewModelPresenterResult>();

            INavigationMediator[] m;
            lock (mediators)
            {
                m = mediators.ToArray();
            }

            var managers = Managers.GetItems();
            for (var i = 0; i < managers.Length; i++)
            {
                var result = managers[i].TryCloseInternal(this, viewModel!, m, metadata);
                if (result != null)
                    return result;
            }

            var results = new IChildViewModelPresenterResult[mediators.Count];
            for (var i = 0; i < results.Length; i++)
            {
                var mediator = m[i];
                results[i] = new ChildViewModelPresenterResult(mediator, mediator.NavigationType, mediator.Close(metadata), this);
            }

            return results;
        }

        protected virtual IChildViewModelPresenterResult? TryRestoreInternal(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata)
        {
            var viewInfo = metadata.Get(NavigationInternalMetadata.RestoredView);
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            var initializer = viewInfo?.GetInitializer<IViewInitializer>();
            if (viewInfo == null || viewModel == null || initializer == null)
                return null;

            var mediator = TryGetMediator(viewModel, initializer, metadata);
            if (mediator == null)
                return null;

            return new ChildViewModelPresenterResult(mediator, mediator.NavigationType, mediator.Restore(viewInfo, metadata), this);
        }

        protected virtual INavigationMediator? TryGetMediator(IViewModelBase viewModel, IViewInitializer viewInitializer, IReadOnlyMetadataContext metadata)
        {
            var mediators = viewModel.Metadata.GetOrAdd(NavigationInternalMetadata.NavigationMediators, (object?)null, (object?)null,
                (context, o, arg3) => new List<INavigationMediator>())!;

            var managers = Managers.GetItems();
            lock (mediators)
            {
                var mediator = mediators.FirstOrDefault(m => m.ViewInitializer.Id == viewInitializer.Id);
                if (mediator == null)
                {
                    for (var i = 0; i < managers.Length; i++)
                    {
                        mediator = managers[i].TryGetMediator(this, viewModel, viewInitializer, metadata);
                        if (mediator != null)
                        {
                            mediators.Add(mediator);
                            break;
                        }
                    }
                }

                return mediator;
            }
        }

        #endregion
    }
}