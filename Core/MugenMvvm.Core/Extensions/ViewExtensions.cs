using System;
using System.Linq;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static TView? TryWrap<TView>(this IView view, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
            where TView : class
        {
            return (TView?) view.TryWrap(typeof(TView), metadata, wrapperManager);
        }

        public static object? TryWrap(this IView view, Type wrapperType, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
        {
            return WrapInternal(view, wrapperType, metadata, wrapperManager, true);
        }

        public static TView Wrap<TView>(this IView view, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
            where TView : class
        {
            return (TView) view.Wrap(typeof(TView), metadata, wrapperManager);
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
            return wrapperType.IsInstanceOfType(view.View) || wrapperManager.DefaultIfNull().CanWrap(view.View.GetType(), wrapperType, metadata);
        }

        private static object? WrapInternal(this IView view, Type wrapperType, IReadOnlyMetadataContext? metadata, IWrapperManager? wrapperManager, bool checkCanWrap)
        {
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            if (wrapperType.IsInstanceOfType(view.View))
                return view.View;

            var collection = view.Components;
            lock (collection)
            {
                var item = collection.Get<object>(metadata).FirstOrDefault(wrapperType.IsInstanceOfType);
                if (item == null)
                {
                    wrapperManager = wrapperManager.DefaultIfNull();
                    if (checkCanWrap && !wrapperManager.CanWrap(view.View.GetType(), wrapperType, metadata))
                        return null;

                    item = wrapperManager.Wrap(view.View, wrapperType, metadata);
                    collection.Add(item, metadata);
                }

                return item;
            }
        }

        #endregion
    }
}