using System;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Extensions.Components
{
    public static class DefaultComponentExtensions
    {
        #region Methods

        public static void Invalidate<TState>(this IHasCache[] components, in TState state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
                components[i].Invalidate(state, metadata);
        }

        public static void Dispose(object? components)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IDisposable)?.Dispose();
            }
            else
                (components as IDisposable)?.Dispose();
        }

        public static void Dispose(this IDisposable[] components)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
                components[i].Dispose();
        }

        public static bool IsSuspended(this ISuspendable[] components)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].IsSuspended)
                    return true;
            }

            return false;
        }

        public static ActionToken Suspend<TState>(this ISuspendable[] components, in TState state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 0)
                return default;
            if (components.Length == 1)
                return components[0].Suspend(state, metadata);

            var tokens = new ActionToken[components.Length];
            for (var i = 0; i < components.Length; i++)
                tokens[i] = components[i].Suspend(state, metadata);

            return new ActionToken((o, _) =>
            {
                var list = (ActionToken[])o!;
                for (var i = 0; i < list.Length; i++)
                    list[i].Dispose();
            }, tokens);
        }

        #endregion
    }
}