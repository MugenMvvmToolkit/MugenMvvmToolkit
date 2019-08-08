using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.IoC;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static Task<IReadOnlyMetadataContext> CleanupAsync(this IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewInfo, nameof(viewInfo));
            return viewInfo.Initializer.CleanupAsync(viewInfo, viewModel, metadata);
        }

        public static TType? GetComponent<T, TType>(this IComponentOwner<T> owner, bool optional)
            where T : class
            where TType : class, IComponent<T>
        {
            Should.NotBeNull(owner, nameof(owner));
            var components = owner.GetComponents();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] is TType component)
                    return component;
            }

            if (!optional)
                ExceptionManager.ThrowCannotGetComponent(owner, typeof(T));
            return null;
        }

        public static bool MemberNameEqual(string changedMember, string listenedMember, bool emptyListenedMemberResult = false)
        {
            if (string.Equals(changedMember, listenedMember) || string.IsNullOrEmpty(changedMember))
                return true;
            if (string.IsNullOrEmpty(listenedMember))
                return emptyListenedMemberResult;

            if (listenedMember[0] == '[')
            {
                if (Default.IndexerName.Equals(changedMember))
                    return true;
                if (changedMember.StartsWith("Item[", StringComparison.Ordinal))
                {
                    int i = 4, j = 0;
                    while (i < changedMember.Length)
                    {
                        if (j >= listenedMember.Length)
                            return false;
                        var c1 = changedMember[i];
                        var c2 = listenedMember[j];
                        if (c1 == c2)
                        {
                            ++i;
                            ++j;
                        }
                        else if (c1 == '"')
                            ++i;
                        else if (c2 == '"')
                            ++j;
                        else
                            return false;
                    }

                    return j == listenedMember.Length;
                }
            }

            return false;
        }

        public static bool LazyInitialize<T>(this IComponentCollectionProvider? provider, [EnsuresNotNull] ref IComponentCollection<T>? item, object target,
            IReadOnlyMetadataContext? metadata = null)
            where T : class
        {
            return item == null && LazyInitialize(ref item, provider.ServiceIfNull().GetComponentCollection<T>(target, metadata));
        }

        public static bool LazyInitialize(this IMetadataContextProvider? provider, [EnsuresNotNull] ref IMetadataContext? metadataContext,
            object? target, IEnumerable<MetadataContextValue>? values = null)
        {
            return metadataContext == null && LazyInitialize(ref metadataContext, GetMetadataContext(target, values, provider));
        }

        public static T[] GetItemsOrDefault<T>(this IComponentCollection<T>? componentCollection) where T : class
        {
            return componentCollection?.GetItems() ?? Default.EmptyArray<T>();
        }

        [StringFormatMethod("format")]
        public static string Format(this string format, params object?[] args)
        {
            return string.Format(format, args);
        }

        public static void AddComponent<T>(this IComponentOwner<T> componentOwner, IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            componentOwner.Components.Add(component, metadata);
        }

        public static void RemoveComponent<T>(this IComponentOwner<T> componentOwner, IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            if (componentOwner.HasComponents)
                componentOwner.Components.Remove(component, metadata);
        }

        public static void ClearComponents<T>(this IComponentOwner<T> componentOwner, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            if (componentOwner.HasComponents)
                componentOwner.Components.Clear(metadata);
        }

        public static IComponent<T>[] GetComponents<T>(this IComponentOwner<T> componentOwner) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            if (componentOwner.HasComponents)
                return componentOwner.Components.GetItems();
            return Default.EmptyArray<IComponent<T>>();
        }

        [Pure]
        public static T GetService<T>(this IServiceProvider serviceProvider)
        {
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            return (T)serviceProvider.GetService(typeof(T));
        }

        [Pure]
        public static bool TryGetService<T>(this IServiceProvider serviceProvider, out T service)
        {
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            try
            {
                if (serviceProvider is IIocContainer container)
                {
                    if (container.TryGet(typeof(T), out var o))
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

        public static bool TryGet<T>(this IIocContainer iocContainer, [NotNullWhenTrue] out T service, IReadOnlyMetadataContext? metadata = null)
        {
            var tryGet = iocContainer.TryGet(typeof(T), out var objService, metadata);
            if (tryGet)
            {
                service = (T)objService!;
                return true;
            }

            service = default!;
            return false;
        }

        public static bool TryGet(this IIocContainer iocContainer, Type serviceType, [NotNullWhenTrue] out object? service, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            Should.NotBeNull(serviceType, nameof(serviceType));
            if (iocContainer.CanResolve(serviceType, metadata))
            {
                try
                {
                    service = iocContainer.Get(serviceType, metadata);
                    return true;
                }
                catch
                {
                    service = null;
                    return false;
                }
            }

            service = null;
            return false;
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