using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;

namespace MugenMvvm.Presenters.Components
{
    public sealed class NavigationCallbackPresenterDecorator : ComponentDecoratorBase<IPresenter, IPresenterComponent>, IPresenterComponent, IHasPriority
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;

        #endregion

        #region Constructors

        public NavigationCallbackPresenterDecorator(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Decorator;

        #endregion

        #region Implementation of interfaces

        public IPresenterResult? TryShow<TRequest>([DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var dispatcher = _navigationDispatcher.DefaultIfNull();
            using (SuspendNavigation(dispatcher, metadata))
            {
                var result = Components.TryShow(request, cancellationToken, metadata);
                if (result != null)
                {
                    var components = dispatcher.GetComponents<INavigationCallbackManagerComponent>(metadata);
                    components.TryAddNavigationCallback(NavigationCallbackType.Showing, result, metadata);
                    components.TryAddNavigationCallback(NavigationCallbackType.Close, result, metadata);
                }

                return result;
            }
        }

        public IReadOnlyList<IPresenterResult>? TryClose<TRequest>([DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var dispatcher = _navigationDispatcher.DefaultIfNull();
            using (SuspendNavigation(dispatcher, metadata))
            {
                var results = Components.TryClose(request, cancellationToken, metadata);
                if (results != null && results.Count != 0)
                {
                    var components = dispatcher.GetComponents<INavigationCallbackManagerComponent>(metadata);
                    for (var i = 0; i < results.Count; i++)
                        components.TryAddNavigationCallback(NavigationCallbackType.Closing, results[i], metadata);
                }

                return results;
            }
        }

        public IReadOnlyList<IPresenterResult>? TryRestore<TRequest>([DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return Components.TryRestore(request, cancellationToken, metadata);
        }

        #endregion

        #region Methods

        private ActionToken SuspendNavigation(INavigationDispatcher navigationDispatcher, IReadOnlyMetadataContext? metadata)
        {
            return navigationDispatcher.GetComponents<ISuspendable>().Suspend(Owner, metadata);
        }

        #endregion
    }
}