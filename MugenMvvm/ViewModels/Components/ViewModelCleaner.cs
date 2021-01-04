using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.ViewModels.Components
{
    public class ViewModelCleaner : IViewModelLifecycleListener, IHasPriority
    {
        #region Fields

        private readonly IAttachedValueManager? _attachedValueManager;
        private readonly IViewManager? _viewManager;

        #endregion

        #region Constructors

        public ViewModelCleaner(IViewManager? viewManager = null, IAttachedValueManager? attachedValueManager = null)
        {
            _viewManager = viewManager;
            _attachedValueManager = attachedValueManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewModelComponentPriority.PostInitializer;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == ViewModelLifecycleState.Disposed)
                Cleanup(viewModel, lifecycleState, state, metadata);
        }

        #endregion

        #region Methods

        protected virtual void Cleanup(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            var viewManager = _viewManager.DefaultIfNull();
            foreach (var v in viewManager.GetViews(viewModel, metadata))
                viewManager.TryCleanupAsync(v, state, default, metadata);

            var busyManager = viewModel.TryGetService<IBusyManager>(true);
            if (busyManager != null)
            {
                busyManager.ClearBusy();
                busyManager.ClearComponents(metadata);
            }

            var messenger = viewModel.TryGetService<IMessenger>(true);
            if (messenger != null)
            {
                messenger.UnsubscribeAll(metadata);
                messenger.ClearComponents(metadata);
            }

            viewModel.ClearMetadata(true);
            viewModel.AttachedValues(metadata, _attachedValueManager).Clear();
            (viewModel as IValueHolder<IWeakReference>)?.ReleaseWeakReference();
        }

        #endregion
    }
}