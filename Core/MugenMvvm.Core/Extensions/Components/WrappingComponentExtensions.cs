using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class WrappingComponentExtensions
    {
        #region Methods

        public static bool CanWrap(this IWrapperManagerComponent[] components, Type type, Type wrapperType, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].CanWrap(type, wrapperType, metadata))
                    return true;
            }

            return false;
        }


        public static object? TryWrap(this IWrapperManagerComponent[] components, object target, Type wrapperType, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
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
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnWrapped(wrapperManager, wrapper!, item, wrapperType, metadata);
        }

        #endregion
    }
}