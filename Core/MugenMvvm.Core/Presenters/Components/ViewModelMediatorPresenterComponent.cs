using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Metadata;

namespace MugenMvvm.Presenters.Components
{
    public class ViewModelMediatorPresenterComponent : AttachableComponentBase<IPresenter>, ICloseablePresenterComponent, IRestorablePresenterComponent, IPresenterComponent,
        INavigationDispatcherNavigatedListener
    {
        #region Fields

        protected static readonly IMetadataContextKey<List<IViewModelPresenterMediator>?> NavigationMediators = MetadataContextKey
            .Create<List<IViewModelPresenterMediator>?>(typeof(ViewModelMediatorPresenterComponent), nameof(NavigationMediators))
            .NotNull()
            .Build();

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelMediatorPresenterComponent(IViewManager viewManager, INavigationDispatcher navigationDispatcher)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            ViewManager = viewManager;
            NavigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        protected IViewManager ViewManager { get; }

        protected INavigationDispatcher NavigationDispatcher { get; }

        public int Priority { get; set; }

        public int NavigationDispatcherListenerPriority { get; set; }

        #endregion

        #region Implementation of interfaces

        public int GetPriority(object source)
        {
            if (source is INavigationDispatcher)
                return NavigationDispatcherListenerPriority;
            return Priority;
        }

        public IReadOnlyList<IPresenterResult> TryClose(IMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return TryCloseInternal(metadata);
        }

        void INavigationDispatcherNavigatedListener.OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            if (navigationContext.NavigationMode.IsClose && navigationContext.NavigationProvider is IViewModelPresenterMediator mediator)
            {
                var mediators = mediator.ViewModel.Metadata.Get(NavigationMediators);
                if (mediators == null)
                    return;
                lock (mediators)
                {
                    mediators.Remove(mediator);
                }
            }
        }

        public IPresenterResult? TryShow(IMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return TryShowInternal(metadata);
        }

        public IReadOnlyList<IPresenterResult> TryRestore(IMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return TryRestoreInternal(metadata);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IPresenter owner, IReadOnlyMetadataContext? metadata)
        {
            NavigationDispatcher.AddComponent(this);
        }

        protected override void OnDetachedInternal(IPresenter owner, IReadOnlyMetadataContext? metadata)
        {
            NavigationDispatcher.RemoveComponent(this);
        }

        protected virtual IPresenterResult? TryShowInternal(IMetadataContext metadata)
        {
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            if (viewModel == null)
                return null;

            var initializers = ViewManager.GetInitializersByViewModel(viewModel, metadata);
            for (var i = 0; i < initializers.Count; i++)
            {
                var mediator = TryGetMediator(viewModel, initializers[i], metadata);
                if (mediator != null)
                    return mediator.Show(metadata);
            }

            return null;
        }

        protected virtual IReadOnlyList<IPresenterResult> TryCloseInternal(IMetadataContext metadata)
        {
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
                var result = (managers[i] as IViewModelMediatorClosingManagerComponent)?.TryCloseInternal(viewModel!, m, metadata);
                if (result != null)
                    return result;
            }

            var results = new IPresenterResult[mediators.Count];
            for (var i = 0; i < results.Length; i++)
                results[i] = m[i].Close(metadata);
            return results;
        }

        protected virtual IReadOnlyList<IPresenterResult> TryRestoreInternal(IMetadataContext metadata)
        {
            var viewInfo = metadata.Get(NavigationInternalMetadata.RestoredView);
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            if (viewInfo == null || viewModel == null)
                return Default.EmptyArray<IPresenterResult>();

            var mediator = TryGetMediator(viewModel, viewInfo.Initializer, metadata);
            if (mediator == null)
                return Default.EmptyArray<IPresenterResult>();

            return new[] { mediator.Restore(viewInfo, metadata) };
        }

        protected virtual IViewModelPresenterMediator? TryGetMediator(IViewModelBase viewModel, IViewInitializer viewInitializer, IMetadataContext metadata)
        {
            var mediators = viewModel.Metadata.GetOrAdd(NavigationMediators, (object?)null, (object?)null,
                (context, o, arg3) => new List<IViewModelPresenterMediator>())!;

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