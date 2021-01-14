using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions.Components
{
    public static class DefaultComponentExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Invalidate(this ItemOrArray<IHasCache> components, object? state, IReadOnlyMetadataContext? metadata)
        {
            foreach (var c in components)
                c.Invalidate(state, metadata);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Dispose(this ItemOrArray<IDisposable> components)
        {
            foreach (var c in components)
                c.Dispose();
        }

        public static bool IsSuspended(this ItemOrArray<ISuspendable> components)
        {
            foreach (var c in components)
            {
                if (c.IsSuspended)
                    return true;
            }

            return false;
        }

        public static ActionToken Suspend(this ItemOrArray<ISuspendable> components, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return components[0].Suspend(state, metadata);

            var tokens = new ActionToken[components.Count];
            for (var i = 0; i < tokens.Length; i++)
                tokens[i] = components[i].Suspend(state, metadata);
            return new ActionToken(tokens);
        }
    }
}