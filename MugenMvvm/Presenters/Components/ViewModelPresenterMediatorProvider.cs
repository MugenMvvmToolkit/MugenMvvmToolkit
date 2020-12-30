using System;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;

namespace MugenMvvm.Presenters.Components
{
    public sealed class ViewModelPresenterMediatorProvider : IViewModelPresenterMediatorProviderComponent, IHasPriority
    {
        #region Fields

        private readonly Func<IPresenter, IViewModelBase, IViewMapping, IReadOnlyMetadataContext?, IViewModelPresenterMediator?> _getMediator;
        private readonly IWrapperManager? _wrapperManager;

        #endregion

        #region Constructors

        private ViewModelPresenterMediatorProvider(Type viewType, bool isExactlyEqual, Func<IPresenter, IViewModelBase, IViewMapping, IReadOnlyMetadataContext?, IViewModelPresenterMediator?> getMediator,
            IWrapperManager? wrapperManager)
        {
            Should.NotBeNull(viewType, nameof(viewType));
            Should.NotBeNull(getMediator, nameof(getMediator));
            _getMediator = getMediator;
            _wrapperManager = wrapperManager;
            ViewType = viewType;
            IsExactlyEqual = isExactlyEqual;
        }

        #endregion

        #region Properties

        public int Priority { get; private set; }

        public Type ViewType { get; }

        public bool IsExactlyEqual { get; }

        #endregion

        #region Implementation of interfaces

        public IViewModelPresenterMediator? TryGetPresenterMediator(IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping, IReadOnlyMetadataContext? metadata)
        {
            if (IsExactlyEqual)
            {
                if (ViewType == mapping.ViewType)
                    return _getMediator(presenter, viewModel, mapping, metadata);
                return null;
            }

            if (ViewType.IsAssignableFrom(mapping.ViewType) || _wrapperManager.DefaultIfNull().CanWrap(ViewType, mapping.ViewType, metadata))
                return _getMediator(presenter, viewModel, mapping, metadata);
            return null;
        }

        #endregion

        #region Methods

        public static ViewModelPresenterMediatorProvider Get<TMediator, TView>(bool isExactlyEqual, Func<IPresenter, IViewModelBase, IViewMapping, IReadOnlyMetadataContext?, TMediator?> getMediator,
            int priority = PresenterComponentPriority.ViewModelPresenterMediatorProvider, IWrapperManager? wrapperManager = null)
            where TMediator : ViewModelPresenterMediatorBase<TView>
            where TView : class =>
            Get(typeof(TView), isExactlyEqual, getMediator, priority, wrapperManager);

        public static ViewModelPresenterMediatorProvider Get(Type viewType, bool isExactlyEqual, Func<IPresenter, IViewModelBase, IViewMapping, IReadOnlyMetadataContext?, IViewModelPresenterMediator?> getMediator,
            int priority = PresenterComponentPriority.ViewModelPresenterMediatorProvider, IWrapperManager? wrapperManager = null) =>
            new(viewType, isExactlyEqual, getMediator, wrapperManager) {Priority = priority};

        public static ViewModelPresenterMediatorProvider Get(Func<IPresenter, IViewModelBase, IViewMapping, IReadOnlyMetadataContext?, IViewModelPresenterMediator?> getMediator,
            int priority = PresenterComponentPriority.ViewModelPresenterMediatorProvider, IWrapperManager? wrapperManager = null) =>
            new(typeof(object), false, getMediator, wrapperManager) {Priority = priority};

        #endregion
    }
}