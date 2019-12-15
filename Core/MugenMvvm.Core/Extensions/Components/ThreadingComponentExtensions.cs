using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class ThreadingComponentExtensions
    {
        #region Methods

        public static bool CanExecuteInline(this IThreadDispatcherComponent[] components, ThreadExecutionMode executionMode)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(executionMode, nameof(executionMode));
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].CanExecuteInline(executionMode))
                    return true;
            }

            return false;
        }

        public static bool TryExecute<TState>(this IThreadDispatcherComponent[] components, ThreadExecutionMode executionMode, IThreadDispatcherHandler<TState> handler, TState state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(executionMode, nameof(executionMode));
            Should.NotBeNull(handler, nameof(handler));
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].TryExecute(executionMode, handler, state, metadata))
                    return true;
            }

            return false;
        }

        public static bool TryExecute<TState>(this IThreadDispatcherComponent[] components, ThreadExecutionMode executionMode, Action<TState> handler, TState state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(executionMode, nameof(executionMode));
            Should.NotBeNull(handler, nameof(handler));
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].TryExecute(executionMode, handler, state, metadata))
                    return true;
            }

            return false;
        }

        #endregion
    }
}