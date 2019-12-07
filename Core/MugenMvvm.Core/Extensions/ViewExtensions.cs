using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Metadata;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static Task<IReadOnlyMetadataContext> CleanupAsync(this IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewInfo, nameof(viewInfo));
            return viewInfo.Initializer.CleanupAsync(viewInfo, viewModel, metadata);
        }

        public static TView? TryWrap<TView>(this IViewInfo viewInfo, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
            where TView : class
        {
            return (TView?)viewInfo.TryWrap(typeof(TView), metadata, wrapperManager);
        }

        public static object? TryWrap(this IViewInfo viewInfo, Type wrapperType, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
        {
            return WrapInternal(viewInfo, wrapperType, metadata, wrapperManager, true);
        }

        public static TView Wrap<TView>(this IViewInfo viewInfo, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
            where TView : class
        {
            return (TView)viewInfo.Wrap(typeof(TView), metadata, wrapperManager);
        }

        public static object Wrap(this IViewInfo viewInfo, Type wrapperType, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
        {
            return WrapInternal(viewInfo, wrapperType, metadata, wrapperManager, false)!;
        }

        public static bool CanWrap<TView>(this IViewInfo viewInfo, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null) where TView : class
        {
            return viewInfo.CanWrap(typeof(TView), metadata, wrapperManager);
        }

        public static bool CanWrap(this IViewInfo viewInfo, Type wrapperType, IReadOnlyMetadataContext? metadata = null, IWrapperManager? wrapperManager = null)
        {
            Should.NotBeNull(viewInfo, nameof(viewInfo));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            return wrapperType.IsInstanceOfType(viewInfo.View) || wrapperManager.DefaultIfNull().CanWrap(viewInfo.View.GetType(), wrapperType, metadata);
        }

        public static IComponentCollection GetOrAddWrappersCollection(this IViewInfo viewInfo, IComponentCollectionProvider? componentCollectionProvider = null)
        {
            var pair = new KeyValuePair<IComponentCollectionProvider?, IViewInfo>(componentCollectionProvider, viewInfo);
            return viewInfo
                .Metadata
                .GetOrAdd(ViewMetadata.Wrappers, pair, (context, s) => s.Key.DefaultIfNull().GetComponentCollection(s.Value, context));
        }

        private static object? WrapInternal(this IViewInfo viewInfo, Type wrapperType, IReadOnlyMetadataContext? metadata, IWrapperManager? wrapperManager, bool checkCanWrap)
        {
            Should.NotBeNull(viewInfo, nameof(viewInfo));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            if (wrapperType.IsInstanceOfType(viewInfo.View))
                return viewInfo.View;

            var collection = viewInfo.GetOrAddWrappersCollection();
            lock (collection)
            {
                var item = collection.Get<object>(metadata).FirstOrDefault(wrapperType.IsInstanceOfType);
                if (item == null)
                {
                    wrapperManager = wrapperManager.DefaultIfNull();
                    if (checkCanWrap && !wrapperManager.CanWrap(viewInfo.View.GetType(), wrapperType, metadata))
                        return null;

                    item = wrapperManager.Wrap(viewInfo.View, wrapperType, metadata);
                    collection.Add(item, metadata);
                }

                return item;
            }
        }

        #endregion
    }
}