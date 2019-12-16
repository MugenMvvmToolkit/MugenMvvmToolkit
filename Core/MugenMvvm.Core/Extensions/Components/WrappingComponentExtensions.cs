using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class WrappingComponentExtensions
    {
        #region Methods

        public static bool CanWrap(this IWrapperManagerComponent[] components, Type targetType, Type wrapperType, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(targetType, nameof(targetType));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].CanWrap(targetType, wrapperType, metadata))
                    return true;
            }

            return false;
        }


        public static object? TryWrap(this IWrapperManagerComponent[] components, object target, Type wrapperType, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            for (var i = 0; i < components.Length; i++)
            {
                var wrapper = components[i].TryWrap(target, wrapperType, metadata);
                if (wrapper != null)
                    return wrapper;
            }

            return null;
        }

        public static void OnWrapped(this IWrapperManagerListener[] listeners, IWrapperManager wrapperManager, object wrapper, object item, Type wrapperType, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(wrapper, nameof(wrapper));
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnWrapped(wrapperManager, wrapper!, item, wrapperType, metadata);
        }

        #endregion
    }
}