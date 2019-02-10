//using System;
//using MugenMvvm.Collections;
//using MugenMvvm.Interfaces.Metadata;
//using MugenMvvm.Interfaces.Navigation;
//using MugenMvvm.Interfaces.Navigation.Presenters;
//using MugenMvvm.Interfaces.ViewModels;
//using MugenMvvm.Interfaces.Views.Infrastructure;
//using MugenMvvm.Interfaces.Wrapping;
//
//namespace MugenMvvm.Infrastructure.Navigation.Presenters
//{
//    public class WindowViewModelPresenter : IRestorableChildViewModelPresenter //todo fix view check, fix Window -> Popup, Dialog?
//    {
//        #region Constructors
//
//        public WindowViewModelPresenter(IServiceProvider serviceProvider, IViewManager viewManager, IWrapperManager wrapperManager)
//        {
//            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
//            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
//            Should.NotBeNull(viewManager, nameof(viewManager));
//            ServiceProvider = serviceProvider;
//            ViewManager = viewManager;
//            WrapperManager = wrapperManager;
//            MediatorRegistrations = new OrderedListInternal<INavigationWindowMediatorFactory>(this);
//        }
//
//        #endregion
//
//        #region Properties
//
//        protected OrderedListInternal<INavigationWindowMediatorFactory> MediatorRegistrations { get; }
//
//        protected IWrapperManager WrapperManager { get; }
//
//        protected IServiceProvider ServiceProvider { get; }
//
//        protected IViewManager ViewManager { get; }
//
//        public virtual int Priority => ViewModelPresenter.WindowPresenterPriority;
//
//        #endregion
//
//        #region Implementation of interfaces
//
//        public IChildViewModelPresenterResult? TryShow(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata)
//        {
//            Should.NotBeNull(metadata, nameof(metadata));
//            Should.NotBeNull(parentPresenter, nameof(parentPresenter));
//            return TryShowInternal(metadata, parentPresenter);
//        }
//
//        public IChildViewModelPresenterResult? TryClose(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata)
//        {
//            Should.NotBeNull(metadata, nameof(metadata));
//            Should.NotBeNull(parentPresenter, nameof(parentPresenter));
//            return TryCloseInternal(metadata, parentPresenter);
//        }
//
//        public IChildViewModelPresenterResult? TryRestore(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata)
//        {
//            Should.NotBeNull(metadata, nameof(metadata));
//            Should.NotBeNull(parentPresenter, nameof(parentPresenter));
//            return TryRestoreInternal(metadata, parentPresenter);
//        }
//
//        #endregion
//
//        #region Methods
//
//        public void RegisterMediatorFactory<TMediator, TView>(bool disableWrap = false, int priority = 0)
//            where TMediator : NavigationWindowMediatorBase<TView>
//            where TView : class
//        {
//            RegisterMediatorFactory(typeof(TMediator), typeof(TView), disableWrap, priority);
//        }
//
//        public void RegisterMediatorFactory(Type mediatorType, Type viewType, bool disableWrap, int priority = 0)
//        {
//            Should.NotBeNull(viewType, nameof(viewType));
//            if (disableWrap)
//            {
//                RegisterMediatorFactory((vm, type, arg3) =>
//                {
//                    if (type == viewType)
//                        return (INavigationWindowMediator)ServiceProvider.GetService(mediatorType);
//                    return null;
//                }, priority);
//            }
//            else
//            {
//                RegisterMediatorFactory((vm, type, arg3) =>
//                {
//                    if (viewType.IsAssignableFromUnified(type) || WrapperManager.CanWrap(type, viewType, arg3))
//                        return (INavigationWindowMediator)ServiceProvider.GetService(mediatorType);
//                    return null;
//                }, priority);
//            }
//        }
//
//        public void RegisterMediatorFactory(INavigationWindowMediatorFactory mediatorFactory)
//        {
//            Should.NotBeNull(mediatorFactory, nameof(mediatorFactory));
//            lock (MediatorRegistrations)
//            {
//                MediatorRegistrations.Add(mediatorFactory);
//            }
//        }
//
//        protected virtual IChildViewModelPresenterResult? TryShowInternal(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter)
//        {
//            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
//            if (viewModel == null)
//                return null;
//            var mediator = TryCreateMediator(viewModel, metadata);
//            if (mediator == null)
//                return null;
//            return ChildViewModelPresenterResult.CreateShowResult(mediator.NavigationType, mediator.Show(metadata), true);
//        }
//
//        protected virtual IChildViewModelPresenterResult? TryCloseInternal(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter)
//        {
//            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
//            var mediator = viewModel?.Metadata.Get(NavigationInternalMetadata.NavigationWindowMediator);
//            if (mediator == null)
//                return null;
//            return new ChildViewModelPresenterResult(mediator.Close(metadata), mediator.NavigationType);
//        }
//
//        protected virtual IChildViewModelPresenterResult? TryRestoreInternal(IReadOnlyMetadataContext metadata, IViewModelPresenter parentPresenter)
//        {
//            var view = metadata.Get(NavigationInternalMetadata.RestoredWindowView);
//            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
//            if (view == null || viewModel == null)
//                return null;
//
//            var mediator = TryCreateMediator(viewModel, metadata);
//            if (mediator == null)
//                return null;
//            return new ChildViewModelPresenterResult(mediator.Restore(view, metadata), mediator.NavigationType);
//        }
//
//        protected virtual INavigationWindowMediator? CreateWindowViewMediator(IViewModel viewModel, IViewInitializer viewInitializer, IReadOnlyMetadataContext metadata)
//        {
//            lock (MediatorRegistrations)
//            {
//                for (var i = 0; i < MediatorRegistrations.Count; i++)
//                {
//                    var mediator = MediatorRegistrations[i].GetMediator(viewModel, viewInitializer, metadata);
//                    if (mediator != null)
//                        return mediator;
//                }
//            }
//
//            return null;
//        }
//
//        private INavigationWindowMediator? TryCreateMediator(IViewModel viewModel, IReadOnlyMetadataContext metadata)
//        {
//            if (metadata.Get(NavigationMetadata.SuppressWindowNavigation))
//                return null;
//
//            var mappingInfo = ViewMappingProvider.TryGetMappingByViewModel(viewModel.GetType(), metadata);
//            if (mappingInfo == null)
//                return null;
//
//            return viewModel.Metadata.GetOrAdd(NavigationInternalMetadata.NavigationWindowMediator, mappingInfo, this, (context, info, @this) =>
//            {
//                var viewMediator = @this.CreateWindowViewMediator(viewModel, info.ViewType, metadata);
//                viewMediator?.Initialize(viewModel, metadata);
//                return viewMediator;
//            });
//        }
//
//        #endregion
//
//        #region Nested types
//
//        public interface INavigationWindowMediatorFactory
//        {
//            int Priority { get; }
//
//            INavigationWindowMediator GetMediator(IViewModel viewModel, IViewInitializer viewInitializer, IReadOnlyMetadataContext metadata);
//        }
//
//        private sealed class DelegateMediatorRegistration : INavigationWindowMediatorFactory
//        {
//            #region Fields
//
//            private readonly Func<IViewModel, IViewInitializer, IReadOnlyMetadataContext, INavigationWindowMediator> _factory;
//
//            #endregion
//
//            #region Constructors
//
//            public DelegateMediatorRegistration(int priority, Func<IViewModel, IViewInitializer, IReadOnlyMetadataContext, INavigationWindowMediator> factory)
//            {
//                Priority = priority;
//                _factory = factory;
//            }
//
//            #endregion
//
//            #region Properties
//
//            public int Priority { get; }
//
//            #endregion
//
//            #region Implementation of interfaces
//
//            public INavigationWindowMediator GetMediator(IViewModel viewModel, IViewInitializer viewInitializer, IReadOnlyMetadataContext metadata)
//            {
//                return _factory(viewModel, viewInitializer, metadata);
//            }
//
//            #endregion
//        }
//
//        #endregion
//    }
//}