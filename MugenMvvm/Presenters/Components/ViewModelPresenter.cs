using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Presenters.Components
{
    public sealed class ViewModelPresenter : IPresenterComponent, IHasPriority
    {
        #region Fields

        private readonly List<MediatorRegistration> _mediators;
        private readonly IServiceProvider? _serviceProvider;
        private readonly IViewManager? _viewManager;
        private readonly IWrapperManager? _wrapperManager;

        private static readonly IMetadataContextKey<Dictionary<string, IViewModelPresenterMediator>, Dictionary<string, IViewModelPresenterMediator>> Mediators
            = MetadataContextKey.FromMember<Dictionary<string, IViewModelPresenterMediator>, Dictionary<string, IViewModelPresenterMediator>>(typeof(ViewModelPresenter), nameof(Mediators));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelPresenter(IViewManager? viewManager = null, IWrapperManager? wrapperManager = null, IServiceProvider? serviceProvider = null)
        {
            _mediators = new List<MediatorRegistration>();
            _viewManager = viewManager;
            _wrapperManager = wrapperManager;
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = PresenterComponentPriority.Presenter;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryShow(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var viewModel = MugenExtensions.TryGetViewModelView(request, out object? view);
            if (viewModel == null)
                return default;

            var result = ItemOrListEditor.Get<IPresenterResult>();
            foreach (var mediator in TryGetMediators(viewModel, request, metadata).Iterator())
                result.Add(mediator.TryShow(view, cancellationToken, metadata));

            return result.ToItemOrList<IReadOnlyList<IPresenterResult>>();
        }

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var viewModel = MugenExtensions.TryGetViewModelView(request, out object? _);
            if (viewModel == null)
                return default;

            var result = ItemOrListEditor.Get<IPresenterResult>();
            lock (_mediators)
            {
                var dictionary = viewModel.GetMetadataOrDefault().Get(Mediators);
                if (dictionary == null)
                    return default;

                foreach (var mediator in dictionary)
                    result.Add(mediator.Value.TryClose(cancellationToken, metadata));
                return result.ToItemOrList<IReadOnlyList<IPresenterResult>>();
            }
        }

        #endregion

        #region Methods

        public ActionToken RegisterMediator<TMediator, TView>(bool viewExactlyEqual = false, int priority = 0)
            where TMediator : ViewModelPresenterMediatorBase<TView>
            where TView : class =>
            RegisterMediator(typeof(TMediator), typeof(TView), viewExactlyEqual, priority);

        public ActionToken RegisterMediator(Type mediatorType, Type viewType, bool viewExactlyEqual, int priority = 0)
        {
            Should.BeOfType<IViewModelPresenterMediator>(mediatorType, nameof(mediatorType));
            Should.NotBeNull(viewType, nameof(viewType));
            var registration = new MediatorRegistration(mediatorType, viewType, viewExactlyEqual, priority);
            lock (_mediators)
            {
                MugenExtensions.AddOrdered(_mediators, registration, registration);
            }

            return new ActionToken((l, m) =>
            {
                var list = (List<MediatorRegistration>) l!;
                lock (list)
                {
                    list.Remove((MediatorRegistration) m!);
                }
            }, _mediators, registration);
        }

        private ItemOrList<IViewModelPresenterMediator, List<IViewModelPresenterMediator>> TryGetMediators(IViewModelBase viewModel, object request, IReadOnlyMetadataContext? metadata)
        {
            if (viewModel == null)
                return default;

            var result = ItemOrListEditor.Get<IViewModelPresenterMediator>();
            lock (_mediators)
            {
                if (_mediators.Count == 0)
                    return default;

                var dictionary = viewModel.Metadata.Get(Mediators);
                foreach (var mapping in _viewManager.DefaultIfNull().GetMappings(request, metadata).Iterator())
                {
                    if (dictionary == null || !dictionary.TryGetValue(mapping.Id, out var mediator))
                    {
                        mediator = GetMediator(viewModel, mapping, metadata)!;
                        if (mediator == null)
                            continue;

                        if (dictionary == null)
                        {
                            dictionary = new Dictionary<string, IViewModelPresenterMediator>();
                            viewModel.Metadata.Set(Mediators, dictionary, out _);
                        }

                        dictionary[mapping.Id] = mediator;
                    }

                    result.Add(mediator);
                }
            }

            return result.ToItemOrList();
        }

        private IViewModelPresenterMediator? GetMediator(IViewModelBase viewModel, IViewMapping mapping, IReadOnlyMetadataContext? metadata)
        {
            var wrapperManager = _wrapperManager.DefaultIfNull();
            Type? mediatorType = null;
            for (var i = 0; i < _mediators.Count; i++)
            {
                var mediatorInfo = _mediators[i];
                if (mediatorInfo.ExactlyEqual)
                {
                    if (mediatorInfo.ViewType == mapping.ViewType)
                    {
                        mediatorType = mediatorInfo.MediatorType;
                        break;
                    }
                }
                else
                {
                    if (mediatorInfo.ViewType.IsAssignableFrom(mapping.ViewType) || wrapperManager.CanWrap(mediatorInfo.ViewType, mapping.ViewType, metadata))
                    {
                        mediatorType = mediatorInfo.MediatorType;
                        break;
                    }
                }
            }

            if (mediatorType == null)
                return null;

            var mediator = (IViewModelPresenterMediator?) _serviceProvider.DefaultIfNull().GetService(mediatorType);
            mediator?.Initialize(viewModel, mapping, metadata);
            return mediator;
        }

        #endregion

        #region Nested types

        private sealed class MediatorRegistration : IComparer<MediatorRegistration>
        {
            #region Fields

            public readonly bool ExactlyEqual;
            public readonly Type MediatorType;
            public readonly int Priority;
            public readonly Type ViewType;

            #endregion

            #region Constructors

            public MediatorRegistration(Type mediatorType, Type viewType, bool exactlyEqual, int priority)
            {
                MediatorType = mediatorType;
                ViewType = viewType;
                ExactlyEqual = exactlyEqual;
                Priority = priority;
            }

            #endregion

            #region Implementation of interfaces

            public int Compare([AllowNull] MediatorRegistration x, [AllowNull] MediatorRegistration y) => y!.Priority.CompareTo(x!.Priority);

            #endregion
        }

        #endregion
    }
}