using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface ICommandProviderListener : IComponent<ICommandManager>
    {
        void OnCommandCreated<TRequest>(ICommandManager provider, ICompositeCommand command, [DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}