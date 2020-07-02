using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands
{
    public interface ICommandManager : IComponentOwner<ICommandManager>
    {
        ICompositeCommand? TryGetCommand<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null);
    }
}