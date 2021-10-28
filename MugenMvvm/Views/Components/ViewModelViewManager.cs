using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewModelViewManager : IViewManagerComponent, IViewProviderComponent, IHasPriority
    {
        private readonly IAttachedValueManager? _attachedValueManager;
        private readonly IViewModelManager? _viewModelManager;
        private readonly IComponentCollectionManager? _componentCollectionManager;

        [Preserve(Conditional = true)]
        public ViewModelViewManager(IAttachedValueManager? attachedValueManager = null, IViewModelManager? viewModelManager = null,
            IComponentCollectionManager? componentCollectionManager = null)
        {
            _attachedValueManager = attachedValueManager;
            _viewModelManager = viewModelManager;
            _componentCollectionManager = componentCollectionManager;
        }

        public int Priority { get; init; } = ViewComponentPriority.Initializer;

        public ValueTask<IView?> TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            var viewModel = MugenExtensions.TryGetViewModelView(request, out object? view);
            if (viewModel == null || view == null)
                return default;

            var currentViews = ItemOrArray.FromRawValue<IView>(viewModel.Metadata.Get(InternalMetadata.Views));
            foreach (var currentView in currentViews)
            {
                if (currentView.Mapping.Id == mapping.Id)
                {
                    if (currentView.Target == view)
                        return new ValueTask<IView?>(currentView);

                    Cleanup(viewManager, currentView, null, metadata);
                    break;
                }
            }

            return new ValueTask<IView?>(InitializeView(viewManager, mapping, viewModel, view, metadata));
        }

        public Task<bool> TryCleanupAsync(IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (!view.Target.AttachedValues(metadata, _attachedValueManager).TryGet(InternalConstant.ViewsValueKey, out var v) ||
                !ItemOrIReadOnlyList.FromRawValue<IView>(v).Contains(view))
                return Default.FalseTask;

            Cleanup(viewManager, view, state, metadata);
            return Default.TrueTask;
        }

        public ItemOrIReadOnlyList<IView> TryGetViews(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            var viewModel = MugenExtensions.TryGetViewModelView(request, out object? view);
            if (viewModel != null)
                return ItemOrIReadOnlyList.FromRawValue<IView>(viewModel.GetOrDefault(InternalMetadata.Views));

            if (view != null && view.AttachedValues(metadata, _attachedValueManager).TryGet(InternalConstant.ViewsValueKey, out var value))
                return ItemOrIReadOnlyList.FromRawValue<IView>(value);
            return default;
        }

        private IView InitializeView(IViewManager viewManager, IViewMapping mapping, IViewModelBase viewModel, object rawView, IReadOnlyMetadataContext? metadata)
        {
            var view = new View(mapping, rawView, viewModel, null, _componentCollectionManager);
            try
            {
                viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initializing, viewModel, metadata);
                var views = viewModel.Metadata.Get(InternalMetadata.Views);
                MugenExtensions.AddRaw(ref views, view);
                viewModel.Metadata.Set(InternalMetadata.Views, views);

                var attachedValues = rawView.AttachedValues(metadata, _attachedValueManager);
                attachedValues.TryGet(InternalConstant.ViewsValueKey, out views);
                MugenExtensions.AddRaw(ref views, view);
                attachedValues.Set(InternalConstant.ViewsValueKey, views);

                if (viewModel.IsInState(ViewModelLifecycleState.Disposed, metadata, _viewModelManager))
                    ExceptionManager.ThrowObjectDisposed(viewModel);
                viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, viewModel, metadata);
                return view;
            }
            catch
            {
                Cleanup(viewManager, view, viewModel, metadata);
                throw;
            }
        }

        private void Cleanup(IViewManager viewManager, IView view, object? state, IReadOnlyMetadataContext? metadata)
        {
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, state, metadata);

            var viewModel = view.ViewModel;
            var views = viewModel.Metadata.Get(InternalMetadata.Views);
            MugenExtensions.RemoveRaw(ref views, view);
            if (views == null)
                viewModel.Metadata.Remove(InternalMetadata.Views);
            else
                viewModel.Metadata.Set(InternalMetadata.Views, views);

            var attachedValues = view.Target.AttachedValues(metadata, _attachedValueManager);
            attachedValues.TryGet(InternalConstant.ViewsValueKey, out views);
            MugenExtensions.RemoveRaw(ref views, view);
            if (views == null)
                attachedValues.Remove(InternalConstant.ViewsValueKey);
            else
                attachedValues.Set(InternalConstant.ViewsValueKey, views);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Cleared, state, metadata);
        }
    }
}