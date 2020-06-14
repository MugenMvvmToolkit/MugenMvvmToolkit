﻿using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views.Components
{
    public class ViewCleaner : IViewLifecycleDispatcherComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ComponentPriority.PostInitializer;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged<TState>(object view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == ViewLifecycleState.Cleared && view is IView viewImp)
                Cleanup(viewImp, lifecycleState, state, metadata);
        }

        #endregion

        #region Methods

        protected virtual void Cleanup<TState>(IView view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            view.ViewModel.TryUnsubscribe(view.Target, metadata);
            (view.Target as ICleanableView)?.Cleanup(state, metadata);
            var cleanableViews = view.GetComponents<ICleanableView>(metadata);
            for (var i = 0; i < cleanableViews.Length; i++)
                cleanableViews[i].Cleanup(state, metadata);
            view.ClearMetadata(true);
            if (view.HasComponents)
            {
                view.Components.Clear(metadata);
                view.Components.ClearComponents(metadata);
            }
        }

        #endregion
    }
}