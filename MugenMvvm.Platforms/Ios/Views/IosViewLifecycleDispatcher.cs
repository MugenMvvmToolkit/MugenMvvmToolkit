using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Ios.Enums;
using MugenMvvm.Ios.Extensions;
using UIKit;

namespace MugenMvvm.Ios.Views
{
    public sealed class IosViewLifecycleDispatcher : IViewLifecycleDispatcherComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.PreInitializer;

        public bool DisposeView { get; set; } = true;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if ((lifecycleState == IosViewLifecycleState.WillAppearing || lifecycleState == IosViewLifecycleState.DidMovedToParentViewController || lifecycleState == IosViewLifecycleState.RemovedFromParentViewController)
                && MugenExtensions.GetUnderlyingView(view) is UIViewController controller)
            {
                controller.ViewIfLoaded?.RaiseParentChanged();
                BindableMembers.For<object>().ParentNative().TryRaise(controller);
            }
            else if (lifecycleState == ViewLifecycleState.Cleared && MugenExtensions.GetUnderlyingView(view) is UIViewController c)
                c.ViewIfLoaded?.ClearBindings(true, DisposeView, true);
        }

        #endregion
    }
}