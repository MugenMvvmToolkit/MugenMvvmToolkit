using System;
using System.Linq;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views.Infrastructure;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    //todo fix view check, fix Window -> Popup, Dialog?, multi close check,
    //todo review interface INavigationMediatorFactory, remove IWrapperManager, IServiceProvider
    //todo IHasPriority merge
    public class NavigationMediatorViewModelPresenter : IRestorableChildViewModelPresenter, IComparer<NavigationMediatorViewModelPresenter.INavigationMediatorFactory>
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationMediatorViewModelPresenter(IServiceProvider serviceProvider, IViewManager viewManager, IWrapperManager wrapperManager)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            Should.NotBeNull(viewManager, nameof(viewManager));
            ServiceProvider = serviceProvider;
            ViewManager = viewManager;
            WrapperManager = wrapperManager;
            MediatorRegistrations = new OrderedListInternal<INavigationMediatorFactory>(this);
        }

        #endregion

        #region Properties

        protected ICollection<INavigationMediatorFactory> MediatorRegistrations { get; }

        protected IWrapperManager WrapperManager { get; }

        protected IServiceProvider ServiceProvider { get; }

        protected IViewManager ViewManager { get; }

        public virtual int Priority => PresenterConstants.GenericNavigationPriority;

        #endregion

        #region Implementation of interfaces

        int IComparer<INavigationMediatorFactory>.Compare(INavigationMediatorFactory x, INavigationMediatorFactory y)
        {
            return y.Priority.CompareTo(x.Priority);
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

        public void RegisterMediatorFactory<TMediator, TView>(bool disableWrap = false, int priority = 0)
            where TMediator : NavigationMediatorBase<TView>
            where TView : class
        {
            RegisterMediatorFactory(typeof(TMediator), typeof(TView), disableWrap, priority);
        }

        public void RegisterMediatorFactory(Type mediatorType, Type viewType, bool disableWrap, int priority = 0)
        {
            Should.NotBeNull(viewType, nameof(viewType));
            if (disableWrap)
            {
                RegisterMediatorFactory(new DelegateMediatorRegistration((vm, initializer, arg3) =>
                {
                    if (initializer.ViewType.EqualsEx(viewType))
                        return (INavigationMediator)ServiceProvider.GetService(mediatorType);
                    return null;
                }, priority));
            }
            else
            {
                RegisterMediatorFactory(new DelegateMediatorRegistration((vm, initializer, arg3) =>
                {
                    if (viewType.IsAssignableFromUnified(initializer.ViewType) || WrapperManager.CanWrap(initializer.ViewType, viewType, arg3))
                        return (INavigationMediator)ServiceProvider.GetService(mediatorType);
                    return null;
                }, priority));
            }
        }

        public void RegisterMediatorFactory(INavigationMediatorFactory mediatorFactory)
        {
            Should.NotBeNull(mediatorFactory, nameof(mediatorFactory));
            lock (MediatorRegistrations)
            {
                MediatorRegistrations.Add(mediatorFactory);
            }
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
                    return ChildViewModelPresenterResult.CreateShowResult(mediator, mediator.NavigationType, mediator.Show(metadata), this, true);
            }

            return null;
        }

        protected virtual IReadOnlyList<IChildViewModelPresenterResult> TryCloseInternal(IViewModelPresenter parentPresenter, IReadOnlyMetadataContext metadata)
        {
            var viewModel = metadata.Get(NavigationMetadata.ViewModel);
            var mediators = viewModel?.Metadata.Get(NavigationInternalMetadata.NavigationMediators);
            if (mediators == null || mediators.Count == 0)
                return Default.EmptyArray<IChildViewModelPresenterResult>();

            if (mediators.Count == 1)
            {
                var mediator = mediators[0];
                return new IChildViewModelPresenterResult[] {new ChildViewModelPresenterResult(mediator, mediator.NavigationType, mediator.Close(metadata), this)};
            }

            if (metadata.Get(NavigationInternalMetadata.CloseAll))
            {
                var results = new IChildViewModelPresenterResult[mediators.Count];
                for (int i = 0; i < results.Length; i++)
                {
                    var mediator = mediators[i];
                    results[i] = new ChildViewModelPresenterResult(mediator, mediator.NavigationType, mediator.Close(metadata), this);
                }

                return results;
            }

            var initializers = ViewManager.GetInitializersByViewModel(viewModel, metadata);
            if (initializers.Count == 0)
                return Default.EmptyArray<IChildViewModelPresenterResult>();

            var r = new List<IChildViewModelPresenterResult>();
            foreach (var initializer in initializers)
            {
                foreach (var mediator in mediators)
                {
                    if (r.Any(result => result.NavigationProvider.Id == mediator.Id))
                        continue;

                    if (initializer.Id == mediator.ViewInitializer.Id)
                        r.Add(new ChildViewModelPresenterResult(mediator, mediator.NavigationType, mediator.Close(metadata), this));
                }
            }

            return r;
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
                (context, o, arg3) => new List<INavigationMediator>());
            lock (mediators)
            {
                var mediator = mediators.FirstOrDefault(m => m.ViewInitializer.Id == viewInitializer.Id);
                if (mediator == null)
                {
                    lock (MediatorRegistrations)
                    {
                        foreach (var factory in MediatorRegistrations)
                        {
                            mediator = factory.GetMediator(viewModel, viewInitializer, metadata);
                            if (mediator != null)
                                break;
                        }
                    }
                }
            }


            return null;
        }

        #endregion

        #region Nested types

        public interface INavigationMediatorFactory
        {
            int Priority { get; }

            INavigationMediator GetMediator(IViewModelBase viewModel, IViewInitializer viewInitializer, IReadOnlyMetadataContext metadata);
        }

        private sealed class DelegateMediatorRegistration : INavigationMediatorFactory
        {
            #region Fields

            private readonly Func<IViewModelBase, IViewInitializer, IReadOnlyMetadataContext, INavigationMediator> _factory;

            #endregion

            #region Constructors

            public DelegateMediatorRegistration(Func<IViewModelBase, IViewInitializer, IReadOnlyMetadataContext, INavigationMediator> factory, int priority)
            {
                _factory = factory;
                Priority = priority;
            }

            #endregion

            #region Properties

            public int Priority { get; }

            #endregion

            #region Implementation of interfaces

            public INavigationMediator GetMediator(IViewModelBase viewModel, IViewInitializer viewInitializer, IReadOnlyMetadataContext metadata)
            {
                return _factory(viewModel, viewInitializer, metadata);
            }

            #endregion
        }

        #endregion
    }
}