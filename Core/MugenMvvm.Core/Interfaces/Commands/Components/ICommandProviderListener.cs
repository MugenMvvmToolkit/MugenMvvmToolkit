using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface ICommandProviderListener : IComponent<ICommandProvider>
    {
        void OnCommandCreated<TRequest>(ICommandProvider provider, [DisallowNull]in TRequest request, ICompositeCommand command, IReadOnlyMetadataContext? metadata);
    }
}