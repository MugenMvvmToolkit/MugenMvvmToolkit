using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Presenters.Components
{
    public sealed class ViewModelPresenter : IPresenterComponent, IHasPriority
    {
        #region Fields

        private readonly object _locker;
        private readonly IViewManager? _viewManager;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelPresenter(IViewManager? viewManager = null)
        {
            _viewManager = viewManager;
            _locker = new object();
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
            foreach (var mediator in TryGetMediators(presenter, viewModel, request, metadata))
                result.AddIfNotNull(mediator.TryShow(view, cancellationToken, metadata)!);

            return result.ToItemOrList<IReadOnlyList<IPresenterResult>>();
        }

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var viewModel = MugenExtensions.TryGetViewModelView(request, out object? view);
            if (viewModel == null)
                return default;

            var result = ItemOrListEditor.Get<IPresenterResult>();
            lock (_locker)
            {
                var dictionary = viewModel.GetOrDefault(InternalMetadata.Mediators);
                if (dictionary == null)
                    return default;

                foreach (var mediator in dictionary)
                    result.AddIfNotNull(mediator.Value.TryClose(view, cancellationToken, metadata)!);
                return result.ToItemOrList<IReadOnlyList<IPresenterResult>>();
            }
        }

        #endregion

        #region Methods

        private ItemOrList<IViewModelPresenterMediator, List<IViewModelPresenterMediator>> TryGetMediators(IPresenter presenter, IViewModelBase viewModel, object request, IReadOnlyMetadataContext? metadata)
        {
            var result = ItemOrListEditor.Get<IViewModelPresenterMediator>();
            lock (_locker)
            {
                var components = presenter.GetComponents<IViewModelPresenterMediatorProviderComponent>();
                if (components.Length == 0)
                    return default;

                var dictionary = viewModel.Metadata.Get(InternalMetadata.Mediators);
                foreach (var mapping in _viewManager.DefaultIfNull().GetMappings(request, metadata))
                {
                    if (dictionary == null || !dictionary.TryGetValue(mapping.Id, out var mediator))
                    {
                        mediator = components.TryGetPresenterMediator(presenter, viewModel, mapping, metadata)!;
                        if (mediator == null)
                            continue;

                        if (dictionary == null)
                        {
                            dictionary = new Dictionary<string, IViewModelPresenterMediator>();
                            viewModel.Metadata.Set(InternalMetadata.Mediators, dictionary, out _);
                        }

                        dictionary[mapping.Id] = mediator;
                    }

                    result.Add(mediator);
                }
            }

            return result.ToItemOrList();
        }

        #endregion
    }
}