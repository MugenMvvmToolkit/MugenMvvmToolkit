using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        public static Task<IView> InitializeAsync<TRequest>(this IViewManager viewManager, IViewModelViewMapping mapping, [DisallowNull] in TRequest request, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            var task = viewManager.TryInitializeAsync(mapping, request, cancellationToken, metadata);
            if (task == null)
                ExceptionManager.ThrowObjectNotInitialized<IViewManagerComponent>(viewManager);
            return task;
        }

        public static Task CleanupAsync<TRequest>(this IViewManager viewManager, IView view, [DisallowNull] in TRequest request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null)
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
            return WrapInternal(view, wrapperType, metadata, wrapperManager, true);
        }

        public static TView Wrap<TView>(this IView view, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
            where TView : class
        {
            return (TView)view.Wrap(typeof(TView), metadata, wrapperManager);
        }

        public static object Wrap(this IView view, Type wrapperType, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
        {
            return WrapInternal(view, wrapperType, metadata, wrapperManager, false)!;
        }

        public static bool CanWrap<TView>(this IView view, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null) where TView : class
        {
            return view.CanWrap(typeof(TView), metadata, wrapperManager);
        }

        public static bool CanWrap(this IView view, Type wrapperType, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
        {
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            return wrapperType.IsInstanceOfType(view.Target) || wrapperManager.DefaultIfNull().CanWrap(wrapperType, view.Target, metadata);
        }

        private static object? WrapInternal(this IView view, Type wrapperType, IReadOnlyMetadataContext? metadata, IWrapperManager? wrapperManager, bool tryWrap)
        {
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            if (wrapperType.IsInstanceOfType(view.Target))
                return view.Target;

            var collection = view.Components;
            lock (collection)
            {
                var item = collection.Get<object>(metadata).FirstOrDefault(wrapperType.IsInstanceOfType);
                if (item == null)
                {
                    wrapperManager = wrapperManager.DefaultIfNull();
                    if (tryWrap)
                    {
                        item = wrapperManager.TryWrap(wrapperType, view.Target, metadata);
                        if (item == null)
                            return null;
                    }
                    else
                        item = wrapperManager.Wrap(wrapperType, view.Target, metadata);
                }

                return item;
            }
        }

        #endregion
    }
}