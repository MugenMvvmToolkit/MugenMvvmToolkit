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
        private readonly IAttachedValueManager? _attachedValueManager;
        private readonly IViewManager? _viewManager;

        public ViewModelCleaner(IViewManager? viewManager = null, IAttachedValueManager? attachedValueManager = null)
        {
            _viewManager = viewManager;
            _attachedValueManager = attachedValueManager;
        }

        public int Priority { get; init; } = ViewModelComponentPriority.PostInitializer;

        public void OnLifecycleChanged(IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == ViewModelLifecycleState.Disposed)
                Cleanup(viewModel, lifecycleState, state, metadata);
        }

        protected virtual void Cleanup(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            var viewManager = _viewManager.DefaultIfNull(viewModel);
            foreach (var v in viewManager.GetViews(viewModel, metadata))
                viewManager.TryCleanupAsync(v, state, default, metadata);

            var busyManager = viewModel.TryGetService<IBusyManager>(true);
            if (busyManager != null)
            {
                if (MugenExtensions.IsServiceOwner(viewModel, busyManager))
                {
                    busyManager.ClearBusy();
                    busyManager.ClearComponents(metadata);
                }
                else
                    busyManager.Components.Remove(viewModel, metadata);
            }

            var messenger = viewModel.TryGetService<IMessenger>(true);
            if (messenger != null)
            {
                if (MugenExtensions.IsServiceOwner(viewModel, messenger))
                {
                    messenger.UnsubscribeAll(metadata);
                    messenger.ClearComponents(metadata);
                }
                else
                {
                    messenger.TryUnsubscribe(viewModel, metadata);
                    messenger.Components.Remove(viewModel, metadata);
                }
            }

            viewModel.ClearMetadata(true);
            viewModel.AttachedValues(metadata, _attachedValueManager).Clear();
            (viewModel as IValueHolder<IWeakReference>)?.ReleaseWeakReference();
        }
    }
}