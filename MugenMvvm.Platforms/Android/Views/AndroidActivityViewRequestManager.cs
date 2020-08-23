using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Requests;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Android.Views
{
    public sealed class AndroidActivityViewRequestManager : ComponentDecoratorBase<IViewManager, IViewManagerComponent>, IViewManagerComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.ViewModelViewProviderDecorator + 1;

        #endregion

        #region Implementation of interfaces

        public Task<IView>? TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (!(request is AndroidActivityViewRequest activityRequest))
                return Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata);
            var handler = new PendingActivityHandler(activityRequest.Mapping, cancellationToken);
            viewManager.AddComponent(handler);
            activityRequest.StartActivity();

            var tcs = new TaskCompletionSource<IView>();
            handler.Task.ContinueWithEx((tcs, Components, viewManager, mapping, activityRequest, metadata), (task, tuple) =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    tuple.tcs.TrySetFromTask(task);
                    return;
                }

                tuple.activityRequest.View = task.Result;
                var result = tuple.Components.TryInitializeAsync(tuple.viewManager, tuple.mapping, tuple.activityRequest, default, tuple.metadata);
                if (result == null)
                    ExceptionManager.ThrowObjectNotInitialized(tuple.Components);
                tuple.tcs.TrySetFromTask(result);
            }).ContinueWith((task, o) => ((TaskCompletionSource<IView>)o).TrySetFromTask(task), tcs, TaskContinuationOptions.NotOnRanToCompletion);
            return tcs.Task;
        }

        public Task? TryCleanupAsync(IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => Components.TryCleanupAsync(viewManager, view, state, cancellationToken, metadata);

        #endregion

        #region Nested types

        private sealed class PendingActivityHandler : TaskCompletionSource<object>, IViewLifecycleDispatcherComponent, IHasPriority
        {
            #region Fields

            private readonly IViewMapping _mapping;
            private readonly CancellationToken _cancellationToken;

            #endregion

            #region Constructors

            public PendingActivityHandler(IViewMapping mapping, CancellationToken cancellationToken)
            {
                _mapping = mapping;
                _cancellationToken = cancellationToken;
            }

            #endregion

            #region Properties

            public int Priority => ViewComponentPriority.PreInitializer;

            #endregion

            #region Implementation of interfaces

            public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
            {
                if (lifecycleState != AndroidViewLifecycleState.Created)
                    return;
                view = MugenExtensions.GetUnderlyingView(view);
                if (!_mapping.ViewType.IsInstanceOfType(view))
                    return;

                viewManager.RemoveComponent(this);
                if (_cancellationToken.IsCancellationRequested)
                {
                    if (view is IActivityView activityView)
                        activityView.Finish();
                    TrySetCanceled(_cancellationToken);
                }
                else
                    TrySetResult(view);
            }

            #endregion
        }

        #endregion
    }
}