using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Metadata;

namespace MugenMvvm.Presenters.Components
{
    public sealed class ViewModelMediatorPresenterComponent : AttachableComponentBase<IPresenter>, ICloseablePresenterComponent, IRestorablePresenterComponent, IPresenterComponent,
        INavigationDispatcherNavigatedListener, IHasComponentPriority, IHasPriority
    {
        #region Fields

        private readonly IViewManager? _viewManager;
        private INavigationDispatcher? _navigationDispatcher;

        private static readonly IMetadataContextKey<List<IViewModelPresenterMediator>?> NavigationMediators = MetadataContextKey
            .Create<List<IViewModelPresenterMediator>?>(typeof(ViewModelMediatorPresenterComponent), nameof(NavigationMediators))
            .NotNull()
            .Build();

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelMediatorPresenterComponent(IViewManager? viewManager = null, INavigationDispatcher? navigationDispatcher = null)
        {
            _viewManager = viewManager;
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public int NavigationDispatcherListenerPriority { get; set; }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IPresenterResult> TryClose(IMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            var mediators = viewModel?.Metadata.Get(NavigationMediators);
            if (mediators == null || mediators.Count == 0)
                return Default.EmptyArray<IPresenterResult>();

            IViewModelPresenterMediator[] m;
            lock (mediators)
            {
                m = mediators.ToArray();
            }

            var managers = Owner.GetComponents();
            for (var i = 0; i < managers.Length; i++)
            {
                var result = (managers[i] as IViewModelMediatorCloseManagerComponent)?.TryCloseInternal(viewModel!, m, metadata);
                if (result != null)
                    return result;
            }

            var results = new IPresenterResult[mediators.Count];
            for (var i = 0; i < results.Length; i++)
                results[i] = m[i].Close(metadata);
            return results;
        }

        int IHasComponentPriority.GetPriority(object owner)
        {
            return owner is INavigationDispatcher ? NavigationDispatcherListenerPriority : Priority;
        }

        void INavigationDispatcherNavigatedListener.OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            if (!navigationContext.NavigationMode.IsClose || !(navigationContext.NavigationProvider is IViewModelPresenterMediator mediator))
                return;

            var mediators = mediator.ViewModel.Metadata.Get(NavigationMediators);
            if (mediators == null)
                return;

            lock (mediators)
            {
                mediators.Remove(mediator);
            }
        }

        public IPresenterResult? TryShow(IMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            if (viewModel == null)
                return null;

            var initializers = _viewManager.DefaultIfNull().GetInitializersByViewModel(viewModel, metadata);
            for (var i = 0; i < initializers.Count; i++)
            {
                var mediator = TryGetMediator(viewModel, initializers[i], metadata);
                if (mediator != null)
                    return mediator.Show(metadata);
            }

            return null;
        }

        public IReadOnlyList<IPresenterResult> TryRestore(IMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            var viewInfo = metadata.Get(NavigationInternalMetadata.RestoredView);
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            if (viewInfo == null || viewModel == null)
                return Default.EmptyArray<IPresenterResult>();

            var mediator = TryGetMediator(viewModel, viewInfo.Initializer, metadata);
            if (mediator == null)
                return Default.EmptyArray<IPresenterResult>();

            return new[] {mediator.Restore(viewInfo, metadata)};
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IPresenter owner, IReadOnlyMetadataContext? metadata)
        {
            if (_navigationDispatcher == null)
                _navigationDispatcher = MugenService.NavigationDispatcher;
            _navigationDispatcher.AddComponent(this);
        }

        protected override void OnDetachedInternal(IPresenter owner, IReadOnlyMetadataContext? metadata)
        {
            _navigationDispatcher?.RemoveComponent(this);
            _navigationDispatcher = null;
        }

        private IViewModelPresenterMediator? TryGetMediator(IViewModelBase viewModel, IViewInitializer viewInitializer, IMetadataContext metadata)
        {
            var mediators = viewModel.Metadata.GetOrAdd(NavigationMediators, (object?) null, (context, _) => new List<IViewModelPresenterMediator>())!;
            var components = Owner.GetComponents();
            lock (mediators)
            {
                var mediator = mediators.FirstOrDefault(m => m.ViewInitializer.Id == viewInitializer.Id);
                if (mediator == null)
                {
                    for (var i = 0; i < components.Length; i++)
                    {
                        mediator = (components[i] as IViewModelMediatorProviderComponent)?.TryGetMediator(viewModel, viewInitializer, metadata)!;
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