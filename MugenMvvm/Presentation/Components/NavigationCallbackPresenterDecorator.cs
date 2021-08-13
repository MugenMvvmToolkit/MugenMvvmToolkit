using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Presentation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Presentation.Components
{
    public sealed class NavigationCallbackPresenterDecorator : ComponentDecoratorBase<IPresenter, IPresenterComponent>, IPresenterComponent
    {
        private readonly INavigationDispatcher? _navigationDispatcher;

        [Preserve(Conditional = true)]
        public NavigationCallbackPresenterDecorator(INavigationDispatcher? navigationDispatcher = null, int priority = PresenterComponentPriority.CallbackDecorator)
            : base(priority)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        public ItemOrIReadOnlyList<IPresenterResult> TryShow(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var dispatcher = _navigationDispatcher.DefaultIfNull();
            using (SuspendNavigation(dispatcher, metadata))
            {
                var results = Components.TryShow(presenter, request, cancellationToken, metadata);
                if (results.Count != 0)
                {
                    var components = dispatcher.GetComponents<INavigationCallbackManagerComponent>(metadata);
                    foreach (var result in results)
                    {
                        components.TryAddNavigationCallback(dispatcher, NavigationCallbackType.Show, result.NavigationId, result.NavigationType, result, metadata);
                        components.TryAddNavigationCallback(dispatcher, NavigationCallbackType.Close, result.NavigationId, result.NavigationType, result, metadata);
                    }
                }

                return results;
            }
        }

        public ItemOrIReadOnlyList<IPresenterResult> TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var dispatcher = _navigationDispatcher.DefaultIfNull();
            using (SuspendNavigation(dispatcher, metadata))
            {
                var results = Components.TryClose(presenter, request, cancellationToken, metadata);
                if (results.Count != 0)
                {
                    var components = dispatcher.GetComponents<INavigationCallbackManagerComponent>(metadata);
                    foreach (var t in results)
                        components.TryAddNavigationCallback(dispatcher, NavigationCallbackType.Closing, t.NavigationId, t.NavigationType, t, metadata);
                }

                return results;
            }
        }

        private ActionToken SuspendNavigation(INavigationDispatcher navigationDispatcher, IReadOnlyMetadataContext? metadata) =>
            navigationDispatcher.GetComponents<ISuspendableComponent<INavigationDispatcher>>(metadata).TrySuspend(navigationDispatcher, metadata);
    }
}