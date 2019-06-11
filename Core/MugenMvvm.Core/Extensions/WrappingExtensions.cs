using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Wrapping;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views.Infrastructure;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IChildWrapperManager AddWrapper(this IWrapperManager wrapperManager, Func<IWrapperManager, Type, Type, IReadOnlyMetadataContext, bool> condition,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext, object?> wrapperFactory, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            var factory = new DelegateChildWrapperManager(condition, wrapperFactory);
            wrapperManager.Managers.Add(factory, metadata);
            return factory;
        }

        public static IChildWrapperManager AddWrapper(this IWrapperManager wrapperManager, Type wrapperType, Type implementation,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext, object>? wrapperFactory = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            Should.BeOfType(implementation, nameof(implementation), wrapperType);
            if (implementation.IsInterfaceUnified() || implementation.IsAbstractUnified())
                ExceptionManager.ThrowWrapperTypeShouldBeNonAbstract(implementation);

            if (wrapperFactory == null)
            {
                var constructor = implementation
                    .GetConstructorsUnified(MemberFlags.InstanceOnly)
                    .FirstOrDefault();
                if (constructor == null)
                    ExceptionManager.ThrowCannotFindConstructor(implementation);

                wrapperFactory = (manager, o, arg3, arg4) => constructor.InvokeEx(o);
            }

            return wrapperManager.AddWrapper((manager, type, arg3, arg4) => wrapperType.EqualsEx(arg3), wrapperFactory);
        }

        public static IChildWrapperManager AddWrapper<TWrapper>(this IWrapperManager wrapperManager, Type implementation,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext, TWrapper>? wrapperFactory = null)
            where TWrapper : class
        {
            return wrapperManager.AddWrapper(typeof(TWrapper), implementation, wrapperFactory);
        }

        public static IChildWrapperManager AddWrapper<TWrapper, TImplementation>(this IWrapperManager wrapperManager,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext, TWrapper>? wrapperFactory = null)
            where TWrapper : class
            where TImplementation : class, TWrapper
        {
            return wrapperManager.AddWrapper(typeof(TWrapper), typeof(TImplementation), wrapperFactory);
        }

        public static TView? TryWrap<TView>(this IViewInfo viewInfo, IReadOnlyMetadataContext? metadata = null, IWrapperManager wrapperManager = null)
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
            if (metadata == null)
                metadata = Default.Metadata;
            return wrapperType.IsInstanceOfTypeUnified(viewInfo.View) || wrapperManager.ServiceIfNull().CanWrap(viewInfo.View.GetType(), wrapperType, metadata);
        }

        public static IComponentCollection<object> GetOrAddWrappersCollection(this IViewInfo viewInfo, IComponentCollectionProvider? provider = null)
        {
            return viewInfo
                .Metadata
                .GetOrAdd(ViewMetadata.Wrappers, viewInfo, provider, (context, v, p) => p.ServiceIfNull().GetComponentCollection<object>(v, context));
        }

        private static object? WrapInternal(this IViewInfo viewInfo, Type wrapperType, IReadOnlyMetadataContext? metadata, IWrapperManager? wrapperManager, bool checkCanWrap)
        {
            Should.NotBeNull(viewInfo, nameof(viewInfo));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            if (wrapperType.IsInstanceOfTypeUnified(viewInfo.View))
                return viewInfo.View;

            if (metadata == null)
                metadata = Default.Metadata;

            var collection = viewInfo.GetOrAddWrappersCollection();
            lock (collection)
            {
                var item = collection.GetItems().FirstOrDefault(wrapperType.IsInstanceOfTypeUnified);
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