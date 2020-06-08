using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;

namespace MugenMvvm.Threading
{
    public sealed class ThreadDispatcher : ComponentOwnerBase<IThreadDispatcher>, IThreadDispatcher
    {
        #region Constructors

        public ThreadDispatcher(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public bool CanExecuteInline(ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IThreadDispatcherComponent>().CanExecuteInline(executionMode, metadata);
        }

        public void Execute<TState>(ThreadExecutionMode executionMode, object handler, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            if (!GetComponents<IThreadDispatcherComponent>().TryExecute(executionMode, handler, state, metadata))
                ExceptionManager.ThrowObjectNotInitialized(this);
        }

        #endregion
    }
}