using System.Runtime.CompilerServices;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class ThreadingComponentExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanExecuteInline(this ItemOrArray<IThreadDispatcherComponent> components, IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            Should.NotBeNull(executionMode, nameof(executionMode));
            foreach (var c in components)
            {
                if (c.CanExecuteInline(threadDispatcher, executionMode, metadata))
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryExecute(this ItemOrArray<IThreadDispatcherComponent> components, IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode,
            object handler, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            Should.NotBeNull(executionMode, nameof(executionMode));
            Should.NotBeNull(handler, nameof(handler));
            foreach (var c in components)
            {
                if (c.TryExecute(threadDispatcher, executionMode, handler, state, metadata))
                    return true;
            }

            return false;
        }
    }
}