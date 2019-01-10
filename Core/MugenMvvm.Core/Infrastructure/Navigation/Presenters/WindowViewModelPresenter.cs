using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views.Infrastructure;
using MugenMvvm.Interfaces.Wrapping;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class WindowViewModelPresenter : IRestorableChildViewModelPresenter
    {
        #region Fields

        private readonly OrderedListInternal<MediatorRegistration> _mediatorRegistrations;

        #endregion

        #region Constructors

        public WindowViewModelPresenter(IServiceProvider serviceProvider, IViewMappingProvider viewMappingProvider, IWrapperManager wrapperManager)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            Should.NotBeNull(viewMappingProvider, nameof(viewMappingProvider));
            ServiceProvider = serviceProvider;
            ViewMappingProvider = viewMappingProvider;
            WrapperManager = wrapperManager;
            _mediatorRegistrations = new OrderedListInternal<MediatorRegistration>();
        }

        #endregion

        #region Properties

        protected IWrapperManager WrapperManager { get; }

        protected IServiceProvider ServiceProvider { get; }

        protected IViewMappingProvider ViewMappingProvider { get; }

        public virtual int Priority => ViewModelPresenter.WindowPresenterPriority;

        #endregion

        #region Implementation of interfaces

        public IChildViewModelPresenterResult? TryShow(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            Should.NotBeNull(parentPresenter, nameof(parentPresenter));
            return TryShowInternal(metadata, parentPresenter);
        }

        public IChildViewModelPresenterResult? TryClose(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            Should.NotBeNull(parentPresenter, nameof(parentPresenter));
            return TryCloseInternal(metadata, parentPresenter);
        }

        public IChildViewModelPresenterResult? TryRestore(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            Should.NotBeNull(parentPresenter, nameof(parentPresenter));
            return TryRestoreInternal(metadata, parentPresenter);
        }

        #endregion

        #region Methods

        public void RegisterMediatorFactory<TMediator, TView>(bool viewExactlyEqual = false, int priority = 0)
            where TMediator : NavigationWindowMediatorBase<TView>
            where TView : class
        {
            RegisterMediatorFactory(typeof(TMediator), typeof(TView), viewExactlyEqual, priority);
        }

        public void RegisterMediatorFactory(Type mediatorType, Type viewType, bool viewExactlyEqual, int priority = 0)
        {
            Should.NotBeNull(viewType, nameof(viewType));
            if (viewExactlyEqual)
            {
                RegisterMediatorFactory((vm, type, arg3) =>
                {
                    if (type == viewType)
                        return (INavigationWindowMediator)ServiceProvider.GetService(mediatorType);
                    return null;
                }, priority);
            }
            else
            {
                RegisterMediatorFactory((vm, type, arg3) =>
                {
                    if (viewType.IsAssignableFromUnified(type) || WrapperManager.CanWrap(type, viewType, arg3))
                        return (INavigationWindowMediator)ServiceProvider.GetService(mediatorType);
                    return null;
                }, priority);
            }
        }

        public void RegisterMediatorFactory(Func<IViewModel, Type, IReadOnlyMetadataContext, INavigationWindowMediator> mediatorFactory, int priority = 0)
        {
            Should.NotBeNull(mediatorFactory, nameof(mediatorFactory));
            lock (_mediatorRegistrations)
            {
                _mediatorRegistrations.Add(new MediatorRegistration(priority, mediatorFactory));
            }
        }

        protected virtual IChildViewModelPresenterResult? TryShowInternal(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter)
        {
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            if (viewModel == null)
                return null;
            var mediator = TryCreateMediator(viewModel, metadata);
            if (mediator == null)
                return null;
            return ChildViewModelPresenterResult.CreateShowResult(mediator.NavigationType, mediator.Show(metadata), true);
        }

        protected virtual IChildViewModelPresenterResult? TryCloseInternal(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter)
        {
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            var mediator = viewModel?.Metadata.Get(NavigationInternalMetadata.NavigationWindowMediator);
            if (mediator == null)
                return null;
            return new ChildViewModelPresenterResult(mediator.Close(metadata), mediator.NavigationType);
        }

        protected virtual IChildViewModelPresenterResult? TryRestoreInternal(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter)
        {
            var view = metadata.Get(NavigationInternalMetadata.RestoredWindowView);
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            if (view == null || viewModel == null)
                return null;

            var mediator = TryCreateMediator(viewModel, metadata);
            if (mediator == null)
                return null;
            return new ChildViewModelPresenterResult(mediator.Restore(view, metadata), mediator.NavigationType);
        }

        protected virtual INavigationWindowMediator? CreateWindowViewMediator(IViewModel viewModel, Type viewType, IReadOnlyMetadataContext metadata)
        {
            return TryCreateMediatorFromFactory(viewModel, viewType, metadata);
        }

        protected INavigationWindowMediator? TryCreateMediatorFromFactory(IViewModel viewModel, Type viewType, IReadOnlyMetadataContext metadata)
        {
            lock (_mediatorRegistrations)
            {
                for (var i = 0; i < _mediatorRegistrations.Count; i++)
                {
                    var mediator = _mediatorRegistrations[i].Factory.Invoke(viewModel, viewType, metadata);
                    if (mediator != null)
                        return mediator;
                }
            }

            return null;
        }

        private INavigationWindowMediator? TryCreateMediator(IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            if (metadata.Get(NavigationMetadata.SuppressWindowNavigation))
                return null;

            var mappingInfo = ViewMappingProvider.TryGetMappingByViewModel(viewModel.GetType(), metadata);
            if (mappingInfo == null)
                return null;

            return viewModel.Metadata.GetOrAdd(NavigationInternalMetadata.NavigationWindowMediator, mappingInfo, this, (context, info, @this) =>
            {
                var viewMediator = @this.CreateWindowViewMediator(viewModel, info.ViewType, metadata);
                viewMediator?.Initialize(viewModel, metadata);
                return viewMediator;
            });
        }

        #endregion

        #region Nested types

        private sealed class MediatorRegistration : IComparable<MediatorRegistration>
        {
            #region Fields

            private readonly int _priority;
            public readonly Func<IViewModel, Type, IReadOnlyMetadataContext, INavigationWindowMediator> Factory;

            #endregion

            #region Constructors

            public MediatorRegistration(int priority, Func<IViewModel, Type, IReadOnlyMetadataContext, INavigationWindowMediator> factory)
            {
                _priority = priority;
                Factory = factory;
            }

            #endregion

            #region Implementation of interfaces

            public int CompareTo(MediatorRegistration other)
            {
                if (ReferenceEquals(this, other))
                    return 0;
                if (ReferenceEquals(null, other))
                    return 1;
                return other._priority.CompareTo(_priority);
            }

            #endregion
        }

        #endregion
    }
}