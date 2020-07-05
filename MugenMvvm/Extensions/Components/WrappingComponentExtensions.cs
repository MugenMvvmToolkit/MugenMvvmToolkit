using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class WrappingComponentExtensions
    {
        #region Methods

        public static bool CanWrap<TRequest>(this IWrapperManagerComponent[] components, IWrapperManager wrapperManager, Type wrapperType, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].CanWrap(wrapperManager, wrapperType, request, metadata))
                    return true;
            }

            return false;
        }


        public static object? TryWrap<TRequest>(this IWrapperManagerComponent[] components, IWrapperManager wrapperManager, Type wrapperType, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            for (var i = 0; i < components.Length; i++)
            {
                var wrapper = components[i].TryWrap(wrapperManager, wrapperType, request, metadata);
                if (wrapper != null)
                    return wrapper;
            }

            return null;
        }

        public static void OnWrapped<TRequest>(this IWrapperManagerListener[] listeners, IWrapperManager wrapperManager, object wrapper, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(wrapper, nameof(wrapper));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnWrapped(wrapperManager, wrapper, request, metadata);
        }

        #endregion
    }
}