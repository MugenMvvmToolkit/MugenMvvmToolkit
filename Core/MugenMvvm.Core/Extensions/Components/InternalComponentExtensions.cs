using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class InternalComponentExtensions
    {
        #region Methods

        public static bool TryGetOrAddAttachedValueProvider(this IAttachedValueManagerComponent[] components, object item, IReadOnlyMetadataContext? metadata, out IAttachedValueProvider? provider)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryGetOrAddAttachedValueProvider(item, metadata, out provider))
                    return true;
            }

            provider = null;
            return false;
        }

        public static bool TryGetAttachedValueProvider(this IAttachedValueManagerComponent[] components, object item, IReadOnlyMetadataContext? metadata, out IAttachedValueProvider? provider)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryGetAttachedValueProvider(item, metadata, out provider))
                    return true;
            }

            provider = null;
            return false;
        }

        public static bool CanTrace(this ITracerComponent[] components, TraceLevel level, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].CanTrace(level, metadata))
                    return true;
            }

            return false;
        }

        public static void Trace(this ITracerComponent[] components, TraceLevel level, string message, Exception? exception, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
                components[i].Trace(level, message, exception, metadata);
        }

        #endregion
    }
}