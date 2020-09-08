using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Internal;
using MugenMvvm.Presenters;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> Show(this IPresenter presenter, object request, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            var result = presenter.TryShow(request, cancellationToken, metadata);
            if (result.IsNullOrEmpty())
                ExceptionManager.ThrowPresenterCannotShowRequest(request, metadata);
            return result;
        }

        public static ShowPresenterResult ShowAsync(this IViewModelBase viewModel, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null, IPresenter? presenter = null, INavigationDispatcher? navigationDispatcher = null)
        {
            TryGetShowPresenterResult(presenter.DefaultIfNull().Show(viewModel, cancellationToken, metadata), navigationDispatcher, metadata, out var showingCallback, out var closeCallback, out var presenterResult);
            return new ShowPresenterResult(presenterResult, showingCallback, closeCallback);
        }

        public static ShowPresenterResult<T> ShowAsync<T>(this IHasNavigationResult<T> hasNavigationResult, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null, IPresenter? presenter = null, INavigationDispatcher? navigationDispatcher = null)
        {
            TryGetShowPresenterResult(presenter.DefaultIfNull().Show(hasNavigationResult, cancellationToken, metadata), navigationDispatcher, metadata, out var showingCallback, out var closeCallback,
                out var presenterResult);
            return new ShowPresenterResult<T>(presenterResult, showingCallback, closeCallback);
        }

        public static Task<bool> CloseAsync(this IViewModelBase viewModel, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null, IPresenter? presenter = null, INavigationDispatcher? navigationDispatcher = null)
        {
            var tasks = ItemOrListEditor.Get<Task<bool>>();
            foreach (var result in presenter.DefaultIfNull().TryClose(viewModel, cancellationToken, metadata).Iterator())
            {
                foreach (var callback in navigationDispatcher.DefaultIfNull().GetNavigationCallbacks(result, metadata).Iterator())
                {
                    if (callback.CallbackType == NavigationCallbackType.Closing)
                        tasks.Add(callback.AsTask().CancelToFalse());
                }
            }

            var taskList = tasks.ToItemOrList();
            if (taskList.Item != null)
                return taskList.Item;
            if (taskList.List == null)
                return Default.FalseTask;

            return Task.WhenAll(taskList.List).ContinueWith(task =>
            {
                var result = task.Result;
                for (var i = 0; i < result.Length; i++)
                {
                    if (!result[i])
                        return false;
                }

                return true;
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        public static TaskAwaiter GetAwaiter(this ShowPresenterResult showResult) => ((Task) showResult.CloseCallback.AsTask()).GetAwaiter();

        public static TaskAwaiter<T> GetAwaiter<T>(this ShowPresenterResult<T> showResult) =>
            showResult.CloseCallback
                .AsTask()
                .ContinueWith(task => task.Result.Target is IHasNavigationResult<T> navigationResult ? navigationResult.Result : default, TaskContinuationOptions.ExecuteSynchronously)
                .GetAwaiter()!;

        private static Task<bool> CancelToFalse(this Task<INavigationContext> task) =>
            task.ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return false;
                return t.Result != null;
            }, TaskContinuationOptions.ExecuteSynchronously);


        private static void TryGetShowPresenterResult(ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> result, INavigationDispatcher? navigationDispatcher, IReadOnlyMetadataContext? metadata,
            out INavigationCallback? showingCallback, out INavigationCallback closeCallback, out IPresenterResult presenterResult)
        {
            if (result.List != null || result.Item == null)
                ExceptionManager.ThrowMultiplePresenterResultNotSupported();
            presenterResult = result.Item;
            showingCallback = null;
            closeCallback = null!;
            foreach (var navigationCallback in navigationDispatcher.DefaultIfNull().GetNavigationCallbacks(result.Item, metadata).Iterator())
            {
                if (navigationCallback.CallbackType == NavigationCallbackType.Showing)
                    showingCallback = navigationCallback;
                else if (navigationCallback.CallbackType == NavigationCallbackType.Close)
                    closeCallback = navigationCallback;
            }

            if (closeCallback == null)
            {
                ExceptionManager.ThrowRequestNotSupported<ShowPresenterResult>(presenterResult, presenterResult, metadata);
                closeCallback = null;
            }
        }

        #endregion
    }
}