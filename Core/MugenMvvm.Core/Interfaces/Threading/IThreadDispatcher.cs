using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Threading
{
    public interface IThreadDispatcher : IComponentOwner<IThreadDispatcher>, IComponent<IMugenApplication>
    {
        bool CanExecuteInline(ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata = null);

        bool TryExecute<THandler, TState>(ThreadExecutionMode executionMode, [DisallowNull]in THandler handler, in TState state, IReadOnlyMetadataContext? metadata = null);
    }
}