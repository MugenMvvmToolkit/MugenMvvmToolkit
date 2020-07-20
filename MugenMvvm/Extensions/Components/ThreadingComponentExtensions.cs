using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class ThreadingComponentExtensions
    {
        #region Methods

        public static bool CanExecuteInline(this IThreadDispatcherComponent[] components, IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            Should.NotBeNull(executionMode, nameof(executionMode));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].CanExecuteInline(threadDispatcher, executionMode, metadata))
                    return true;
            }

            return false;
        }

        public static bool TryExecute(this IThreadDispatcherComponent[] components, IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, object handler, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            Should.NotBeNull(executionMode, nameof(executionMode));
            Should.NotBeNull(handler, nameof(handler));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryExecute(threadDispatcher, executionMode, handler, state, metadata))
                    return true;
            }

            return false;
        }

        #endregion
    }
}