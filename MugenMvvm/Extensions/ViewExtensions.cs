using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Interfaces.Wrapping;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static object GetUnderlyingView(object view) => view is IView v ? v.Target : view;

        public static Task<IView> InitializeAsync(this IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            var task = viewManager.TryInitializeAsync(mapping, request, cancellationToken, metadata);
            if (task == null)
                ExceptionManager.ThrowRequestNotSupported<IViewManagerComponent>(viewManager, request, metadata);
            return task;
        }

        public static Task CleanupAsync(this IViewManager viewManager, IView view, object? state = null, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null) =>
            viewManager.TryCleanupAsync(view, state, cancellationToken, metadata) ?? Task.CompletedTask;

        public static TView? TryWrap<TView>(this IView view, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
            where TView : class =>
            (TView?) view.TryWrap(typeof(TView), metadata, wrapperManager);

        public static object? TryWrap(this IView view, Type wrapperType, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null) =>
            wrapperManager.DefaultIfNull().TryWrap(wrapperType, view, metadata);

        public static TView Wrap<TView>(this IView view, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
            where TView : class =>
            (TView) view.Wrap(typeof(TView), metadata, wrapperManager);

        public static object Wrap(this IView view, Type wrapperType, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null) => wrapperManager.DefaultIfNull().Wrap(wrapperType, view, metadata);

        public static bool CanWrap<TView>(this IView view, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null) where TView : class => view.CanWrap(typeof(TView), metadata, wrapperManager);

        public static bool CanWrap(this IView view, Type wrapperType, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null) =>
            wrapperManager.DefaultIfNull().CanWrap(wrapperType, view, metadata);

        #endregion
    }
}