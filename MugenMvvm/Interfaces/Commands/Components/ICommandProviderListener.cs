using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface ICommandProviderListener : IComponent<ICommandProvider>
    {
        void OnCommandCreated<TRequest>(ICommandProvider provider, ICompositeCommand command, [DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}