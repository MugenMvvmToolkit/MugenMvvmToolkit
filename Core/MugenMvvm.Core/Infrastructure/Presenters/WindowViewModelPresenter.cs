using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Infrastructure.Navigation;
using MugenMvvm.Infrastructure.Presenters.Results;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Results;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views.Infrastructure;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Presenters
{
    public class WindowViewModelPresenter : IRestorableChildViewModelPresenter
    {
        #region Fields

        private readonly OrderedListInternal<MediatorRegistration> _mediatorRegistrations;

        #endregion

        #region Constructors

        public WindowViewModelPresenter(IWrapperManager wrapperManager, IServiceProvider serviceProvider, IViewMappingProvider viewMappingProvider)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            Should.NotBeNull(viewMappingProvider, nameof(viewMappingProvider));
            WrapperManager = wrapperManager;
            ServiceProvider = serviceProvider;
            ViewMappingProvider = viewMappingProvider;
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
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            if (viewModel == null)
                return null;
            var mediator = TryCreateMediator(viewModel, metadata);
            if (mediator == null)
                return null;
            parentPresenter
                .WaitOpenNavigationAsync(NavigationType.Window, metadata)
                .ContinueWith(task => mediator.Show(metadata), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
            return new ChildViewModelPresenterResult(metadata, NavigationType.Window, true);
        }

        public IClosingViewModelPresenterResult? TryClose(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            Should.NotBeNull(parentPresenter, nameof(parentPresenter));
            return TryCloseInternal(metadata, parentPresenter);
        }

        public IRestorationViewModelPresenterResult? TryRestore(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter)
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

        protected virtual INavigationWindowMediator? CreateWindowViewMediator(IViewModel viewModel, Type viewType, IReadOnlyMetadataContext metadata)
        {
            return TryCreateMediatorFromFactory(viewModel, viewType, metadata);
        }

        protected virtual IClosingViewModelPresenterResult? TryCloseInternal(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter)
        {
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            var mediator = viewModel?.Metadata.Get(NavigationMetadata.NavigationWindowMediator);
            if (mediator == null)
                return null;
            return new ClosingViewModelPresenterResult(metadata, mediator.CloseAsync(metadata));
        }

        protected virtual IRestorationViewModelPresenterResult? TryRestoreInternal(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter)
        {
            var view = metadata.Get(NavigationMetadata.RestoredWindowView);
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            if (view == null || viewModel == null)
                return null;

            var mediator = TryCreateMediator(viewModel, metadata);
            if (mediator == null)
                return null;

            mediator.UpdateView(view, true, metadata);
            return new RestorationViewModelPresenterResult(metadata, true);
        }

        protected INavigationWindowMediator TryCreateMediatorFromFactory(IViewModel viewModel, Type viewType, IReadOnlyMetadataContext metadata)
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

        private INavigationWindowMediator TryCreateMediator(IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            if (metadata.Get(NavigationMetadata.SuppressWindowNavigation))
                return null;

            if (!ViewMappingProvider.TryGetMappingByViewModel(viewModel.GetType(), viewModel.GetViewName(metadata), out var mappingInfo))
                return null;

            return viewModel.Metadata.GetOrAdd(NavigationMetadata.NavigationWindowMediator, mappingInfo, this, (context, info, @this) =>
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