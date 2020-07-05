using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Threading.Components
{
    public interface IThreadDispatcherComponent : IComponent<IThreadDispatcher>
    {
        bool CanExecuteInline(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata);

        bool TryExecute<THandler, TState>(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, [DisallowNull] in THandler handler, in TState state, IReadOnlyMetadataContext? metadata);
    }
}