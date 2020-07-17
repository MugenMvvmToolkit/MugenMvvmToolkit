using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class WrappingComponentExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanWrap(this IWrapperManagerComponent[] components, IWrapperManager wrapperManager, Type wrapperType, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            Should.NotBeNull(request, nameof(request));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].CanWrap(wrapperManager, wrapperType, request, metadata))
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? TryWrap(this IWrapperManagerComponent[] components, IWrapperManager wrapperManager, Type wrapperType, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            Should.NotBeNull(request, nameof(request));
            for (var i = 0; i < components.Length; i++)
            {
                var wrapper = components[i].TryWrap(wrapperManager, wrapperType, request, metadata);
                if (wrapper != null)
                    return wrapper;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnWrapped(this IWrapperManagerListener[] listeners, IWrapperManager wrapperManager, object wrapper, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(wrapper, nameof(wrapper));
            Should.NotBeNull(request, nameof(request));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnWrapped(wrapperManager, wrapper, request, metadata);
        }

        #endregion
    }
}