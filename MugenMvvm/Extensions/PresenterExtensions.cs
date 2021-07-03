using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Presentation;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        public static ItemOrIReadOnlyList<IPresenterResult> Show(this IPresenter presenter, object request, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            var result = presenter.TryShow(request, cancellationToken, metadata);
            if (result.IsEmpty)
                ExceptionManager.ThrowPresenterCannotShowRequest(request, metadata);
            return result;
        }

        public static ShowPresenterResult ShowAsync(this IViewModelBase viewModel, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null, IPresenter? presenter = null, INavigationDispatcher? navigationDispatcher = null)
        {
            TryGetShowPresenterResult(presenter.DefaultIfNull(viewModel).Show(viewModel, cancellationToken, metadata), navigationDispatcher.DefaultIfNull(viewModel), metadata,
                out var showingCallback, out var closeCallback, out var presenterResult);
            return new ShowPresenterResult(presenterResult, showingCallback, closeCallback);
        }

        public static ShowPresenterResult<T> ShowAsync<T>(this IHasResult<T> hasNavigationResult, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null, IPresenter? presenter = null, INavigationDispatcher? navigationDispatcher = null)
        {
            TryGetShowPresenterResult(presenter.DefaultIfNull(hasNavigationResult).Show(hasNavigationResult, cancellationToken, metadata),
                navigationDispatcher.DefaultIfNull(hasNavigationResult), metadata, out var showingCallback, out var closeCallback, out var presenterResult);
            return new ShowPresenterResult<T>(presenterResult, showingCallback, closeCallback);
        }

        public static ValueTask<bool> CloseAsync(this IViewModelBase viewModel, CancellationToken cancellationToken = default, bool isSerializable = true,
            IReadOnlyMetadataContext? metadata = null, IPresenter? presenter = null, INavigationDispatcher? navigationDispatcher = null)
        {
            var callbacks = new ItemOrListEditor<INavigationCallback>(2);
            foreach (var result in presenter.DefaultIfNull(viewModel).TryClose(viewModel, cancellationToken, metadata))
            foreach (var callback in navigationDispatcher.DefaultIfNull(viewModel).GetNavigationCallbacks(result, metadata))
            {
                if (callback.CallbackType == NavigationCallbackType.Closing)
                    callbacks.Add(callback);
            }

            return callbacks.WhenAll(true, isSerializable);
        }

        public static ValueTaskAwaiter<INavigationContext> GetAwaiter(this ShowPresenterResult showResult) => showResult.CloseCallback.AsTask(true).GetAwaiter();

        public static ValueTaskAwaiter<T?> GetAwaiter<T>(this ShowPresenterResult<T> showResult) => showResult.CloseCallback.GetResultAsync<T>().GetAwaiter()!;

        public static ConfiguredValueTaskAwaitable<INavigationContext> ConfigureAwait(this ShowPresenterResult showResult, bool continueOnCapturedContext) =>
            showResult.CloseCallback.AsTask(true).ConfigureAwait(continueOnCapturedContext);

        public static ConfiguredValueTaskAwaitable<T?> ConfigureAwait<T>(this ShowPresenterResult<T> showResult, bool continueOnCapturedContext) =>
            showResult.CloseCallback.GetResultAsync<T>().ConfigureAwait(continueOnCapturedContext);

        private static async ValueTask<T?> GetResultAsync<T>(this INavigationCallback callback)
        {
            var navigationContext = await callback.AsTask(true).ConfigureAwait(false);
            if (navigationContext.Target is IHasResult<T> hasResult)
                return hasResult.Result;
            return default;
        }

        private static void TryGetShowPresenterResult(ItemOrIReadOnlyList<IPresenterResult> result, INavigationDispatcher navigationDispatcher, IReadOnlyMetadataContext? metadata,
            out INavigationCallback? showingCallback, out INavigationCallback closeCallback, out IPresenterResult presenterResult)
        {
            if (result.List != null || result.Item == null)
                ExceptionManager.ThrowMultiplePresenterResultNotSupported();
            presenterResult = result.Item;
            showingCallback = null;
            closeCallback = null!;
            foreach (var navigationCallback in navigationDispatcher.GetNavigationCallbacks(result.Item, metadata))
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
    }
}