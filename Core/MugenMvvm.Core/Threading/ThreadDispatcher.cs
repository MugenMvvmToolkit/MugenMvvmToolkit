using System;
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

        public bool CanExecuteInline(ThreadExecutionMode executionMode)
        {
            return GetComponents<IThreadDispatcherComponent>().CanExecuteInline(executionMode);
        }

        public void Execute<TState>(ThreadExecutionMode executionMode, IThreadDispatcherHandler<TState> handler, TState state = default, IReadOnlyMetadataContext? metadata = null)
        {
            if (!GetComponents<IThreadDispatcherComponent>().TryExecute(executionMode, handler, state, metadata))
                ExceptionManager.ThrowObjectNotInitialized(this);
        }

        public void Execute<TState>(ThreadExecutionMode executionMode, Action<TState> handler, TState state = default, IReadOnlyMetadataContext? metadata = null)
        {
            if (!GetComponents<IThreadDispatcherComponent>().TryExecute(executionMode, handler, state, metadata))
                ExceptionManager.ThrowObjectNotInitialized(this);
        }

        #endregion
    }
}