using System.Diagnostics.CodeAnalysis;
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
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IThreadDispatcherComponent[]? _components;

        #endregion

        #region Constructors

        public ThreadDispatcher(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IThreadDispatcherComponent, ThreadDispatcher>((components, state, _) => state._components = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public bool CanExecuteInline(ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata = null)
        {
            if (_components == null)
                _componentTracker.Attach(this, metadata);
            return _components!.CanExecuteInline(executionMode, metadata);
        }

        public bool TryExecute<THandler, TState>(ThreadExecutionMode executionMode, [DisallowNull] in THandler handler, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            if (_components == null)
                _componentTracker.Attach(this, metadata);
            return _components!.TryExecute(executionMode, handler, state, metadata);
        }

        #endregion
    }
}