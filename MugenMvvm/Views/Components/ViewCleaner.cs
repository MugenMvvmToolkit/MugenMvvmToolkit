using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members;
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
    public class ViewCleaner : IViewLifecycleDispatcherComponent, IHasPriority, IComponentCollectionChangedListener
    {
        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.PostInitializer;

        public bool ClearDataContext { get; set; }

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
        }

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            (component as ICleanableView)?.Cleanup<object?>(null, metadata);
        }

        public void OnLifecycleChanged<TState>(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            if (!(view is IView viewImp))
                return;

            if (lifecycleState == ViewLifecycleState.Initializing)
                Initialize(viewImp, state, metadata);
            else if (lifecycleState == ViewLifecycleState.Cleared)
                Cleanup(viewImp, state, metadata);
        }

        #endregion

        #region Methods

        protected virtual void Initialize<TState>(IView view, in TState state, IReadOnlyMetadataContext? metadata)
        {
            view.Components.AddComponent(this);
        }

        protected virtual void Cleanup<TState>(IView view, in TState state, IReadOnlyMetadataContext? metadata)
        {
            view.ViewModel.TryUnsubscribe(view.Target, metadata);
            (view.Target as ICleanableView)?.Cleanup(state, metadata);
            var cleanableViews = view.GetComponents<ICleanableView>(metadata);
            for (var i = 0; i < cleanableViews.Length; i++)
                cleanableViews[i].Cleanup(state, metadata);
            view.ClearMetadata(true);
            view.Components.RemoveComponent(this);
            view.Components.Clear(metadata);
            view.Components.ClearComponents(metadata);
            if (ClearDataContext)
                view.Target.BindableMembers().SetDataContext(null);
        }

        #endregion
    }
}