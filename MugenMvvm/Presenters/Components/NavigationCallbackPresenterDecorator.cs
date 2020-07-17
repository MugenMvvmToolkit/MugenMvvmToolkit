﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Attributes;
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
using MugenMvvm.Internal;

namespace MugenMvvm.Presenters.Components
{
    public sealed class NavigationCallbackPresenterDecorator : ComponentDecoratorBase<IPresenter, IPresenterComponent>, IPresenterComponent, IHasPriority
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationCallbackPresenterDecorator(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = PresenterComponentPriority.CallbackDecorator;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryShow(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var dispatcher = _navigationDispatcher.DefaultIfNull();
            using (SuspendNavigation(dispatcher, metadata))
            {
                var results = Components.TryShow(presenter, request, cancellationToken, metadata);
                var iterator = results.Iterator();
                if (iterator.Count != 0)
                {
                    var components = dispatcher.GetComponents<INavigationCallbackManagerComponent>(metadata);
                    foreach (var result in iterator)
                    {
                        components.TryAddNavigationCallback(dispatcher, NavigationCallbackType.Showing, result, metadata);
                        components.TryAddNavigationCallback(dispatcher, NavigationCallbackType.Close, result, metadata);
                    }
                }

                return results;
            }
        }

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var dispatcher = _navigationDispatcher.DefaultIfNull();
            using (SuspendNavigation(dispatcher, metadata))
            {
                var results = Components.TryClose(presenter, request, cancellationToken, metadata);
                var iterator = results.Iterator();
                if (iterator.Count != 0)
                {
                    var components = dispatcher.GetComponents<INavigationCallbackManagerComponent>(metadata);
                    foreach (var t in iterator)
                        components.TryAddNavigationCallback(dispatcher, NavigationCallbackType.Closing, t, metadata);
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