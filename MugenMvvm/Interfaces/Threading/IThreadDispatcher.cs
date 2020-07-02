using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Threading
{
    public interface IThreadDispatcher : IComponentOwner<IThreadDispatcher>
    {
        bool CanExecuteInline(ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata = null);

        bool TryExecute<THandler, TState>(ThreadExecutionMode executionMode, [DisallowNull] in THandler handler, in TState state, IReadOnlyMetadataContext? metadata = null);
    }
}