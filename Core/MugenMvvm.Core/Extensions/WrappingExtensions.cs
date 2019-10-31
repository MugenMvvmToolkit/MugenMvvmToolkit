using System;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;
using MugenMvvm.Metadata;
using MugenMvvm.Wrapping.Components;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IWrapperManagerComponent AddWrapper(this IWrapperManager wrapperManager, Func<IWrapperManager, Type, Type, IReadOnlyMetadataContext?, bool> condition,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext?, object?> wrapperFactory, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            var factory = new DelegateWrapperManagerComponent(condition, wrapperFactory);
            wrapperManager.Components.Add(factory, metadata);
            return factory;
        }

        public static IWrapperManagerComponent AddWrapper(this IWrapperManager wrapperManager, Type wrapperType, Type implementation,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext?, object>? wrapperFactory = null, 
            IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            Should.BeOfType(implementation, nameof(implementation), wrapperType);
            if (implementation.IsInterface || implementation.IsAbstract)
                ExceptionManager.ThrowWrapperTypeShouldBeNonAbstract(implementation);

            if (wrapperFactory == null)
            {
                var constructor = implementation
                    .GetConstructors(BindingFlagsEx.InstanceOnly)
                    .FirstOrDefault()
                    ?.GetActivator(reflectionDelegateProvider);
                if (constructor == null)
                    ExceptionManager.ThrowCannotFindConstructor(implementation);
                wrapperFactory = (manager, o, arg3, arg4) => constructor!.Invoke(new[] { o });
            }

            return wrapperManager.AddWrapper((manager, type, arg3, arg4) => wrapperType == arg3, wrapperFactory);//todo closure check
        }

        public static IWrapperManagerComponent AddWrapper<TWrapper>(this IWrapperManager wrapperManager, Type implementation,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext?, TWrapper>? wrapperFactory = null)
            where TWrapper : class
        {
            return wrapperManager.AddWrapper(typeof(TWrapper), implementation, wrapperFactory);
        }

        public static IWrapperManagerComponent AddWrapper<TWrapper, TImplementation>(this IWrapperManager wrapperManager,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext?, TWrapper>? wrapperFactory = null)
            where TWrapper : class
            where TImplementation : class, TWrapper
        {
            return wrapperManager.AddWrapper(typeof(TWrapper), typeof(TImplementation), wrapperFactory);
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
            return wrapperType.IsInstanceOfType(viewInfo.View) || wrapperManager.ServiceIfNull().CanWrap(viewInfo.View.GetType(), wrapperType, metadata);
        }

        public static IComponentCollection<object> GetOrAddWrappersCollection(this IViewInfo viewInfo, IComponentCollectionProvider? componentCollectionProvider = null)
        {
            return viewInfo
                .Metadata
                .GetOrAdd(ViewMetadata.Wrappers, viewInfo, componentCollectionProvider, (context, v, p) => p.ServiceIfNull().GetComponentCollection<object>(v, context))!;
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
                var item = collection.GetItems().FirstOrDefault(wrapperType.IsInstanceOfType);
                if (item == null)
                {
                    wrapperManager = wrapperManager.ServiceIfNull();
                    if (checkCanWrap && !wrapperManager.CanWrap(viewInfo.View.GetType(), wrapperType, metadata))
                        return null;

                    item = wrapperManager.Wrap(viewInfo.View, wrapperType, metadata);
                    collection.Add(item);
                }

                return item;
            }
        }

        #endregion
    }
}