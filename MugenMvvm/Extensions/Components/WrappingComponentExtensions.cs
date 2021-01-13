using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class WrappingComponentExtensions
    {
        #region Methods

        public static bool CanWrap(this ItemOrArray<IWrapperManagerComponent> components, IWrapperManager wrapperManager, Type wrapperType, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            Should.NotBeNull(request, nameof(request));
            foreach (var c in components)
            {
                if (c.CanWrap(wrapperManager, wrapperType, request, metadata))
                    return true;
            }

            return false;
        }

        public static object? TryWrap(this ItemOrArray<IWrapperManagerComponent> components, IWrapperManager wrapperManager, Type wrapperType, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            Should.NotBeNull(request, nameof(request));
            foreach (var c in components)
            {
                var wrapper = c.TryWrap(wrapperManager, wrapperType, request, metadata);
                if (wrapper != null)
                    return wrapper;
            }

            return null;
        }

        public static void OnWrapped(this ItemOrArray<IWrapperManagerListener> listeners, IWrapperManager wrapperManager, object wrapper, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(wrapper, nameof(wrapper));
            Should.NotBeNull(request, nameof(request));
            foreach (var c in listeners)
                c.OnWrapped(wrapperManager, wrapper, request, metadata);
        }

        #endregion
    }
}