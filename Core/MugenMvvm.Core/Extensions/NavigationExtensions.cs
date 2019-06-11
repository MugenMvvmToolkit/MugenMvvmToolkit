using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Navigation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views.Infrastructure;
using MugenMvvm.Interfaces.Wrapping;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static bool RegisterMediatorFactory(this INavigationMediatorChildViewModelPresenter viewModelPresenter,
            Func<IViewModelBase, IViewInitializer, IReadOnlyMetadataContext, INavigationMediator?> factory, int priority = 0, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModelPresenter, nameof(viewModelPresenter));
            Should.NotBeNull(factory, nameof(factory));
            return viewModelPresenter.Managers.Add(new DelegateNavigationMediatorFactory(factory, priority), metadata);
        }

        public static bool RegisterMediatorFactory<TMediator, TView>(this INavigationMediatorChildViewModelPresenter viewModelPresenter,
            Func<IViewModelBase, IViewInitializer, IReadOnlyMetadataContext, TMediator>? factory = null, int priority = 0,
            IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
            where TMediator : NavigationMediatorBase<TView>
            where TView : class
        {
            return viewModelPresenter.RegisterMediatorFactory(typeof(TMediator), typeof(TView), factory, priority, metadata, wrapperManager);
        }

        public static bool RegisterMediatorFactory(this INavigationMediatorChildViewModelPresenter viewModelPresenter, Type mediatorType, Type viewType,
            Func<IViewModelBase, IViewInitializer, IReadOnlyMetadataContext, INavigationMediator>? factory = null, int priority = 0,
            IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
        {
            Should.NotBeNull(mediatorType, nameof(mediatorType));
            Should.NotBeNull(viewType, nameof(viewType));
            return viewModelPresenter.RegisterMediatorFactory((vm, initializer, arg3) =>
            {
                if (viewType.IsAssignableFromUnified(initializer.ViewType) || wrapperManager.ServiceIfNull().CanWrap(initializer.ViewType, viewType, arg3))
                {
                    if (factory == null)
                        return (INavigationMediator)Service<IServiceProvider>.Instance.GetService(mediatorType);
                    return factory(vm, initializer, arg3);
                }
                return null;
            }, priority, metadata);
        }

        public static INavigatingResult OnNavigatingTo(this INavigationDispatcher navigationDispatcher, INavigationProvider navigationProvider, NavigationMode mode,
            NavigationType navigationType, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            return navigationDispatcher.OnNavigating(navigationDispatcher.ContextProvider.GetNavigationContextTo(navigationProvider, mode, navigationType, viewModel, metadata.DefaultIfNull()));
        }

        public static INavigatingResult OnNavigatingFrom(this INavigationDispatcher navigationDispatcher, INavigationProvider navigationProvider, NavigationMode mode,
            NavigationType navigationType, IViewModelBase viewModelFrom, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            return navigationDispatcher.OnNavigating(navigationDispatcher.ContextProvider.GetNavigationContextFrom(navigationProvider, mode, navigationType, viewModelFrom, metadata.DefaultIfNull()));
        }

        public static Task WaitNavigationAsync(this INavigationDispatcher navigationDispatcher, Func<INavigationCallback, bool> filter,
            IReadOnlyMetadataContext? metadata = null)
        {
            return navigationDispatcher?.NavigationJournal.WaitNavigationAsync(filter, metadata);
        }

        public static Task WaitNavigationAsync(this INavigationDispatcherJournal navigationDispatcherJournal, Func<INavigationCallback, bool> filter,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(navigationDispatcherJournal, nameof(navigationDispatcherJournal));
            Should.NotBeNull(filter, nameof(filter));
            if (metadata == null)
                metadata = Default.Metadata;
            var entries = navigationDispatcherJournal.GetNavigationEntries(null, metadata);
            List<Task>? tasks = null;
            for (var i = 0; i < entries.Count; i++)
            {
                var callbacks = entries[i].GetCallbacks(null, metadata);
                for (var j = 0; j < callbacks.Count; j++)
                {
                    if (tasks == null)
                        tasks = new List<Task>();
                    var callback = callbacks[i];
                    if (filter(callback))
                        tasks.Add(callback.WaitAsync());
                }
            }

            if (tasks == null)
                return Default.CompletedTask;
            return Task.WhenAll(tasks);
        }

        #endregion
    }
}