using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface ICommandProviderComponent : IComponent<ICommandManager>
    {
        ICompositeCommand? TryGetCommand<TRequest>([DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}