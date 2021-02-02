using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views.Components
{
    public class ViewCleaner : IViewLifecycleListener, IHasPriority, IComponentCollectionChangedListener
    {
        private readonly IAttachedValueManager? _attachedValueManager;

        public ViewCleaner(IAttachedValueManager? attachedValueManager = null)
        {
            _attachedValueManager = attachedValueManager;
        }

        public bool ClearDataContext { get; set; }

        public int Priority { get; set; } = ViewComponentPriority.PostInitializer;

        protected IAttachedValueManager AttachedValueManager => _attachedValueManager.DefaultIfNull();

        public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (view is not IView viewImp)
                return;

            if (lifecycleState == ViewLifecycleState.Initializing)
                Initialize(viewImp, state, metadata);
            else if (lifecycleState == ViewLifecycleState.Cleared)
                Cleanup(viewImp, state, metadata);
        }

        protected virtual void Initialize(IView view, object? state, IReadOnlyMetadataContext? metadata) => view.Components.AddComponent(this);

        protected virtual void Cleanup(IView view, object? state, IReadOnlyMetadataContext? metadata)
        {
            view.ViewModel.TryUnsubscribe(view.Target, metadata);
            (view.Target as ICleanableView)?.Cleanup(state, metadata);
            foreach (var v in view.GetComponents<ICleanableView>(metadata))
                v.Cleanup(state, metadata);

            view.ClearMetadata(true);
            view.Components.RemoveComponent(this);
            view.Components.Clear(metadata);
            view.Components.ClearComponents(metadata);
            view.Target.AttachedValues(null, _attachedValueManager).Clear();
            if (ClearDataContext)
                view.Target.BindableMembers().SetDataContext(null);
        }

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
        }

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            (component as ICleanableView)?.Cleanup(null, metadata);
    }
}