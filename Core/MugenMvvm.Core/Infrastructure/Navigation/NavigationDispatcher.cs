using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Navigation
{
    public class NavigationDispatcher : HasListenersBase<INavigationDispatcherListener>, INavigationDispatcher
    {
        #region Implementation of interfaces

        public IReadOnlyList<INavigationEntry> GetNavigationEntries(NavigationType? type, IReadOnlyMetadataContext metadata)
        {
            throw new NotImplementedException();
        }

        public Task<bool> OnNavigatingAsync(INavigationContext context)
        {
            throw new NotImplementedException();
        }

        public void OnNavigated(INavigationContext context)
        {
            throw new NotImplementedException();
        }

        public void OnNavigationFailed(INavigationContext context, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void OnNavigationCanceled(INavigationContext context)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Nested types

        protected sealed class WeakNavigationEntry
        {
            #region Fields

            private readonly WeakReference _providerReference;
            private readonly WeakReference _viewModelReference;

            #endregion

            #region Constructors

            public WeakNavigationEntry(IViewModel viewModel, object? provider, NavigationType navigationType)
            {
                NavigationType = navigationType;
                _viewModelReference = MugenExtensions.GetWeakReference(viewModel);
                _providerReference = MugenExtensions.GetWeakReference(provider);
            }

            #endregion

            #region Properties

            public IViewModel? ViewModel => (IViewModel) _viewModelReference.Target;

            public object? NavigationProvider => _providerReference.Target;

            public NavigationType NavigationType { get; }

            #endregion

            #region Methods

            public INavigationEntry? ToNavigationEntry()
            {
                var viewModel = ViewModel;
                var provider = NavigationProvider;
                if (viewModel == null)
                    return null;
                return new NavigationEntry(NavigationType, viewModel, provider);
            }

            #endregion
        }

        protected sealed class NavigationEntry : INavigationEntry
        {
            #region Constructors

            public NavigationEntry(NavigationType type, IViewModel viewModel, object? provider)
            {
                Should.NotBeNull(type, nameof(type));
                Should.NotBeNull(viewModel, nameof(viewModel));
                Type = type;
                ViewModel = viewModel;
                Provider = provider;
            }

            #endregion

            #region Properties

            public NavigationType Type { get; }

            public IViewModel ViewModel { get; }

            public object? Provider { get; }

            #endregion
        }

        #endregion
    }
}