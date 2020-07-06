using System;
using System.Diagnostics.CodeAnalysis;
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

        public static Task<IView> InitializeAsync<TRequest>(this IViewManager viewManager, IViewMapping mapping, [DisallowNull] in TRequest request, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            var task = viewManager.TryInitializeAsync(mapping, request, cancellationToken, metadata);
            if (task == null)
                ExceptionManager.ThrowObjectNotInitialized<IViewManagerComponent>(viewManager);
            return task;
        }

        public static Task CleanupAsync<TRequest>(this IViewManager viewManager, IView view, in TRequest request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null)
        {
            return viewManager.TryCleanupAsync(view, request, cancellationToken, metadata) ?? Task.CompletedTask;
        }

        public static TView? TryWrap<TView>(this IView view, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
            where TView : class
        {
            return (TView?)view.TryWrap(typeof(TView), metadata, wrapperManager);
        }

        public static object? TryWrap(this IView view, Type wrapperType, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
        {
            return wrapperManager.DefaultIfNull().TryWrap(wrapperType, view, metadata);
        }

        public static TView Wrap<TView>(this IView view, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
            where TView : class
        {
            return (TView)view.Wrap(typeof(TView), metadata, wrapperManager);
        }

        public static object Wrap(this IView view, Type wrapperType, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
        {
            return wrapperManager.DefaultIfNull().Wrap(wrapperType, view, metadata);
        }

        public static bool CanWrap<TView>(this IView view, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null) where TView : class
        {
            return view.CanWrap(typeof(TView), metadata, wrapperManager);
        }

        public static bool CanWrap(this IView view, Type wrapperType, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
        {
            return wrapperManager.DefaultIfNull().CanWrap(wrapperType, view, metadata);
        }

        #endregion
    }
}