using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.IoC;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static bool LazyInitialize<T>(this IComponentCollectionProvider provider, [EnsuresNotNull] ref IComponentCollection<T>? item, object target,
            IReadOnlyMetadataContext? metadata = null)
            where T : class //todo R# bug return?
        {
            return item == null && LazyInitialize(ref item, provider.ServiceIfNull().GetComponentCollection<T>(target, metadata.DefaultIfNull()));
        }

        public static bool LazyInitialize(this IMetadataContextProvider provider, [EnsuresNotNull] ref IMetadataContext? metadataContext,
            object? target, IEnumerable<MetadataContextValue> values = null) //todo R# bug return?
        {
            return metadataContext == null && LazyInitialize(ref metadataContext, GetMetadataContext(target, values, provider));
        }

        public static bool LazyInitialize(this IMetadataContextProvider provider, [EnsuresNotNull] ref IObservableMetadataContext? metadataContext,
            object? target, IEnumerable<MetadataContextValue> values = null) //todo R# bug return?
        {
            return metadataContext == null && LazyInitialize(ref metadataContext, GetObservableMetadataContext(target, values, provider));
        }

        public static T[] GetItemsOrDefault<T>(this IComponentCollection<T> componentCollection) where T : class //todo R# bug return?
        {
            return componentCollection?.GetItems() ?? Default.EmptyArray<T>();
        }

        [StringFormatMethod("format")]
        public static string Format(this string format, params object?[] args)
        {
            return string.Format(format, args);
        }

        public static void AddListener<T>(this IHasListeners<T> hasListeners, T listener, IReadOnlyMetadataContext? metadata = null) where T : class, IListener
        {
            Should.NotBeNull(hasListeners, nameof(hasListeners));
            hasListeners.Listeners.Add(listener, metadata);
        }

        public static void RemoveListener<T>(this IHasListeners<T> hasListeners, T listener, IReadOnlyMetadataContext? metadata = null) where T : class, IListener
        {
            Should.NotBeNull(hasListeners, nameof(hasListeners));
            hasListeners.Listeners.Remove(listener, metadata);
        }

        public static void RemoveAllListeners<T>(this IHasListeners<T> hasListeners, IReadOnlyMetadataContext? metadata = null) where T : class, IListener
        {
            Should.NotBeNull(hasListeners, nameof(hasListeners));
            hasListeners.Listeners.Clear(metadata);
        }

        [Pure]
        public static T GetService<T>(this IServiceProvider serviceProvider)
        {
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            return (T)serviceProvider.GetService(typeof(T));
        }

        [Pure]
        public static bool TryGetService<T>(this IServiceProvider serviceProvider, [NotNullWhenTrue] out T service) where T : class
        {
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            try
            {
                if (serviceProvider is IServiceProviderEx serviceProviderEx)
                {
                    if (serviceProviderEx.TryGetService(typeof(T), out var o))
                    {
                        service = (T)o!;
                        return true;
                    }

                    service = default!;
                    return false;
                }

                service = (T)serviceProvider.GetService(typeof(T));
                return true;
            }
            catch
            {
                service = default!;
                return false;
            }
        }

        public static IWeakReference GetWeakReference(object? item, IReadOnlyMetadataContext? metadata = null, IWeakReferenceProvider? provider = null)
        {
            if (item == null)
                return Default.WeakReference;
            return provider.ServiceIfNull().GetWeakReference(item, metadata);
        }

        //note for better performance use this method for creating delegate instead of handler.Execute because it will use ldftn opcode instead of ldvirtftn       
        public static void ExecuteDelegate(this IThreadDispatcherHandler handler, object? state)
        {
            handler.Execute(state);
        }

        //note for better performance use this method for creating delegate if state parameter is null
        public static void ExecuteNullState(this IThreadDispatcherHandler handler)
        {
            handler.Execute(null);
        }

        [Pure]
        public static bool HasMemberFlag(this MemberFlags es, MemberFlags value)
        {
            return (es & value) == value;
        }

        [Pure]
        public static bool HasFlagEx(this BatchUpdateCollectionMode mode, BatchUpdateCollectionMode value)
        {
            return (mode & value) == value;
        }

        [Pure]
        public static bool HasFlagEx(this BusyMessageHandlerType handlerMode, BusyMessageHandlerType value)
        {
            return (handlerMode & value) == value;
        }

        #endregion
    }
}