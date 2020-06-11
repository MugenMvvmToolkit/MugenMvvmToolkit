using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Requests;

namespace MugenMvvm.Presenters.Components
{
    public sealed class ViewModelMediatorPresenter : IPresenterComponent, IHasPriority
    {
        #region Fields

        private readonly List<(Type mediatorType, Type viewType, bool viewExactlyEqual)> _mediators;
        private readonly RemoveHandler _removeHandlerFalse;
        private readonly RemoveHandler _removeHandlerTrue;
        private readonly IServiceProvider? _serviceProvider;
        private readonly IViewManager? _viewManager;
        private readonly IWrapperManager? _wrapperManager;

        private static readonly IMetadataContextKey<Dictionary<string, IViewModelPresenterMediator>, Dictionary<string, IViewModelPresenterMediator>> Mediators
            = MetadataContextKey.FromMember<Dictionary<string, IViewModelPresenterMediator>, Dictionary<string, IViewModelPresenterMediator>>(typeof(ViewModelMediatorPresenter), nameof(Mediators));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelMediatorPresenter(IViewManager? viewManager = null, IWrapperManager? wrapperManager = null, IServiceProvider? serviceProvider = null)
        {
            _mediators = new List<(Type, Type, bool)>();
            _viewManager = viewManager;
            _wrapperManager = wrapperManager;
            _serviceProvider = serviceProvider;
            _removeHandlerTrue = new RemoveHandler(_mediators, true);
            _removeHandlerFalse = new RemoveHandler(_mediators, false);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = PresenterComponentPriority.Presenter;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryShow<TRequest>([DisallowNull]in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            object? view;
            IViewModelBase? viewModel;
            if (Default.IsValueType<TRequest>())
            {
                if (typeof(TRequest) != typeof(ViewModelViewRequest))
                    return default;

                var r = MugenExtensions.CastGeneric<TRequest, ViewModelViewRequest>(request);
                view = r.View;
                viewModel = r.ViewModel;
            }
            else
            {
                view = null;
                viewModel = request as IViewModelBase;
            }

            if (viewModel == null)
                return default;

            ItemOrList<IPresenterResult, List<IPresenterResult>> result = default;
            var mediators = TryGetMediators(viewModel, request, metadata);
            for (var i = 0; i < MugenExtensions.Count(mediators); i++)
            {
                var mediator = MugenExtensions.Get(mediators, i);
                result.Add(mediator.TryShow(view, cancellationToken, metadata));
            }

            return result.Cast<IReadOnlyList<IPresenterResult>>();
        }

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose<TRequest>([DisallowNull]in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (Default.IsValueType<TRequest>() || !(request is IViewModelBase viewModel))
                return default;

            ItemOrList<IPresenterResult, List<IPresenterResult>> result = default;
            lock (_mediators)
            {
                var dictionary = viewModel.GetMetadataOrDefault().Get(Mediators);
                if (dictionary == null)
                    return default;

                foreach (var mediator in dictionary)
                    result.Add(mediator.Value.TryClose(cancellationToken, metadata));
                return result.Cast<IReadOnlyList<IPresenterResult>>();
            }
        }

        #endregion

        #region Methods

        public ActionToken RegisterMediator<TMediator, TView>(bool viewExactlyEqual = false)
            where TMediator : ViewModelPresenterMediatorBase<TView>
            where TView : class
        {
            return RegisterMediator(typeof(TMediator), typeof(TView), viewExactlyEqual);
        }

        public ActionToken RegisterMediator(Type mediatorType, Type viewType, bool viewExactlyEqual)
        {
            Should.BeOfType<IViewModelPresenterMediator>(mediatorType, nameof(mediatorType));
            Should.NotBeNull(viewType, nameof(viewType));
            lock (_mediators)
            {
                _mediators.Add((mediatorType, viewType, viewExactlyEqual));
            }

            if (viewExactlyEqual)
                return new ActionToken(_removeHandlerTrue, mediatorType, viewType);
            return new ActionToken(_removeHandlerFalse, mediatorType, viewType);
        }

        private ItemOrList<IViewModelPresenterMediator, List<IViewModelPresenterMediator>> TryGetMediators<TRequest>(IViewModelBase viewModel, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (viewModel == null)
                return default;

            ItemOrList<IViewModelPresenterMediator, List<IViewModelPresenterMediator>> result = default;
            lock (_mediators)
            {
                if (_mediators.Count == 0)
                    return default;

                var dictionary = viewModel.Metadata.Get(Mediators);
                var mappings = _viewManager.DefaultIfNull().GetMappings(request, metadata);
                for (var i = 0; i < mappings.Count(); i++)
                {
                    var mapping = mappings.Get(i);
                    if (dictionary == null || !dictionary.TryGetValue(mapping.Id, out var mediator))
                    {
                        mediator = GetMediator(viewModel, mapping, metadata)!;
                        if (mediator == null)
                            continue;

                        if (dictionary == null)
                        {
                            dictionary = new Dictionary<string, IViewModelPresenterMediator>();
                            viewModel.Metadata.Set(Mediators, dictionary);
                        }

                        dictionary[mapping.Id] = mediator;
                    }

                    result.Add(mediator);
                }
            }

            return result;
        }

        private IViewModelPresenterMediator? GetMediator(IViewModelBase viewModel, IViewModelViewMapping mapping, IReadOnlyMetadataContext? metadata)
        {
            var wrapperManager = _wrapperManager.DefaultIfNull();
            Type? mediatorType = null;
            for (var i = 0; i < _mediators.Count; i++)
            {
                var mediatorInfo = _mediators[i];
                if (mediatorInfo.viewExactlyEqual)
                {
                    if (mediatorInfo.viewType == mapping.ViewType)
                    {
                        mediatorType = mediatorInfo.mediatorType;
                        break;
                    }
                }
                else
                {
                    if (mediatorInfo.viewType.IsAssignableFrom(mapping.ViewType) || wrapperManager.CanWrap(mediatorInfo.viewType, mapping.ViewType, metadata))
                    {
                        mediatorType = mediatorInfo.mediatorType;
                        break;
                    }
                }
            }

            if (mediatorType == null)
                return null;

            var mediator = (IViewModelPresenterMediator?)_serviceProvider.DefaultIfNull().GetService(mediatorType);
            mediator?.Initialize(viewModel, mapping, metadata);
            return mediator;
        }

        #endregion

        #region Nested types

        private sealed class RemoveHandler : ActionToken.IHandler
        {
            #region Fields

            private readonly List<(Type mediatorType, Type viewType, bool viewExactlyEqual)> _mediators;
            private readonly bool _viewExactlyEqual;

            #endregion

            #region Constructors

            public RemoveHandler(List<(Type mediatorType, Type viewType, bool viewExactlyEqual)> mediators, bool viewExactlyEqual)
            {
                _mediators = mediators;
                _viewExactlyEqual = viewExactlyEqual;
            }

            #endregion

            #region Implementation of interfaces

            public void Invoke(object? state1, object? state2)
            {
                lock (_mediators)
                {
                    _mediators.Remove(((Type)state1!, (Type)state2!, _viewExactlyEqual));
                }
            }

            #endregion
        }

        #endregion
    }
}