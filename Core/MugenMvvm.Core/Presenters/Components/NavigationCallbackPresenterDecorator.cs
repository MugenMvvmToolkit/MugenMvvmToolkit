using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Extensions.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Internal;

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

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryShow<TRequest>([DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var dispatcher = _navigationDispatcher.DefaultIfNull();
            using (SuspendNavigation(dispatcher, metadata))
            {
                var results = Components.TryShow(request, cancellationToken, metadata);
                if (results.Count() != 0)
                {
                    var components = dispatcher.GetComponents<INavigationCallbackManagerComponent>(metadata);
                    for (var i = 0; i < results.Count(); i++)
                    {
                        var result = results.Get(i);
                        components.TryAddNavigationCallback(NavigationCallbackType.Showing, result, metadata);
                        components.TryAddNavigationCallback(NavigationCallbackType.Close, result, metadata);
                    }
                }

                return results;
            }
        }

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose<TRequest>([DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var dispatcher = _navigationDispatcher.DefaultIfNull();
            using (SuspendNavigation(dispatcher, metadata))
            {
                var results = Components.TryClose(request, cancellationToken, metadata);
                if (results.Count() != 0)
                {
                    var components = dispatcher.GetComponents<INavigationCallbackManagerComponent>(metadata);
                    for (var i = 0; i < results.Count(); i++)
                        components.TryAddNavigationCallback(NavigationCallbackType.Closing, results.Get(i), metadata);
                }

                return results;
            }
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