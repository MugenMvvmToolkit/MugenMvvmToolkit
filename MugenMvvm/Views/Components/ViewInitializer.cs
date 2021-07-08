using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views.Components
{
    public class ViewInitializer : IViewLifecycleListener, IComponentCollectionChangedListener, IHasPriority
    {
        public bool SetDataContext { get; set; } = true;

        public int Priority { get; init; } = ViewComponentPriority.PreInitializer;

        public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (view is not IView viewImp)
                return;

            if (lifecycleState == ViewLifecycleState.Initializing)
                Initialize(viewImp, state, metadata);
            else if (lifecycleState == ViewLifecycleState.Clearing)
                Cleanup(viewImp, state, metadata);
        }

        protected virtual void Initialize(IView view, object? state, IReadOnlyMetadataContext? metadata)
        {
            view.ViewModel.TrySubscribe(view.Target, ThreadExecutionMode.Main, metadata);
            (view.Target as IInitializableView)?.Initialize(view, state, metadata);
            foreach (var v in view.GetComponents<IInitializableView>(metadata))
                v.Initialize(view, state, metadata);

            view.Components.AddComponent(this);
            if (SetDataContext)
                view.Target.BindableMembers().SetDataContext(view.ViewModel);
        }

        protected virtual void Cleanup(IView view, object? state, IReadOnlyMetadataContext? metadata) => view.Components.RemoveComponent(this);

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            (component as IInitializableView)?.Initialize((IView)collection.Owner, null, metadata);

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
        }
    }
}